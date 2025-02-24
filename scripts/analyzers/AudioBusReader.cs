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


namespace ClockBombGames.CircleBeats.Analyzers
{
	public class AudioBusReader
	{
		readonly int busIndex = -1;
		readonly AudioEffectCapture capture;
		readonly AudioEffectSpectrumAnalyzerInstance spectrumAnalyzer;

		private const int MIN_FREQ = 20;
		private const int MAX_FREQ = 14000;
		private const int MIN_DB = 60;

		float averageSample = 0f;


		public AudioBusReader(string bus) {
			busIndex = AudioServer.GetBusIndex(bus);

			for (int i = 0; i < AudioServer.GetBusEffectCount(busIndex); i++) {
				AudioEffect effect = AudioServer.GetBusEffect(busIndex, i);

				if (effect is AudioEffectCapture audioEffectCapture) {
					capture = audioEffectCapture;
					break;
				}
			}

			for (int i = 0; i < AudioServer.GetBusEffectCount(busIndex); i++) {
				AudioEffectInstance effect = AudioServer.GetBusEffectInstance(busIndex, i);

				if (effect is AudioEffectSpectrumAnalyzerInstance audioEffectSpectrumAnalyzerInstance) {
					spectrumAnalyzer = audioEffectSpectrumAnalyzerInstance;
					break;
				}
			}
		}

		public float GetAverageSample()
		{
			if (capture == null) {
				GD.PrintErr("The selected audio bus requires a AudioEffectCapture effect.");

				return 0f;
			}

			int qSamples = capture.GetFramesAvailable();

			if (qSamples > 0) {
				Vector2[] buffer = capture.GetBuffer(qSamples);

				averageSample = 0;

				for (int i = 0; i < buffer.Length; i++) {
					Vector2 sample = buffer[i];

					averageSample += Mathf.Abs(sample.X) + Mathf.Abs(sample.Y);
				}

				averageSample /= qSamples * 2f;
			}

			return averageSample;
		}

		public void GetSpectrum(ref float[] spectrum)
		{
			if (spectrumAnalyzer == null) {
				GD.PrintErr("The selected audio bus requires a AudioEffectSpectrumAnalyzer effect.");

				return;
			}

			int N = spectrum.Length;
			float prevHz = MIN_FREQ;

			for (int i = 0; i < spectrum.Length; i++) {
				float hz = (N - i) * (MAX_FREQ - MIN_FREQ) / N;

				Vector2 maxFreq = spectrumAnalyzer.GetMagnitudeForFrequencyRange(prevHz, hz, 0); // 0 is Average Mode
				float energy = Mathf.Max(maxFreq.X, maxFreq.Y);

				spectrum[i] = (MIN_DB + Mathf.LinearToDb(energy)) / MIN_DB * (energy * 10f);
				prevHz = hz;
			}
		}
	}
}
