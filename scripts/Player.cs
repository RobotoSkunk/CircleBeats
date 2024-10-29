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


namespace ClockBombGames.CircleBeats
{
	public partial class Player : CharacterBody3D
	{
		[ExportCategory("Components")]
		[Export] Node3D dart;


		float horizontalAxis;
		float angle;
		float effect = 0f;

		Vector2 move;

		public float Speed { get; set; } = 1f;


		public override void _Process(double delta)
		{
			horizontalAxis = Input.GetAxis("left_axis", "right_axis");
		}


		public override void _PhysicsProcess(double delta)
		{
			angle += 360f * Speed * horizontalAxis * (float)delta;
			RotationDegrees = Vector3.Forward * angle;

			effect = Mathf.Lerp(effect, horizontalAxis * 25f, Mathf.Clamp(0.2f * Speed, 0f, 1f));
			dart.RotationDegrees = Vector3.Forward * effect;
		}
	}
}
