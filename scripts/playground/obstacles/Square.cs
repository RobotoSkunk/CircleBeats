using Godot;

using ClockBombGames.CircleBeats.Structures;


namespace ClockBombGames.CircleBeats.Playground.Obstacles
{
	public partial class Square : Node3D, IForTimeline, IForIndexedObjectPool
	{
		public int PoolIndex { get; set; }

		Interval<TimelineParameters> parameters;


		public virtual void SetInterval(Interval<TimelineParameters> parameters)
		{
			this.parameters = parameters;
		}

		public virtual void ExecuteTime(float time)
		{
			float deltaTime = (time - parameters.Start) / parameters.Zoom;

			parameters.Value.TransformByTime(this, deltaTime);
		}
	}
}
