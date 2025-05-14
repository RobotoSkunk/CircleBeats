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
using ClockBombGames.CircleBeats.Utils;
using Godot;


namespace ClockBombGames.CircleBeats.Playground
{
	public partial class Scenario : Node3D
	{
		[ExportCategory("Components")]
		[Export] Playground playground;
		[Export] Player player;

		[ExportCategory("Obstacles")]
		[Export] Node3D squareObstacle;
		[Export] Node3D obstaclesContainerBack;
		[Export] Node3D obstaclesContainerMiddle;
		[Export] Node3D obstaclesContainerFront;

		[ExportCategory("Scenario Parts")]
		[Export] PackedScene carrouselBarScene;
		[Export] Node3D carrouselContainer;
		[Export] MeshInstance3D[] radialParts;


		double carrouselTickTime;

		float scale = 1f;
		float[] spectrum;

		readonly int carrouselSpikes = 5;
		readonly int spectrumSamples = 128;

		public int CarrouselIndexPosition { get; private set; }

		public float[] Spectrum
		{
			get
			{
				return spectrum;
			}
		}

		public Player Player
		{
			get
			{
				return player;
			}
		}


		public override void _Ready()
		{
			spectrum = new float[spectrumSamples];

			for (int i = 0; i < radialParts.Length; i++) {
				MeshInstance3D mesh = radialParts[i];
				StandardMaterial3D material = (StandardMaterial3D)mesh.MaterialOverride.Duplicate();

				material.AlbedoColor = Color.FromHsv(0, 0, i % 2 == 0 ? 0.5f : 0.75f);

				mesh.MaterialOverride = material;
			}


			Task.Run(async () =>
			{
				await Task.Yield();

				for (int j = 0; j < carrouselSpikes; j++) {
					float jAngle = j * (360f / carrouselSpikes);

					for (int i = 0; i < spectrumSamples; i++) {
						float angle = i * (360f / spectrumSamples) + jAngle;

						CarrouselBar bar = carrouselBarScene.Instantiate<CarrouselBar>();

						Callable.From((int index, float angle) =>
						{
							carrouselContainer.AddChild(bar);
							bar.SetUp(angle, this, index);
						}).CallDeferred(i, angle);
					}
				}
			});
		}

		public override void _Process(double delta)
		{
			float decibelsForce = playground.DecibelsForce;

			float bump = 1f + (playground.AverageSample - 0.2f) * decibelsForce;


			scale = Mathf.Lerp(scale, bump, 0.75f * RSMath.FixedDelta(delta));
			Scale = new Vector3(scale, scale, 1f);


			#region Spectrum and carousel

			if (playground.MusicPlayer.Playing) {
				playground.AudioBusReader.GetSpectrum(ref spectrum);

				// Spin carrousel
				carrouselTickTime += delta;

				if (carrouselTickTime > (1.0 / 15.0)) {
					carrouselTickTime = 0.0;
					CarrouselIndexPosition -= 5;

					if (CarrouselIndexPosition < 0) {
						CarrouselIndexPosition = spectrumSamples - 1;
					}
				}
			}
			#endregion
		}
	}
}
