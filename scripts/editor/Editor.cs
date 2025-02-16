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


using System.Threading.Tasks;
using ClockBombGames.CircleBeats.Analyzers;
using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class Editor : Control
	{
		[Export] Playground playground;

		[ExportGroup("UI Components")]
		[Export] TimelineSlider timelineSlider;
		[Export] Label songTimeLabel;
		[Export] ColorRect timelineSeeker;

		[ExportSubgroup("Waveform")]
		[Export] Control waveformWidthRef;
		[Export] Control waveformHeightRef;
		[Export] TextureRect waveformRect;

		[ExportSubgroup("Split Containers")]
		[Export] HSplitContainer timelineHeader;
		[Export] HSplitContainer timelineBody;

		[ExportSubgroup("Playtest Controls")]
		[Export] Button skipBackwardButton;
		[Export] Button playButton;

		[ExportGroup("Resources")]
		[ExportSubgroup("Play Button")]
		[Export] CompressedTexture2D playSprite;
		[Export] CompressedTexture2D pauseSprite;


		AudioStreamPlayer musicPlayer;
		MP3Reader mp3Reader = new();

		double songPosition = 0f;
		double songLength = 0f;
		double pausedPlaybackBuffer;

		bool isPlaying = false;
		bool finishedReadingMp3 = false;
		bool renderedWaveform = false;
		bool renderWaveformImage = false;

		float lastWaveformRatio = 0f;


		public override void _Ready()
		{
			// Load music info
			musicPlayer = playground.MusicPlayer;
			songLength = musicPlayer.Stream.GetLength();

			// Set Controls listeners
			skipBackwardButton.Pressed += () => SeekMusicPosition(0);
			playButton.Pressed += OnPlayPressed;

			timelineSlider.OnValueChange += SeekMusicPosition;

			// Read mp3 stream
			if (musicPlayer.Stream is AudioStreamMP3 audioStream) {
				mp3Reader.ReadAudioStream(audioStream);

				Task.Run(async () =>
				{
					await mp3Reader.ReadWaveformData();
					finishedReadingMp3 = true;
				});
			} else {
				GD.PrintErr("Only MP3 files are allowed.");
			}

			waveformRect.Texture = mp3Reader.WaveformImageTexture;

		}

		public override void _Process(double delta)
		{
			timelineHeader.SplitOffset = timelineBody.SplitOffset;

			if (isPlaying) {
				songPosition = musicPlayer.GetPlaybackPosition();

				timelineSlider.SetValue((float)(songPosition / songLength));

			} else if (pausedPlaybackBuffer > 0f) {
				pausedPlaybackBuffer -= delta;

			} else if (musicPlayer.Playing) {
				musicPlayer.Playing = false;
				playground.IsPlaying = false;
			}


			// Song time label
			songTimeLabel.Text = string.Concat(ParseSeconds(songPosition), " / ", ParseSeconds(songLength));

			// Timeline Seeker
			Vector2 seekerPosition = timelineSeeker.GlobalPosition;

			seekerPosition.Y = timelineSlider.GlobalPosition.Y;
			seekerPosition.X = timelineSlider.GlobalPosition.X + (float)(songPosition / songLength) * timelineSlider.Size.X;

			timelineSeeker.GlobalPosition = seekerPosition;


			// Waveform renderer
			waveformRect.GlobalPosition = new(
				waveformWidthRef.GlobalPosition.X,
				waveformHeightRef.GlobalPosition.Y
			);

			waveformRect.Size = new(
				waveformWidthRef.Size.X,
				waveformHeightRef.Size.Y
			);

			int width = (int)waveformRect.Size.X;
			int height = (int)waveformRect.Size.Y;

			if (finishedReadingMp3 && lastWaveformRatio != width / height) {
				mp3Reader.ClearImage(Colors.Transparent);

				Task.Run(async () =>
				{
					await mp3Reader.RenderWaveformImage(width, height, Colors.Transparent, Colors.Gold * 0.8f);
					renderWaveformImage = true;
				});

				lastWaveformRatio = width / height;
			}

			if (renderWaveformImage) {
				mp3Reader.ApplyImage();

				renderWaveformImage = false;
			}
		}


		string ParseSeconds(double time)
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
			playground.IsPlaying = isPlaying;

			if (musicPlayer.Playing) {
				playButton.Icon = pauseSprite;

				musicPlayer.Seek((float)songPosition);
				return;
			}

			playButton.Icon = playSprite;
		}

		void SeekMusicPosition(float delta)
		{
			songPosition = delta * songLength;
			float desiredPosition = (float)songPosition;

			if (!isPlaying && desiredPosition < songLength - 0.1f) {
				pausedPlaybackBuffer = 0.1f;

				musicPlayer.Playing = true;
				playground.IsPlaying = true;
			}

			musicPlayer.Seek(desiredPosition);
		}
	}
}
