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
	public partial class TimelineHorizontalScroll : Panel
	{
		[Export] Control handlerBar;

		[ExportCategory("Handlers")]
		[Export] TimelineHorizontalScrollHandler minHandler;
		[Export] TimelineHorizontalScrollHandler scrollHandler;
		[Export] TimelineHorizontalScrollHandler maxHandler;


		private readonly float minZoom = 0.025f;

		public float MinValue { get; private set; } = 0f;
		public float MaxValue { get; private set; } = 1f;

		float lastHandlerPosSeed = 0f;


		public override void _EnterTree()
		{
			minHandler.OnMotionEvent += MinHandlerCallback;
			scrollHandler.OnMotionEvent += ScrollHandlerCallback;
			maxHandler.OnMotionEvent += MaxHandlerCallback;
		}

		public override void _ExitTree()
		{
			minHandler.OnMotionEvent -= MinHandlerCallback;
			scrollHandler.OnMotionEvent -= ScrollHandlerCallback;
			maxHandler.OnMotionEvent -= MaxHandlerCallback;
		}

		public override void _Process(double delta)
		{
			float handlerPosSeed = Size.X + MinValue + MaxValue;

			if (lastHandlerPosSeed != handlerPosSeed) {
				lastHandlerPosSeed = handlerPosSeed;
				CalculateValues();
			}
		}

		void CalculateValues()
		{
			if (MinValue < 0f) {
				MinValue = 0f;
			}

			MaxValue = Mathf.Clamp(MaxValue, MinValue + minZoom, 1f);

			Vector2 size = Size;

			Vector2 handlerSize = handlerBar.Size;
			Vector2 handlerPos = handlerBar.Position;

			handlerSize.X = size.X * (MaxValue - MinValue);
			handlerPos.X = size.X * MinValue;

			handlerBar.Size = handlerSize;
			handlerBar.Position = handlerPos;
		}

		float TranslateVelocity(float x)
		{
			return x / Size.X;
		}


		void MinHandlerCallback(Vector2 pointer)
		{
			float relativePos = TranslateVelocity(pointer.X);

			if (MinValue + relativePos > MaxValue - minZoom) {
				return;
			}

			MinValue += relativePos;
			CalculateValues();
		}

		void ScrollHandlerCallback(Vector2 pointer)
		{
			float relativePos = TranslateVelocity(pointer.X);

			if (MinValue + relativePos < 0f ||MaxValue + relativePos > 1f) {
				return;
			}

			MinValue += relativePos;
			MaxValue += relativePos;

			CalculateValues();
		}

		void MaxHandlerCallback(Vector2 pointer)
		{
			float relativePos = TranslateVelocity(pointer.X);

			if (MaxValue + relativePos < MinValue + minZoom) {
				return;
			}

			MaxValue += relativePos;
			CalculateValues();
		}
	}
}
