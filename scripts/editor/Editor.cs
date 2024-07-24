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


using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class Editor : Control
	{
		[Export] Playground playground;

		[ExportGroup("UI Components")]
		[Export] HSlider timelineSlider;

		[ExportSubgroup("Split Containers")]
		[Export] HSplitContainer timelineHeader;
		[Export] HSplitContainer timelineBody;

		[ExportSubgroup("Playtest Controls")]
		[Export] Button skipBackwardButton;
		[Export] Button fastBackwardsButton;
		[Export] Button playButton;
		[Export] Button fastForwardButton;
		[Export] Button skipForwardButton;

		[ExportGroup("Resources")]
		[ExportSubgroup("Play Button")]
		[Export] CompressedTexture2D playSprite;
		[Export] CompressedTexture2D pauseSprite;


		AudioStreamPlayer musicPlayer;

		double songPosition = 0f;
		double songLength = 0f;
		double pausedPlaybackBuffer;

		bool isPlaying = false;


		public override void _Ready()
		{
			// Load music info
			musicPlayer = playground.MusicPlayer;
			songLength = musicPlayer.Stream.GetLength();

			// Set Controls listeners
			skipBackwardButton.Pressed += () => SeekMusicPosition(0);
			fastBackwardsButton.Pressed += DecreaseSongTempo;
			playButton.Pressed += OnPlayPressed;
			fastForwardButton.Pressed += IncreaseSongTempo;
			skipForwardButton.Pressed += () => SeekMusicPosition(songLength);

			timelineSlider.ValueChanged += SeekMusicPosition;
		}

		public override void _Process(double delta)
		{
			timelineHeader.SplitOffset = timelineBody.SplitOffset;

			if (isPlaying) {
				songPosition = musicPlayer.GetPlaybackPosition();

				timelineSlider.SetValueNoSignal(songPosition / songLength);

			} else if (pausedPlaybackBuffer > 0f) {
				pausedPlaybackBuffer -= delta;

			} else if (musicPlayer.Playing) {
				musicPlayer.Playing = false;
			}
		}


		void OnPlayPressed()
		{
			isPlaying = !isPlaying;

			musicPlayer.Playing = isPlaying;

			if (musicPlayer.Playing) {
				playButton.Icon = pauseSprite;

				musicPlayer.Seek((float)songPosition);
				return;
			}

			playButton.Icon = playSprite;
		}

		void SeekMusicPosition(double delta)
		{
			songPosition = delta * songLength;
			float desiredPosition = (float)songPosition;

			if (!isPlaying) {
				pausedPlaybackBuffer = 0.05f;
				musicPlayer.Playing = true;
			}

			musicPlayer.Seek(desiredPosition);
		}

		void IncreaseSongTempo()
		{
			musicPlayer.PitchScale++;
		}

		void DecreaseSongTempo()
		{
			musicPlayer.PitchScale--;
		}
	}
}
