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
	public partial class TimelineSlider : DragHandler
	{
		[Signal]
		public delegate void ValueEventHandler(float value);

		[Export] Control handlerRect;

		public event ValueEventHandler OnValueChange = delegate { };

		public float MinValue { get; private set; }
		public float MaxValue { get; private set; }

		float value;

		float prevValue;
		float prevMouseX;


		public Vector2 HandlerPosition
		{
			get {
				return handlerRect.Position;
			}
		}


		public override void _EnterTree()
		{
			base._EnterTree();
			Callable.From(() => Resized += OnResizeHandler).CallDeferred();
		}
		public override void _ExitTree()
		{
			base._EnterTree();
			Resized -= OnResizeHandler;
		}

		void OnResizeHandler() => SetValue(value);
		protected override void OnDragStart(InputEventMouseButton _) => HandleChange();
		protected override void OnDrag(InputEventMouseMotion _) => HandleChange();


		public void SetMinMaxValues(float min, float max)
		{
			MinValue = min;
			MaxValue = max;

			SetValue(value);
		}


		private void HandleChange()
		{
			float mouseX = GetLocalMousePosition().X;

			if (prevValue != value || mouseX != prevMouseX) {
				value = MinValue + (Mathf.Clamp(mouseX / Size.X, 0, 1) * (MaxValue - MinValue));

				SetValue(value);
				OnValueChange(value);
			}

			prevMouseX = mouseX;
			prevValue = value;
		}

		public void SetValue(float value)
		{
			this.value = value;

			Vector2 handlerPos = handlerRect.Position;
			handlerPos.X = (value - MinValue) / (MaxValue - MinValue) * Size.X;

			handlerRect.Position = handlerPos;
		}
	}
}
