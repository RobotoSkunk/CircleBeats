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


namespace ClockBombGames.CircleBeats.Analyzers
{
	public class MP3Reader
	{
		const int BUFFER_SECONDS = 30;
		const int IDEAL_BUFFER_LENGTH = 1 << 20;

		public int SampleRate { get; private set; }
		public int Channels   { get; private set; }
		public int DataLength { get; private set; }
		public bool Ready { get; private set; }


		// [MipMap level] [Channel] [Samples]
		byte[][][] waveformData;
		int mipmapMaxLevel;

		MemoryStream audioMemoryStream;


		public void ReadAudioStream(AudioStreamMP3 audioStream)
		{
			audioMemoryStream = new(audioStream.Data);

			MpegFile audioFileReader = new(audioMemoryStream);
			SampleRate = audioFileReader.SampleRate;
			Channels = audioFileReader.Channels;

			DataLength = 0;
			Ready = false;
		}


		public async Task ReadWaveformData()
		{
			if (Ready) {
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

			mipmapMaxLevel = Mathf.CeilToInt((float)DataLength / IDEAL_BUFFER_LENGTH);
			waveformData = new byte[mipmapMaxLevel][][];

			waveformData[0] = new byte[2][];
			waveformData[0][0] = [.. waveformChannels[0]];
			waveformData[0][1] = [.. waveformChannels[1]];


			// Mipmap samples
			int availableThreads = System.Environment.ProcessorCount - 2; // Exclude main and physics threads
			int jumps = Mathf.CeilToInt((float)mipmapMaxLevel / availableThreads);

			for (int m = 0; m < jumps; m++) {
				int from = availableThreads * m;
				int to = Mathf.Min(from + availableThreads, mipmapMaxLevel - 1);

				await Parallel.ForAsync(from, to, async (i, token) =>
				{
					int maxSamples = 2 + i;
					int sampleLength = DataLength / maxSamples;

					int fixedIndex = i + 1;

					waveformData[fixedIndex] = new byte[2][];
					waveformData[fixedIndex][0] = new byte[sampleLength];
					waveformData[fixedIndex][1] = new byte[sampleLength];

					await ResampleWaveformImpl(0, DataLength, sampleLength, waveformData[0], (channel, i, peak) =>
					{
						waveformData[fixedIndex][channel][i] = peak;
					});
				});
			}

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
			float zoom = (endIndex - startIndex) / (float)DataLength;

			int mipmapLevel = Mathf.Clamp((int)(zoom * mipmapMaxLevel), 0, mipmapMaxLevel - 1);

			startIndex /= mipmapLevel + 1;
			endIndex /= mipmapLevel + 1;

			await ResampleWaveformImpl(startIndex, endIndex, width, waveformData[mipmapLevel], (channel, i, peak) =>
			{
				body(channel, i, peak / 255f);
			});
		}

		public async Task ResampleWaveformImpl(int startIndex, int endIndex, int width, byte[][] samples, Action<int, int, byte> body)
		{
			if (startIndex < 0) {
				startIndex = 0;
			}

			// Resample waveform
			float chunkSize = (float)(endIndex - startIndex) / width;

			await Task.Run(() =>
			{
				for(int i = 0; i < width; i++) {
					int waveformIndex = Mathf.RoundToInt(startIndex + i * chunkSize);
					int endWaveformIndex = Mathf.RoundToInt(waveformIndex + chunkSize);
					int maxIteration = Mathf.Min(endWaveformIndex, samples[0].Length);

					if (Channels == 2) {
						byte maxPeak1 = 0; // Channel 0
						byte maxPeak2 = 0; // Channel 1

						for (int j = waveformIndex; j < maxIteration; j++) {
							byte sample1 = samples[0][j];
							byte sample2 = samples[1][j];

							// Channel 0
							if (sample1 > maxPeak1) {
								maxPeak1 = sample1;
							}

							// Channel 1
							if (sample2 > maxPeak2) {
								maxPeak2 = sample2;
							}
						};

						body(0, i, maxPeak1); // Channel 0
						body(1, i, maxPeak2); // Channel 1
					} else {
						byte maxPeak = 0; // Channel 0

						for (int j = waveformIndex; j < maxIteration; j++) {
							byte sample = samples[0][j];

							if (sample > maxPeak) {
								maxPeak = sample;
							}
						};

						body(0, i, maxPeak); // Channel 0
					}
				}
			});
		}


		public async Task RenderWaveformImage(int startIndex, int endIndex, Image image, Color backgroundColor, Color wavesColor)
		{
			if (!Ready) {
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
