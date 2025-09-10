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

using ClockBombGames.CircleBeats.Structures;

using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class EditorTimelineObject : ColorRect
	{
		[Export] Control leftResizerHandler;
		[Export] Control rightResizerHandler;

		public Interval<TimelineParameters> TargetObject { get; set; }
		public Playground.Playground Playground { get; set; }
		public LayersContainer LayersContainer { get; set; }

		bool hovered;
		bool dragging;
		bool resizingLeftAnchor;
		bool resizingRightAnchor;

		bool hoveredLeftResizer;
		bool hoveredRightResizer;

		float dragTimePoint;
		float originAnchor;
		float originDuration;


		public override void _Ready()
		{
			MouseEntered += OnMouseEntered;
			MouseExited += OnMouseExited;

			leftResizerHandler.MouseEntered += OnMouseEnteredLeftResizer;
			leftResizerHandler.MouseExited += OnMouseExitedLeftResizer;

			rightResizerHandler.MouseEntered += OnMouseEnteredRightResizer;
			rightResizerHandler.MouseExited += OnMouseExitedRightResizer;
		}

		public override void _Process(double delta)
		{
			if (hoveredLeftResizer || hoveredRightResizer) {
				if (Input.IsActionJustPressed("editor_create")) {
					dragTimePoint = LayersContainer.GetTimeByCursorPosition();

					if (hoveredLeftResizer) {
						originAnchor = AnchorLeft;
						resizingLeftAnchor = true;
					} else {
						originAnchor = AnchorRight;
						resizingRightAnchor = true;
					}
				}

			} else if (hovered) {
				if (Input.IsActionJustPressed("editor_create")) {

					dragTimePoint = LayersContainer.GetTimeByCursorPosition();
					originAnchor = AnchorLeft;
					originDuration = AnchorRight - AnchorLeft;

					dragging = true;

				} else if (Input.IsActionJustPressed("editor_remove")) {

					Playground.DeleteTimelineObject(TargetObject);
					Free();
				}
			}

			if ((resizingLeftAnchor || resizingRightAnchor) && Input.IsActionPressed("editor_create")) {

				float diff = LayersContainer.GetTimeByCursorPosition() - dragTimePoint;
				float targetTime = originAnchor + diff;

				if (resizingLeftAnchor) {
					if (targetTime > AnchorRight) {
						targetTime = AnchorRight;
					}

					AnchorLeft = targetTime;
				} else {
					if (targetTime < AnchorLeft) {
						targetTime = AnchorLeft;
					}

					AnchorRight = targetTime;
				}

			} else if (dragging && Input.IsActionPressed("editor_create")) {

				float diff = LayersContainer.GetTimeByCursorPosition() - dragTimePoint;
				float targetTime = originAnchor + diff;

				AnchorLeft = targetTime;
				AnchorRight = targetTime + originDuration;

			} else if (dragging || resizingLeftAnchor || resizingRightAnchor) {
				dragging = false;

				resizingLeftAnchor = false;
				resizingRightAnchor = false;

				UpdateTimelineTree();
			}
		}

		void OnMouseEntered() => hovered = true;
		void OnMouseExited() => hovered = false;

		void OnMouseEnteredLeftResizer() => hoveredLeftResizer = true;
		void OnMouseExitedLeftResizer() => hoveredLeftResizer = false;

		void OnMouseEnteredRightResizer() => hoveredRightResizer = true;
		void OnMouseExitedRightResizer() => hoveredRightResizer = false;


		void UpdateTimelineTree()
		{
			Playground.DeleteTimelineObject(TargetObject);

			TargetObject.Start = AnchorLeft;
			TargetObject.End = AnchorRight;

			Playground.AddTimelineObject(TargetObject);
		}
	}
}
