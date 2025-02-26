/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2023 Edgar Lima (RobotoSkunk) <contact@robotoskunk.com>

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System;
using System.Threading.Tasks;

using ClockBombGames.CircleBeats.Analyzers;

using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class EditorTimeline : Control
	{
		[Signal]
		public delegate void SliderEventHandler(float min, float max);

		[Export] Editor editor;

		#region Timeline variables
		[ExportGroup("Timeline")]

		[ExportSubgroup("Header")]
		[Export] Button skipBackwardButton;
		[Export] Button playButton;
		[Export] Label songTimeLabel;
		[Export] TimelineSlider timelineSlider;

		[ExportSubgroup("Body")]
		[Export] TextureRect waveformTextureRect;
		[Export] Control timelineContent;
		[Export] ColorRect timelineSeeker;

		[ExportSubgroup("Footer")]
		[Export] Label infoLabel;
		[Export] TimelineHorizontalScroll horizontalScroll;
		[Export] ColorRect horizontalScrollSeeker;
		#endregion

		#region Resources variables
		[ExportGroup("Resources")]

		[ExportSubgroup("Play Button")]
		[Export] CompressedTexture2D playSprite;
		[Export] CompressedTexture2D pauseSprite;

		[ExportSubgroup("Timers")]
		[Export] Timer timerWaveformSync;
		#endregion

		public event SliderEventHandler OnSliderChange = delegate { };


		Image waveformImage;
		readonly ImageTexture waveformImageTexture = new();


		AudioStreamPlayer musicPlayer;
		readonly MP3Reader mp3Reader = new();

		private Action skipBackwardAction;

		Vector2 waveformSize;

		double songPosition = 0f;
		double songLength = 0f;
		double pausedPlaybackBuffer;
		double updateWaveformBuffer;

		bool isPlaying = false;
		bool canUpdateWaveform = true;

		float Zoom
		{
			get {
				return horizontalScroll.MaxValue - horizontalScroll.MinValue;
			}
		}


		public override void _Ready()
		{
			// Load music info
			musicPlayer = editor.Playground.MusicPlayer;
			songLength = musicPlayer.Stream.GetLength();

			// Do first calls
			UpdateTimelineSlider();

			// Read mp3 stream
			if (musicPlayer.Stream is AudioStreamMP3 audioStream) {
				mp3Reader.ReadAudioStream(audioStream);

				infoLabel.Text = "Reading audio samples...";

				Task.Run(async () =>
				{
					try {
						await mp3Reader.ReadWaveformData();
					} catch (Exception e) {
						GD.PrintErr(e.Message);
						GD.PrintErr(e.StackTrace);

						Callable.From(() => infoLabel.Text = "Something went wrong...").CallDeferred();

						return;
					}

					Callable.From(() => infoLabel.Text = "").CallDeferred();
				});
			} else {
				GD.PrintErr("Only MP3 files are allowed.");
			}
		}

		public override void _EnterTree()
		{
			skipBackwardAction = () => SeekMusicPosition(0);

			// Set Controls listeners
			skipBackwardButton.Pressed += skipBackwardAction;
			playButton.Pressed += OnPlayPressed;

			timelineSlider.OnValueChange += SeekMusicPosition;
			horizontalScroll.OnDragging += UpdateTimelineSlider;
			timerWaveformSync.Timeout += WaveformSyncLoop;
		}


		public override void _ExitTree()
		{
			skipBackwardButton.Pressed -= skipBackwardAction;
			playButton.Pressed -= OnPlayPressed;

			timelineSlider.OnValueChange -= SeekMusicPosition;
			horizontalScroll.OnDragging -= UpdateTimelineSlider;
			timerWaveformSync.Timeout -= WaveformSyncLoop;
		}


		public override void _Process(double delta)
		{
			if (isPlaying) {
				songPosition = musicPlayer.GetPlaybackPosition();

				timelineSlider.SetValue((float)songPosition);

			} else if (pausedPlaybackBuffer > 0f) {
				pausedPlaybackBuffer -= delta;

			} else if (musicPlayer.Playing) {
				musicPlayer.Playing = false;
				editor.Playground.IsPlaying = false;
			}


			// Song time label
			songTimeLabel.Text = string.Concat(ParseSeconds(songPosition), " / ", ParseSeconds(songLength));

			// Timeline Seeker
			Vector2 seekerPosition = timelineSeeker.GlobalPosition;

			seekerPosition.Y = timelineSlider.GlobalPosition.Y;
			seekerPosition.X = timelineSlider.GlobalPosition.X + timelineSlider.HandlerPosition.X;

			timelineSeeker.GlobalPosition = seekerPosition;

			// Horizontal Scroll Seeker
			seekerPosition = horizontalScrollSeeker.GlobalPosition;

			float relativeMusicPos = (float)(songPosition / songLength);
			seekerPosition.X = horizontalScroll.GlobalPosition.X + horizontalScroll.Size.X * relativeMusicPos;

			horizontalScrollSeeker.GlobalPosition = seekerPosition;


			// Waveform renderer
			Vector2 currentWaveformSize = waveformTextureRect.Size;

			// Check for any changes to update the waveform
			if (mp3Reader.Ready && canUpdateWaveform && waveformSize != currentWaveformSize) {
				waveformSize = currentWaveformSize;

				UpdateWaveform();
			}
		}


		static string ParseSeconds(double time)
		{
			int seconds = (int)time % 60;
			int minutes = (int)time / 60;
			int decimals = (int)(time % 1 * 100);

			return string.Concat(minutes.ToString("00"), ":", seconds.ToString("00"), ".", decimals.ToString("00"));
		}


		void OnPlayPressed()
		{
			isPlaying = !isPlaying;

			musicPlayer.Playing = isPlaying;
			editor.Playground.IsPlaying = isPlaying;

			if (musicPlayer.Playing) {
				playButton.Icon = pauseSprite;

				musicPlayer.Seek((float)songPosition);
				return;
			}

			playButton.Icon = playSprite;
		}

		void SeekMusicPosition(float delta)
		{
			songPosition = delta;

			if (!isPlaying && songPosition < songLength - 0.1f) {
				pausedPlaybackBuffer = 0.1f;

				musicPlayer.Playing = true;
				editor.Playground.IsPlaying = true;
			}

			musicPlayer.Seek((float)songPosition);
		}


		void UpdateTimelineSlider()
		{
			timelineSlider.MinValue = (float)(songLength * horizontalScroll.MinValue);
			timelineSlider.MaxValue = (float)(songLength * horizontalScroll.MaxValue);

			OnSliderChange.Invoke(timelineSlider.MinValue, timelineSlider.MaxValue);


			// Update waveform lazily
			if (!mp3Reader.Ready) {
				return;
			}

			UpdateWaveform();

			timerWaveformSync.Stop();
			timerWaveformSync.Start();
		}

		void WaveformSyncLoop()
		{
			// Wait until the current waveform update thread finishes
			if (!canUpdateWaveform) {
				timerWaveformSync.Start();
				return;
			}

			UpdateWaveform();
		}

		void UpdateWaveform()
		{
			if (!canUpdateWaveform) {
				return;
			}

			if (waveformSize.IsZeroApprox()) {
				waveformSize = waveformTextureRect.Size;
			}


			Vector2I size = new((int)waveformSize.X, (int)waveformSize.Y);

			if (waveformImage == null || waveformImage.GetSize() != size) {
				waveformImage = Image.CreateEmpty(size.X, size.Y, false, Image.Format.Rgba8);

				waveformImageTexture.SetImage(waveformImage);
				waveformTextureRect.Texture ??= waveformImageTexture;
			}

			canUpdateWaveform = false;

			Task.Run(async () =>
			{
				int minIndex = Mathf.RoundToInt(mp3Reader.SampleRate * timelineSlider.MinValue);
				int maxIndex = Mathf.RoundToInt(mp3Reader.SampleRate * timelineSlider.MaxValue);

				try {
					await mp3Reader.RenderWaveformImage(
						minIndex, maxIndex,
						waveformImage,
						Colors.Transparent, new Color(1f, 1f, 1f, 0.3f)
					);

					Callable.From(() =>
					{
						waveformImageTexture.Update(waveformImage);
						canUpdateWaveform = true;
					}).CallDeferred();

				} catch (Exception e) {
					GD.PrintErr(e.Message);
					GD.PrintErr(e.StackTrace);

					canUpdateWaveform = true;
				}
			});
		}
	}
}
