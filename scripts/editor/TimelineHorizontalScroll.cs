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
	public partial class TimelineHorizontalScroll : Control
	{
		[Signal]
		public delegate void DragEventHandler();

		[Export] Control handlerBar;

		[ExportCategory("Handlers")]
		[Export] DragMotionHandler minHandler;
		[Export] DragMotionHandler scrollHandler;
		[Export] DragMotionHandler maxHandler;

		public event DragEventHandler OnDragging = delegate { };


		public float MinZoom { get; set; }
		public float MinValue { get; private set; } = 0f;
		public float MaxValue { get; private set; } = 1f;

		float zoomBuffer;
		float maxValueBuffer;
		float minValueBuffer;


		public override void _EnterTree()
		{
			minHandler.OnMotionStartEvent += MinHandlerStartCallback;
			minHandler.OnMotionEvent += MinHandlerCallback;

			maxHandler.OnMotionStartEvent += MaxHandlerStartCallback;
			maxHandler.OnMotionEvent += MaxHandlerCallback;

			scrollHandler.OnMotionStartEvent += ScrollHandlerStartCallback;
			scrollHandler.OnMotionEvent += ScrollHandlerCallback;

			Callable.From(() =>
			{
				Resized += OnResizeHandler;
			}).CallDeferred();
		}

		public override void _ExitTree()
		{
			minHandler.OnMotionStartEvent -= MinHandlerStartCallback;
			minHandler.OnMotionEvent -= MinHandlerCallback;

			maxHandler.OnMotionStartEvent -= MaxHandlerStartCallback;
			maxHandler.OnMotionEvent -= MaxHandlerCallback;

			scrollHandler.OnMotionStartEvent -= ScrollHandlerStartCallback;
			scrollHandler.OnMotionEvent -= ScrollHandlerCallback;

			Resized -= OnResizeHandler;
		}

		void OnResizeHandler() => CalculateValues();

		void CalculateValues()
		{
			if (MinValue < 0f) {
				MinValue = 0f;
			}

			MaxValue = Mathf.Clamp(MaxValue, MinValue + MinZoom, 1f);

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



		void MinHandlerStartCallback() => minValueBuffer = MinValue;
		void MinHandlerCallback(Vector2 pointer)
		{
			MinValue = minValueBuffer + TranslateVelocity(pointer.X);

			if (MinValue > MaxValue - MinZoom) {
				MinValue = MaxValue - MinZoom;
			}

			CalculateValues();

			OnDragging.Invoke();
		}


		void MaxHandlerStartCallback() => maxValueBuffer = MaxValue;
		void MaxHandlerCallback(Vector2 pointer)
		{
			MaxValue = maxValueBuffer + TranslateVelocity(pointer.X);

			if (MaxValue < MinValue + MinZoom) {
				MaxValue = MinValue + MinZoom;
			}

			CalculateValues();

			OnDragging.Invoke();
		}


		void ScrollHandlerStartCallback()
		{
			minValueBuffer = MinValue;
			maxValueBuffer = MaxValue;

			zoomBuffer = MaxValue - MinValue;
		}

		void ScrollHandlerCallback(Vector2 pointer)
		{
			float relativePos = TranslateVelocity(pointer.X);

			MinValue = minValueBuffer + relativePos;
			MaxValue = maxValueBuffer + relativePos;


			if (MinValue < 0f) {
				MinValue = 0f;
				MaxValue = zoomBuffer;
			}

			if (MaxValue > 1f) {
				MaxValue = 1f;
				MinValue = 1f - zoomBuffer;
			}

			CalculateValues();

			OnDragging.Invoke();
		}
	}
}
