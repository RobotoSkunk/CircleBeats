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


using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class LayersContainer : Control
	{
		private const float LEFT_PADDING = 200f;

		[ExportCategory("Editor Elements")]
		[Export] EditorTimeline timeline;

		[ExportCategory("Containers")]
		[Export] VBoxContainer layersBackgrounds;
		[Export] Control timelineContent;
		[Export] VBoxContainer layersControllers;

		float minTime;
		float maxTime = 1;

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
			sizeCache = Size;
		}

		public override void _EnterTree()
		{
			timeline.OnSliderChange += ResizeContentContainer;
		}

		public override void _ExitTree()
		{
			timeline.OnSliderChange -= ResizeContentContainer;
		}


		public override void _Process(double delta)
		{
			Vector2 size = Size;

			if (sizeCache != size) {
				sizeCache = size;

				ResizeContentContainerImpl();
			}
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
	}
}
