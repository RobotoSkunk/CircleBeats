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


using ClockBombGames.CircleBeats.Editor.Base;

using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class DragMotionHandler : DragHandler
	{
		[Signal]
		public delegate void MotionStartEventHandler();
		public delegate void MotionEventHandler(Vector2 pointerRelative);

		public event MotionStartEventHandler OnMotionStartEvent = delegate { };
		public event MotionEventHandler OnMotionEvent = delegate { };

		Vector2 dragStartPos;


		protected override void OnDragStart(InputEventMouseButton mouseEvent)
		{
			dragStartPos = mouseEvent.GlobalPosition;

			OnMotionStartEvent.Invoke();
		}

		protected override void OnDrag(InputEventMouseMotion @motionEvent)
		{
			OnMotionEvent.Invoke(@motionEvent.GlobalPosition - dragStartPos);
		}
	}
}
