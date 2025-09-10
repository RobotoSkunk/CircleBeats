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

using ClockBombGames.CircleBeats.Structures;

using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class EditorTimelineObject : ColorRect
	{
		public Interval<TimelineParameters> TargetObject { get; set; }
		public Playground.Playground Playground { get; set; }

		bool hovered;
		bool isJustPressed;


		public override void _Ready()
		{
			MouseEntered += OnMouseEntered;
			MouseExited += OnMouseExited;
		}

		public override void _Process(double delta)
		{
			bool buttonPressed = Input.IsMouseButtonPressed(MouseButton.Left);

			if (hovered && isJustPressed && buttonPressed) {
				isJustPressed = false;

				Playground.DeleteTimelineObject(TargetObject);
				Free();

			} else if (!buttonPressed) {
				isJustPressed = true;
			}
		}

		void OnMouseEntered() => hovered = true;
		void OnMouseExited() => hovered = false;
	}
}
