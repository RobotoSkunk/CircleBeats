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
	public partial class TimelineSlider : Panel
	{
		[Signal]
		public delegate void ValueEventHandler(float value);

		[Export] TextureRect handlerRect;

		public event ValueEventHandler OnValueChange = delegate { };

		bool isFocused;
		bool isChangingValue;
		float prevMouseX;


		public override void _Ready()
		{
			MouseEntered += OnMouseEnter;
			MouseExited += OnMouseExit;
		}

		public override void _Process(double delta)
		{
			bool buttonPressed = Input.IsMouseButtonPressed(MouseButton.Left);

			if ((isFocused && buttonPressed) || isChangingValue) {
				float mouseX = GetLocalMousePosition().X;

				if (mouseX != prevMouseX) {
					float value = Mathf.Clamp(mouseX / Size.X, 0, 1);

					SetValue(value);
					OnValueChange(value);
				}

				isChangingValue = true;
				
				prevMouseX = mouseX;
			}

			if (!buttonPressed) {
				isChangingValue = false;
			}
		}


		public void SetValue(float value)
		{
			Vector2 handlerPos = handlerRect.Position;

			handlerPos.X = value * Size.X;

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
