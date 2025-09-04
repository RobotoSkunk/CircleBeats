/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2025 Edgar Lima (RobotoSkunk) <contact@robotoskunk.com>

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

using ClockBombGames.CircleBeats.Analyzers;

using Godot;


namespace ClockBombGames.CircleBeats
{
	public partial class Director : Node
	{
		[Export] AudioStreamPlayer musicPlayer;

		Window window;
		float[] audioSpectrum;
		bool canResetAudioValues = false;

		public AudioStreamPlayer MusicPlayer => musicPlayer;
		public float[] AudioSpectrum => audioSpectrum;

		public AudioBusReader AudioBusReader { get; private set; }
		public float AverageSample { get; private set; }


		public override void _Ready()
		{
			window = GetViewport().GetWindow();
			window.MinSize = new(1270, 720);

			AudioBusReader = new AudioBusReader(MusicPlayer.Bus);

			audioSpectrum = new float[SPECTRUM_SAMPLES];
			Instance = this;
		}

		public override void _Process(double delta)
		{
			if (MusicPlayer.Playing) {
				AverageSample = AudioBusReader.GetAverageSample();
				AudioBusReader.GetSpectrum(ref audioSpectrum);

				canResetAudioValues = true;

			} else if (canResetAudioValues) {
				AverageSample = 0f;
				audioSpectrum = new float[SPECTRUM_SAMPLES];

				canResetAudioValues = false;
			}
		}

		public static Director Instance { get; private set; }
		public static readonly int SPECTRUM_SAMPLES = 128;
	}
}
