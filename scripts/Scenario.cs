/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2023 Edgar Lima <contact@robotoskunk.com>

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
	public partial class Scenario : Node3D
	{
		[ExportCategory("Properties")]
		[Export(PropertyHint.Range, "0, 1, 0.01")]
		float decibelsForce = 1f;

		[ExportCategory("Components")]
		[Export] AudioStreamPlayer musicPlayer;
		[Export] RichTextLabel debugLabel;
		[Export] PackedScene carrouselBarScene;
		[Export] Node3D carrouselContainer;


		AudioBusReader audioBusReader;

		int spectrumSpikePosition;

		double carrouselTickTime;

		float scale;
		float[] spectrumBuffer;

		readonly CarrouselBar[][] carrouselBars = new CarrouselBar[5][];
		readonly int spectrumSamples = 128;


		public override void _Ready()
		{
			audioBusReader = new AudioBusReader(musicPlayer.Bus);
			spectrumBuffer = new float[spectrumSamples];

			for (int j = 0; j < carrouselBars.Length; j++) {
				float jAngle = j * (360f / carrouselBars.Length);

				carrouselBars[j] = new CarrouselBar[spectrumSamples];


				for (int i = 0; i < spectrumSamples; i++) {
					float angle = i * (360f / spectrumSamples) + jAngle;

					CarrouselBar bar = carrouselBarScene.Instantiate<CarrouselBar>();
					carrouselContainer.AddChild(bar);
					bar.Angle = angle;
					bar.CalculatePositions();

					carrouselBars[j][i] = bar;
				}
			}
		}

		public override void _Process(double delta)
		{
			AudioBusReaderOutput output = audioBusReader.CalculateOutput();

			float bumpMultiplier = Mathf.Clamp(output.averageData, 0, 1);
			float bump = 1f - 0.2f * decibelsForce + 0.5f * bumpMultiplier * decibelsForce;

			debugLabel.Text = $"Decibels: {output.decibels}\nCalculated: {bump}\nFPS: {Engine.GetFramesPerSecond()}";


			scale = Mathf.Lerp(scale, bump, 0.75f);
			Scale = new Vector3(scale, scale, 1f);



			#region Spectrum and carousel

			if (musicPlayer.Playing) {
				float[] spectrum = new float[spectrumSamples];
				audioBusReader.GetSpectrum(ref spectrum, 8000);

				System.Array.Clear(spectrumBuffer, 0, spectrumBuffer.Length);


				// Get spectrum data
				for (int i = 0; i < spectrum.Length; i++) {
					int j = i + spectrumSpikePosition;

					if (j >= spectrum.Length) {
						j -= spectrum.Length;
					}

					if (spectrum[j] > spectrumBuffer[i]) {
						spectrumBuffer[i] = spectrum[j];
					}
				}

				float barY;

				// i = spectrum index
				// r = carrousel spike index

				// Update carrousel bars
				for (int i = 0; i < spectrumSamples; i++) {
					barY = spectrumBuffer[i];

					for (int r = 0; r < carrouselBars.Length; r++) {
						if (carrouselBars[r] == null || carrouselBars[r][i] == null) {
							continue;
						}


						float currentSize = carrouselBars[r][i].Size;

						if (barY > currentSize) {
							carrouselBars[r][i].Size = Mathf.Clamp(barY, 0f, 1f) * 10f;
						}
					}
				}

				// Spin carrousel
				carrouselTickTime += delta;
				if (carrouselTickTime > (1.0 / 15.0)) {
					carrouselTickTime = 0.0;
					spectrumSpikePosition -= 5;

					if (spectrumSpikePosition < 0) {
						spectrumSpikePosition = spectrumSamples - 1;
					}
				}
			} else {
				musicPlayer.Play();
			}
			#endregion
		}
	}
}