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

		[ExportCategory("Obstacle Containers")]
		[Export] Node3D obstaclesContainerBack;
		[Export] Node3D obstaclesContainerMiddle;
		[Export] Node3D obstaclesContainerFront;

		[ExportCategory("Scenario Parts")]
		[Export] PackedScene carrouselBarScene;
		[Export] Node3D carrouselContainer;
		[Export] MeshInstance3D[] radialParts;


		double carrouselTickTime;

		float scale = 1f;

		readonly int carrouselSpikes = 5;

		public int CarrouselIndexPosition { get; private set; }
		public Node3D ObstaclesContainerBack => obstaclesContainerBack;
		public Node3D ObstaclesContainerMiddle => obstaclesContainerMiddle;
		public Node3D ObstaclesContainerFront => obstaclesContainerFront;

		public Player Player => player;

		static int SpectrumSamples => Director.SPECTRUM_SAMPLES;
		static AudioStreamPlayer MusicPlayer => Director.Instance.MusicPlayer;
		static float AverageSample => Director.Instance.AverageSample;


		public override void _Ready()
		{
			Task.Run(async () =>
			{
				await Task.Yield();

				for (int j = 0; j < carrouselSpikes; j++) {
					float jAngle = j * (360f / carrouselSpikes);

					for (int i = 0; i < SpectrumSamples; i++) {
						float angle = i * (360f / SpectrumSamples) + jAngle;

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

			float bump = 1f + (AverageSample - 0.2f) * decibelsForce;


			scale = Mathf.Lerp(scale, bump, 0.75f * RSMath.FixedDelta(delta));
			Scale = new Vector3(scale, scale, 1f);


			#region Spectrum and carousel

			if (MusicPlayer.Playing) {
				// Spin carrousel
				carrouselTickTime += delta;

				if (carrouselTickTime > (1.0 / 15.0)) {
					carrouselTickTime = 0.0;
					CarrouselIndexPosition -= 5;

					if (CarrouselIndexPosition < 0) {
						CarrouselIndexPosition = SpectrumSamples - 1;
					}
				}
			}
			#endregion
		}
	}
}
