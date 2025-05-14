using Godot;

using ClockBombGames.CircleBeats.Structures;
using ClockBombGames.CircleBeats.Structures.Frames;


namespace ClockBombGames.CircleBeats.Playground.Obstacles
{
	public partial class Square : Node3D, IForTimeline<Square.Parameters>
	{
		public struct Parameters
		{
			public IntervalTree<Vector2Frame> positions;
			public IntervalTree<ScalarFrame>  rotations;
		}

		Parameters parameters;


		public virtual void SetParameters(Parameters parameters)
		{
			this.parameters = parameters;
		}


		public virtual void ExecuteTime(float time)
		{
			var positionNode = parameters.positions.Search(time);
			var rotationNode = parameters.rotations.Search(time);

			if (positionNode != null) {
				Vector2 position = positionNode.Value.GetVector(time);

				Position = new(position.X, position.Y, 0);
			}

			if (rotationNode != null) {
				float rotation = rotationNode.Value.GetScalar(time);

				RotationDegrees = new(0, 0, rotation);
			}
		}
	}
}
