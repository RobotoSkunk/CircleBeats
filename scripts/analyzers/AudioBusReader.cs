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
using Godot;


namespace ClockBombGames.CircleBeats.Analyzers
{
	public class AudioBusReaderOutput
	{
		public float decibels;
		public float averageData;


		public AudioBusReaderOutput()
		{
			decibels = 0f;
			averageData = 0f;
		}

		public AudioBusReaderOutput(float decibels, float averageData)
		{
			this.decibels = decibels;
			this.averageData = averageData;
		}

		public override string ToString()
		{
			return $"Decibels: {decibels}, Average: {averageData}";
		}
	}

	public class AudioBusReader
	{
		readonly int busIndex = -1;
		readonly AudioEffectCapture capture;
		readonly AudioEffectSpectrumAnalyzerInstance spectrumAnalyzer;

		float decibels = -160f;
		float averageData = 0f;


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

		public void CalculateOutput(ref AudioBusReaderOutput output)
		{
			if (capture == null) {
				int busChannels = AudioServer.GetBusChannels(busIndex);
				float db = 0f;

				if (busChannels > 0) {
					for (int i = 0; i < busChannels; i++) {
						db += AudioServer.GetBusPeakVolumeLeftDb(busIndex, i);
						db += AudioServer.GetBusPeakVolumeRightDb(busIndex, i);
					}

					db /= busChannels * 2;
				}

				decibels = db;
				averageData = Mathf.Abs(decibels) / 200f;

			} else {
				int qSamples = capture.GetFramesAvailable();

				if (qSamples > 0) {
					Vector2[] buffer = capture.GetBuffer(qSamples);

					float sum = 0;
					averageData = 0;

					for (int i = 0; i < buffer.Length; i++) {
						Vector2 sample = buffer[i];

						sum += sample.X * sample.X + sample.Y * sample.Y;
						averageData += Mathf.Abs(sample.X) + Mathf.Abs(sample.Y);
					}

					float rms = Mathf.Sqrt(sum / qSamples);
					averageData /= qSamples;

					decibels = 20f * MathF.Log10(rms / 0.1f);
				}


				if (decibels < -160f) {
					decibels = -160f;
				}
			}

			output.decibels = decibels;
			output.averageData = averageData;
		}

		public void GetSpectrum(ref float[] spectrum, float maxFrequency)
		{
			if (spectrumAnalyzer == null) {
				return;
			}

			float prevHz = 0f;

			for (int i = 0; i < spectrum.Length; i++) {
				float hz = (i + 1) * maxFrequency / spectrum.Length;

				Vector2 rangeAvg = spectrumAnalyzer.GetMagnitudeForFrequencyRange(
					prevHz,
					hz,
					AudioEffectSpectrumAnalyzerInstance.MagnitudeMode.Average
				);

				Vector2 rangeMax = spectrumAnalyzer.GetMagnitudeForFrequencyRange(
					prevHz,
					hz,
					AudioEffectSpectrumAnalyzerInstance.MagnitudeMode.Max
				);

				Vector2 range = (rangeAvg + rangeMax) / 2f;

				spectrum[i] = Mathf.Max(range.X, range.Y) * 10f;
				prevHz = hz;
			}
		}
	}
}
