using Godot;

using ClockBombGames.CircleBeats.Structures;


namespace ClockBombGames.CircleBeats.Playground.Obstacles
{
	public partial class Square : Node3D, IForTimeline, IForIndexedObjectPool
	{
		[Export] MeshInstance3D meshInstance;

		public int PoolIndex { get; set; }

		Interval<TimelineParameters> parameters;


		public override void _Ready()
		{
			meshInstance.SetInstanceShaderParameter("_color", Color.FromHsv(GD.Randf(), 0.8f, 0.95f));
		}


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
