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


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class TimelineSlider : Control
	{
		[Signal]
		public delegate void ValueEventHandler(float value);

		[Export] Control handlerRect;

		public event ValueEventHandler OnValueChange = delegate { };

		public float MinValue { get; set; }
		public float MaxValue { get; set; }

		bool isFocused;
		bool isChangingValue;

		float value;
		float prevMouseX;
		float lastHandlerPosSeed = 0f;


		public Vector2 HandlerPosition
		{
			get {
				return handlerRect.Position;
			}
		}


		public override void _Ready()
		{
			MouseEntered += OnMouseEnter;
			MouseExited += OnMouseExit;
		}

		public override void _Process(double delta)
		{
			float handlerPosSeed = Size.X + MinValue + MaxValue;

			if (lastHandlerPosSeed != handlerPosSeed) {
				lastHandlerPosSeed = handlerPosSeed;
				SetValue(value);
			}
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left) {
				if (mouseEvent.Pressed && isFocused){
					isChangingValue = true;
				}

				if (!mouseEvent.Pressed) {
					isChangingValue = false;
				}

			} else if (@event is InputEventMouseMotion && isChangingValue) {
				float mouseX = GetLocalMousePosition().X;

				if (mouseX != prevMouseX) {
					value = MinValue + (Mathf.Clamp(mouseX / Size.X, 0, 1) * (MaxValue - MinValue));

					SetValue(value);
					OnValueChange(value);
				}

				prevMouseX = mouseX;
			}
		}


		public void SetValue(float value)
		{
			this.value = value;

			Vector2 handlerPos = handlerRect.Position;
			handlerPos.X = (value - MinValue) / (MaxValue - MinValue) * Size.X;

			handlerRect.Position = handlerPos;
		}

		protected void OnMouseEnter()
		{
			isFocused = true;
		}

		protected void OnMouseExit()
		{
			isFocused = new Rect2(Vector2.Zero, Size).HasPoint(GetLocalMousePosition());
		}
	}
}
