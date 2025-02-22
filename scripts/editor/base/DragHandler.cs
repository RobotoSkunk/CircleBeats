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


using Godot;


namespace ClockBombGames.CircleBeats.Editor.Base
{
	public partial class DragHandler : Control
	{
		bool isFocused;
		bool isChangingValue;


		public override void _EnterTree()
		{
			MouseEntered += OnMouseEnter;
			MouseExited += OnMouseExit;
		}

		public override void _ExitTree()
		{
			MouseEntered -= OnMouseEnter;
			MouseExited -= OnMouseExit;
		}


		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left) {
				if (mouseEvent.Pressed && isFocused){
					isChangingValue = true;

					OnDragStart(mouseEvent);
				}

				if (!mouseEvent.Pressed) {
					isChangingValue = false;

					OnDragEnd(mouseEvent);
				}

			} else if (@event is InputEventMouseMotion motionEvent && isChangingValue) {
				OnDrag(motionEvent);
			}
		}


		protected virtual void OnDragStart(InputEventMouseButton @mouseEvent) { }
		protected virtual void OnDrag(InputEventMouseMotion @motionEvent) { }
		protected virtual void OnDragEnd(InputEventMouseButton @mouseEvent) { }


		public void OnMouseEnter()
		{
			isFocused = true;
		}

		public void OnMouseExit()
		{
			isFocused = new Rect2(Vector2.Zero, Size).HasPoint(GetLocalMousePosition());
		}
	}
}
