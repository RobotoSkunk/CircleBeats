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


using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using Godot;

using NLayer;
using System;
using System.Linq;


namespace ClockBombGames.CircleBeats.Analyzers
{
	public class MP3Reader
	{
		const int BUFFER_SECONDS = 30;

		public int SampleRate { get; private set; }
		public int Channels   { get; private set; }
		public int DataLength { get; private set; }
		public bool Ready { get; private set; }

		MemoryStream audioMemoryStream;
		readonly byte[][] waveformData = new byte[2][];


		public void ReadAudioStream(AudioStreamMP3 audioStream)
		{
			audioMemoryStream = new(audioStream.Data);

			MpegFile audioFileReader = new(audioMemoryStream);
			SampleRate = audioFileReader.SampleRate;
			Channels = audioFileReader.Channels;

			DataLength = 0;
			Ready = false;

			waveformData[0] = null;
			waveformData[1] = null;
		}


		public async Task ReadWaveformData()
		{
			if (waveformData[0] != null) {
				return;
			}

			float[] buffer = new float[SampleRate * Channels * BUFFER_SECONDS];

			// Prepare waveform lists
			List<List<byte>> waveformChannels = [
				[],
				[],
			];

			// Read waveform
			MpegFile mpegFile = new(audioMemoryStream);

			int samplesRead;

			await Task.Run(() =>
			{
				while ((samplesRead = mpegFile.ReadSamples(buffer, 0, buffer.Length)) > 0) {
					if (Channels == 2) {
						for (int i = 0; i < samplesRead; i += 2) {
							waveformChannels[0].Add((byte)(255 * Mathf.Abs(buffer[i] * 0.8f)));
							waveformChannels[1].Add((byte)(255 * Mathf.Abs(buffer[i + 1] * 0.8f)));
						}
					} else {
						for (int i = 0; i < samplesRead; i++) {
							waveformChannels[0].Add((byte)(255 * Mathf.Abs(buffer[i] * 0.8f)));
						}
					}
				}
			});

			DataLength = waveformChannels[0].Count;

			waveformData[0] = [.. waveformChannels[0]];
			waveformData[1] = [.. waveformChannels[1]];

			Ready = true;
		}


		/// <summary>
		/// Resamples the waveform and executes an action on each iteration.
		/// </summary>
		/// <param name="startIndex">The start index of the sample array.</param>
		/// <param name="endIndex">The end index of the sample array.</param>
		/// <param name="width">The new length of the resampled waveform.</param>
		/// <param name="body">
		/// 	The function that will be executed on each iteration (int channel, int index, float sampleValue).
		/// </param>
		public async Task ResampleWaveform(int startIndex, int endIndex, int width, Action<int, int, float> body)
		{
			if (waveformData[0] == null) {
				GD.PrintErr("Waveform data hasn't been read.");
				return;
			}

			if (startIndex < 0) {
				startIndex = 0;
			}

			// Resample waveform
			int chunkSize = Mathf.FloorToInt(endIndex - startIndex) / width;

			await Parallel.ForAsync(0, width, async (i, value) => {
				await Task.Run(() =>
				{
					int waveformIndex = startIndex + i * chunkSize;
					int endWaveformIndex = waveformIndex + chunkSize;
					int maxIteration = Mathf.Min(endWaveformIndex, DataLength);

					for (int channel = 0; channel < Channels; channel++) {
						byte maxPeak = 0;

						for (int j = waveformIndex; j < maxIteration; j++) {
							byte sample = waveformData[channel][j];

							if (sample > maxPeak) {
								maxPeak = sample;
							}
						};

						body(channel, i, maxPeak / 255f);
					}
				}, value);
			});
		}


		public async Task RenderWaveformImage(int startIndex, int endIndex, Image image, Color backgroundColor, Color wavesColor)
		{
			if (waveformData[0] == null) {
				GD.PrintErr("Waveform data hasn't been read.");
				return;
			}

			Vector2I size = image.GetSize();
			image.Fill(backgroundColor);


			int channelHeight = size.Y / Channels;

			await ResampleWaveform(startIndex, endIndex, size.X, (channel, x, sample) =>
			{
				int heightCenter = channelHeight / 2 + (channelHeight * channel);
				int peakHeight = (int)(sample * (channelHeight / 2));

				peakHeight = Mathf.Clamp(peakHeight, 1, channelHeight / 2);

				for (int y = 0; y < peakHeight; y++) {
					image.SetPixel(x, heightCenter + y, wavesColor * new Color(0.8f, 0.8f, 0.8f, 1f));
					image.SetPixel(x, heightCenter - y, wavesColor);
				}
			});
		}

		public async Task RenderWaveformImage(Image image, Color backgroundColor, Color wavesColor)
		{
			await RenderWaveformImage(0, DataLength, image, backgroundColor, wavesColor);
		}

		public async Task RenderWaveformImage(Image image)
		{
			await RenderWaveformImage(image, Colors.Black, Colors.Gold);
		}
	}
}
