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


using ClockBombGames.CircleBeats.Structures.Frames;

using Godot;


namespace ClockBombGames.CircleBeats.Structures
{
	public class TimelineParameters
	{
		public IntervalTree<Vector2Frame> PositionFrames { get; set; } = new();
		public IntervalTree<Vector2Frame> ScaleFrames    { get; set; } = new();
		public IntervalTree<ScalarFrame>  RotationFrames { get; set; } = new();


		public void TransformByTime(Node3D node, float time)
		{
			#region Position
			var positionInterval = PositionFrames.Search(time).Interval;

			float positionTime = (time - positionInterval.Start) / positionInterval.Zoom;

			Vector2 position = positionInterval.Value.GetVector(positionTime);
			node.Position = new(position.X, position.Y, 0f);
			#endregion


			#region Scale
			var scaleInterval = ScaleFrames.Search(time).Interval;

			float scaleTime = (time - scaleInterval.Start) / scaleInterval.Zoom;

			Vector2 scale = scaleInterval.Value.GetVector(scaleTime);
			node.Scale = new(scale.X, scale.Y, 1f);
			#endregion


			#region Rotation
			var rotationInterval = RotationFrames.Search(time).Interval;

			float rotationTime = (time - rotationInterval.Start) / rotationInterval.Zoom;

			node.RotationDegrees = new(0f, 0f, rotationInterval.Value.GetScalar(rotationTime));
			#endregion
		}
	}
}
