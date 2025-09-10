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

using System;

using ClockBombGames.CircleBeats.Structures;
using ClockBombGames.CircleBeats.Utils;
using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class LayersContainer : Control
	{
		private const float LEFT_PADDING = 200f;

		[Export] Editor editor;

		[ExportCategory("Editor Elements")]
		[Export] EditorTimeline timeline;

		[ExportCategory("Containers")]
		[Export] VBoxContainer layersBackgrounds;
		[Export] Control timelineContent;
		[Export] VBoxContainer layersControllers;

		[ExportGroup("Scene References")]
		[Export] PackedScene editorTimelineObjectRef;

		float minTime;
		float maxTime = 1;

		bool isJustPressed = true;
		bool mouseInLayersBackgroundsContainer;

		// Try to minimize GD API calls as much as possible
		Vector2 sizeCache;


		private float ContentWidth
		{
			get {
				return sizeCache.X - LEFT_PADDING;
			}
		}


		public override void _Ready()
		{
			OnResized();
		}

		public override void _EnterTree()
		{
			timeline.OnSliderChange += ResizeContentContainer;

			layersBackgrounds.MouseEntered += OnMouseEnteredLayersContainer;
			layersBackgrounds.MouseExited += OnMouseExitedLayersContainer;

			Resized += OnResized;
		}

		public override void _ExitTree()
		{
			timeline.OnSliderChange -= ResizeContentContainer;
		}


		public override void _Process(double delta)
		{
			if (Director.Instance.MusicPlayer.Stream == null) {
				return;
			}

			bool buttonPressed = Input.IsMouseButtonPressed(MouseButton.Left);

			if (mouseInLayersBackgroundsContainer && isJustPressed && buttonPressed) {
				isJustPressed = false;

				float timeZoom = maxTime - minTime;

				if (timeZoom == 0f) {
					return;
				}

				float xPos = timelineContent.GetLocalMousePosition().X;
				float timeStart = xPos * (maxTime - minTime) / ContentWidth;

				var newInterval = editor.Playground.AddTimelineObject(timeStart, timeStart + 5f);
				var timelineObject = editorTimelineObjectRef.Instantiate<EditorTimelineObject>();

				timelineObject.TargetObject = newInterval;
				timelineObject.Playground = editor.Playground;

				Callable.From(() =>
				{
					timelineContent.AddChild(timelineObject);

					timelineObject.AnchorLeft = timeStart;
					timelineObject.AnchorRight = timeStart + 5f;
				}).CallDeferred();

			} else if (!buttonPressed) {
				isJustPressed = true;
			}
		}

		private void OnResized()
		{
			sizeCache = Size;

			ResizeContentContainerImpl();
		}


		private void ResizeContentContainer(float min, float max)
		{
			minTime = min;
			maxTime = max;

			ResizeContentContainerImpl();
		}

		private void ResizeContentContainerImpl()
		{
			Vector2 contentPos = timelineContent.Position;
			Vector2 contentSize = timelineContent.Size;

			float timeZoom = maxTime - minTime;

			if (timeZoom == 0f) {
				return;
			}

			contentSize.X = ContentWidth / timeZoom;
			contentPos.X = LEFT_PADDING + -contentSize.X * minTime;

			timelineContent.Position = contentPos;
			timelineContent.Size = contentSize;
		}


		// This is also temporal
		void OnMouseEnteredLayersContainer() => mouseInLayersBackgroundsContainer = true;
		void OnMouseExitedLayersContainer() => mouseInLayersBackgroundsContainer = false;
	}
}
