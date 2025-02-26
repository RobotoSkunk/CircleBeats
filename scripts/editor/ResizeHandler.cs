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

using ClockBombGames.CircleBeats.Editor.Base;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class ResizeHandler : DragHandler
	{
		[Signal]
		public delegate void OnResizeDragEventHandler(float delta);

		public OnResizeDragEventHandler OnResize = delegate { };


		protected override void OnDrag(InputEventMouseMotion motionEvent)
		{
			OnResize.Invoke(motionEvent.Relative.Y);
		}
	}
}
