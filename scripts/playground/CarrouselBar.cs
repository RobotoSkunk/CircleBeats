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


using ClockBombGames.CircleBeats.Utils;
using Godot;


namespace ClockBombGames.CircleBeats.Playground
{
	public partial class CarrouselBar : Node3D
	{
		[Export] MeshInstance3D mesh;

		int carrouselIndex;
		float size;

		Scenario scenario;

		static float[] Spectrum => Director.Instance.AudioSpectrum;


		private int Index
		{
			get
			{
				int index = carrouselIndex + scenario.CarrouselIndexPosition;

				if (index >= Spectrum.Length) {
					index -= Spectrum.Length;

				} else if (index < 0) {
					index += Spectrum.Length;
				}


				return index;
			}
		}


		public void SetUp(float angle, Scenario scenario, int carrouselIndex)
		{
			this.scenario = scenario;
			this.carrouselIndex = carrouselIndex;

			Scale = Vector3.Zero;

			RotationDegrees = (angle - 90f) * Vector3.Forward;

			Vector2 pos = new Vector2(5f, 0).Rotated(Mathf.DegToRad(RotationDegrees.Z));
			Position = new Vector3(pos.X, pos.Y, 0);
		}

		public override void _Process(double delta)
		{
			float value = Spectrum[Index];

			if (value > size) {
				size = Mathf.Clamp(value, 0, 1) * 10f;
			}

			if (size > 0f) {
				Scale = new(size, 1f, 1f);

				size = Mathf.Lerp(size, 0f, 0.1f * RSMath.FixedDelta(delta));

				if (size < 0.01f) {
					size = 0f;
				}
			}

			mesh.Visible = size > 0.01f;
		}
	}
}
