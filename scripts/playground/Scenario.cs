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
using System.Threading.Tasks;
using ClockBombGames.CircleBeats.Playground.Obstacles;
using ClockBombGames.CircleBeats.Structures;
using ClockBombGames.CircleBeats.Structures.Frames;
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
		[Export] PackedScene squareObstacle;
		[Export] Node3D obstaclesContainerBack;
		[Export] Node3D obstaclesContainerMiddle;
		[Export] Node3D obstaclesContainerFront;

		[ExportCategory("Scenario Parts")]
		[Export] PackedScene carrouselBarScene;
		[Export] Node3D carrouselContainer;
		[Export] MeshInstance3D[] radialParts;


		Random _random = new();
		double carrouselTickTime;

		float scale = 1f;
		float[] spectrum;

		readonly int carrouselSpikes = 5;
		readonly int spectrumSamples = 128;

		public int CarrouselIndexPosition { get; private set; }

		ObjectTimeline<Square> obstacles;

		public float[] Spectrum => spectrum;
		public Player Player => player;

		static AudioStreamPlayer MusicPlayer => Director.Instance.MusicPlayer;


		public override void _Ready()
		{
			spectrum = new float[spectrumSamples];

			for (int i = 0; i < radialParts.Length; i++) {
				MeshInstance3D mesh = radialParts[i];

				mesh.SetInstanceShaderParameter("_color", Color.FromHsv(0, 0, i % 2 == 0 ? 0.5f : 0.75f));
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


			obstacles = new([ squareObstacle ]);

			float audioLength = 200f;

			for (int i = 0; i < 2500; i++) {
				TimelineParameters parameters = new();

				Vector2 pos1 = RandomVector();
				Vector2 pos2 = RandomVector();
				Vector2 pos3 = RandomVector();

				parameters.PositionFrames.Add(new(0f, 0.5f, new(pos1, pos2, BezierCurve.EaseInOut)));
				parameters.PositionFrames.Add(new(0.5f, 1f, new(pos2, pos3, BezierCurve.EaseInOut)));

				parameters.ScaleFrames.Add(new(0f,   0.2f, new(Vector2.Zero, Vector2.One, BezierCurve.QuartIn)));
				parameters.ScaleFrames.Add(new(0.2f, 0.8f, new(Vector2.One, Vector2.One, BezierCurve.Linear)));
				parameters.ScaleFrames.Add(new(0.8f,   1f, new(Vector2.One, Vector2.Zero, BezierCurve.QuartOut)));

				parameters.RotationFrames.Add(new(0f, 1f, new(0f, _random.NextSingle() * 360f, BezierCurve.CubicInOut)));


				float timeStart = _random.NextSingle() * audioLength;
				float timeEnd = timeStart + 5f;
				// float timeEnd = timeStart + _random.NextSingle() * (audioLength - timeStart);

				Node parentTarget = _random.NextSingle() switch {
					< 0.33f => obstaclesContainerBack,
					> 0.66f => obstaclesContainerFront,
					_ => obstaclesContainerMiddle,
				};

				ObjectTimeline<Square>.NodeTimeline nodeTimeline = new(timeStart, timeEnd, parameters) {
					PoolIndex = 0,
					ParentTarget = parentTarget,
				};

				obstacles.Add(nodeTimeline);
			}
		}

		Vector2 RandomVector()
		{
			return new (
				-10f + _random.NextSingle() * 20f,
				-10f + _random.NextSingle() * 20f
			);
		}


		public override void _Process(double delta)
		{
			float decibelsForce = playground.DecibelsForce;

			float bump = 1f + (playground.AverageSample - 0.2f) * decibelsForce;


			scale = Mathf.Lerp(scale, bump, 0.75f * RSMath.FixedDelta(delta));
			Scale = new Vector3(scale, scale, 1f);


			#region Spectrum and carousel

			if (MusicPlayer.Playing) {
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

		public override void _PhysicsProcess(double delta)
		{
			if (MusicPlayer.Playing) {
				obstacles.GetTime(MusicPlayer.GetPlaybackPosition());
			}
		}
	}
}
