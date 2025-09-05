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
using ClockBombGames.CircleBeats.Playground.Obstacles;
using ClockBombGames.CircleBeats.Structures;
using ClockBombGames.CircleBeats.Utils;
using Godot;


namespace ClockBombGames.CircleBeats.Playground
{
	public partial class Playground : Control
	{
		[ExportCategory("Properties")]
		[Export] AudioStream music;

		[ExportCategory("Components")]
		[Export] Scenario scenario;
		[Export] Control gameContainer;

		[ExportCategory("Scene References")]
		[Export] PackedScene squareObstacle;

		[ExportCategory("Debugging")]
		[Export] Label debugLabel;


		long virtualTicks;
		double virtualTime;
		float rotationZ;
		bool _isPlaying;

		ObjectTimeline<Square> obstacles;

		public static AudioStreamPlayer MusicPlayer => Director.Instance.MusicPlayer;

		public static readonly int ticksPerSecond = ProjectSettings
														.GetSetting("physics/common/physics_ticks_per_second")
														.AsInt32();


		public float DecibelsForce { get; private set; }

		public bool IsPlaying
		{
			get
			{
				return _isPlaying;
			}
			set
			{
				_isPlaying = value;
				gameContainer.ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
			}
		}


		public override void _Ready()
		{
			MusicPlayer.Stream = music;
			DecibelsForce = 1f;

			obstacles = new([ squareObstacle ]);
		}

		public override void _Process(double delta)
		{
			debugLabel.Text = "FPS: " + Engine.GetFramesPerSecond() +
							"\nDraw Calls: " + Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
		}


		public override void _PhysicsProcess(double delta)
		{
			if (MusicPlayer.Playing) {
				obstacles.GetTime(MusicPlayer.GetPlaybackPosition());
			}
		}



		public ObjectTimeline<Square>.NodeTimeline AddTimelineObject(float timeStart, float timeEnd)
		{
			TimelineParameters parameters = new();

			Vector2 pos1 = RandomVector();
			Vector2 pos2 = RandomVector();
			Vector2 pos3 = RandomVector();

			parameters.PositionFrames.Add(new(0f, 0.5f, new(pos1, pos2, BezierCurve.EaseInOut)));
			parameters.PositionFrames.Add(new(0.5f, 1f, new(pos2, pos3, BezierCurve.EaseInOut)));

			parameters.ScaleFrames.Add(new(0f,   0.2f, new(Vector2.Zero, Vector2.One, BezierCurve.QuartIn)));
			parameters.ScaleFrames.Add(new(0.2f, 0.8f, new(Vector2.One, Vector2.One, BezierCurve.Linear)));
			parameters.ScaleFrames.Add(new(0.8f,   1f, new(Vector2.One, Vector2.Zero, BezierCurve.QuartOut)));

			parameters.RotationFrames.Add(new(0f, 1f, new(0f, _random.NextSingle() * 360f, BezierCurve.CubicInOut)));

			Node parentTarget = _random.NextSingle() switch {
				< 0.33f => scenario.ObstaclesContainerBack,
				> 0.66f => scenario.ObstaclesContainerFront,
				_ => scenario.ObstaclesContainerMiddle,
			};

			ObjectTimeline<Square>.NodeTimeline nodeTimeline = new(timeStart, timeEnd, parameters) {
				PoolIndex = 0,
				ParentTarget = parentTarget,
			};

			obstacles.Add(nodeTimeline);

			return nodeTimeline;
		}

		Vector2 RandomVector()
		{
			return new (
				-10f + _random.NextSingle() * 20f,
				-10f + _random.NextSingle() * 20f
			);
		}

		readonly Random _random = new();
	}
}
