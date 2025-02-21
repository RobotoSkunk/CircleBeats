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


namespace ClockBombGames.CircleBeats.Analyzers
{
	public class MP3Reader
	{
		const int BUFFER_SECONDS = 30;

		public int SampleRate { get; private set; }
		public int Channels   { get; private set; }
		public int DataLength { get; private set; }
		public ImageTexture WaveformImageTexture { get; private set; } = new();

		MemoryStream audioMemoryStream;
		readonly List<float[]> waveformData = new();

		readonly List<float[]> samplesBuffer = new();
		Image imageBuffer = new();


		public void ReadAudioStream(AudioStreamMP3 audioStream)
		{
			audioMemoryStream = new(audioStream.Data);

			MpegFile audioFileReader = new(audioMemoryStream);
			SampleRate = audioFileReader.SampleRate;
			Channels = audioFileReader.Channels;

			ClearImage(Colors.Transparent);
			DataLength = 0;
		}


		public async Task ReadWaveformData()
		{
			if (waveformData.Count != 0) {
				return;
			}

			float[] buffer = new float[SampleRate * Channels * BUFFER_SECONDS];

			// Prepare waveform lists
			List<List<float>> waveformChannels = new() {
				new(),
				new(),
			};

			// Read waveform
			MpegFile mpegFile = new(audioMemoryStream);

			int samplesRead;
			while ((samplesRead = mpegFile.ReadSamples(buffer, 0, buffer.Length)) > 0) {

				if (Channels == 2) {
					for (int i = 0; i < samplesRead; i += 2) {
						waveformChannels[0].Add(buffer[i]);
						waveformChannels[1].Add(buffer[i + 1]);
					}
				} else {
					waveformChannels[0].AddRange(buffer);
				}

				await Task.Yield();
			}


			waveformData.Add(waveformChannels[0].ToArray());
			DataLength = waveformChannels[0].Count;

			if (waveformChannels[1].Count > 0) {
				waveformData.Add(waveformChannels[1].ToArray());
			}
		}


		public async Task ResampleWaveform(int startIndex, int endIndex, int width, List<float[]> newSamples)
		{
			if (waveformData.Count == 0) {
				GD.PrintErr("Waveform data hasn't been read.");
				return;
			}

			if (startIndex < 0) {
				startIndex = 0;
			}

			if (endIndex > waveformData[0].Length) {
				endIndex = waveformData[0].Length;
			}


			// Prepare waveform lists
			List<float[]> samples = new();
			newSamples.Clear();

			for (int i = 0; i < Channels; i++) {
				samples.Add(waveformData[i][startIndex..endIndex]);
				newSamples.Add(new float[width]);
			}


			// Resample waveform
			int dataLength = samples[0].Length;
			int ratio = dataLength / width + 1;

			int yieldBuffer = 0;


			for (int i = 0; i < width; i++) {
				int waveformIndex = i * ratio;
				int endWaveformIndex = Mathf.Min((i + 1) * ratio, dataLength);

				for (int channel = 0; channel < Channels; channel++) {
					// Final formula: (averagePeak + maxPeak) / 2;
					float sum = 0f;
					float maxPeak = 0f;

					// Jumps are double to decrease the loop length
					for (int j = waveformIndex; j < endWaveformIndex; j += 2) {
						if (j + 1 < samples[channel].Length) {
							float peak1 = Mathf.Abs(samples[channel][j]);
							float peak2 = Mathf.Abs(samples[channel][j + 1]);

							sum += peak1 + peak2;
							maxPeak = Mathf.Max(maxPeak, Mathf.Max(peak1, peak2));
						} else {
							float peak = Mathf.Abs(samples[channel][j]);

							sum += peak;
							maxPeak = Mathf.Max(maxPeak, peak);
						}
					}

					newSamples[channel][i] = ((sum / ratio) + maxPeak) / 2f;


					if (++yieldBuffer > SampleRate * Channels) {
						await Task.Yield();
					}
				}
			}
		}


		public async Task RenderWaveformImage(int startIndex, int endIndex, int width, int height, Color backgroundColor, Color wavesColor)
		{
			if (waveformData.Count == 0) {
				GD.PrintErr("Waveform data hasn't been read.");
				return;
			}

			await ResampleWaveform(startIndex, endIndex, width, samplesBuffer);

			imageBuffer = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
			imageBuffer.Fill(backgroundColor);


			int channelHeight = height / Channels;

			Parallel.For(0, width, x =>
			{
				for (int channel = 0; channel < Channels; channel ++) {
					int heightCenter = channelHeight / 2 + (channelHeight * channel);
					int peakHeight = (int)(samplesBuffer[channel][x] * (channelHeight / 2));

					peakHeight = Mathf.Clamp(peakHeight, 1, channelHeight / 2);

					for (int y = 0; y < peakHeight; y++) {
						imageBuffer.SetPixel(x, heightCenter + y, wavesColor * new Color(0.8f, 0.8f, 0.8f, 1f));
						imageBuffer.SetPixel(x, heightCenter - y, wavesColor);
					}
				}
			});
		}

		public async Task RenderWaveformImage(int width, int height, Color backgroundColor, Color wavesColor)
		{
			await RenderWaveformImage(0, DataLength, width, height, backgroundColor, wavesColor);
		}

		public async Task RenderWaveformImage(int width, int height)
		{
			await RenderWaveformImage(width, height, Colors.Black, Colors.Gold);
		}


		public void ClearImage(Color backgroundColor)
		{
			imageBuffer = Image.CreateEmpty(100, 100, false, Image.Format.Rgba8);
			imageBuffer.Fill(backgroundColor);

			ApplyImage();
		}

		public void ApplyImage()
		{
			WaveformImageTexture.SetImage(imageBuffer);
		}
	}
}
