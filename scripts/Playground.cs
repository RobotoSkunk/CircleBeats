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


using ClockBombGames.CircleBeats.Analyzers;
using Godot;


namespace ClockBombGames.CircleBeats
{
	public partial class Playground : Control
	{
		[ExportCategory("Properties")]
		[Export] AudioStream music;

		[ExportCategory("Inspector Components")]
		[Export] HSlider sliderMusicVolume;
		[Export] HSlider sliderRotationX;
		[Export] HSlider sliderRotationY;
		[Export] HSlider sliderRotationSpeedZ;
		[Export] HSlider sliderDecibelsForce;
		[Export] HSlider sliderPlayerSpeed;

		[ExportCategory("Components")]
		[Export] Scenario scenario;
		[Export] AudioStreamPlayer musicPlayer;
		[Export] RichTextLabel debugLabel;
		[Export] Slider musicSlider;
		[Export] Slider virtualSlider;


		AudioBusReader audioBusReader;
		AudioBusReaderOutput audioReaderOutput = new();

		long songTicks;
		long virtualTicks;

		double virtualTime;
		float rotationZ;


		public static readonly int ticksPerSecond = ProjectSettings
														.GetSetting("physics/common/physics_ticks_per_second")
														.AsInt32();

		public AudioStreamPlayer MusicPlayer
		{
			get
			{
				return musicPlayer;
			}
		}

		public AudioBusReader AudioBusReader
		{
			get
			{
				return audioBusReader;
			}
		}

		public AudioBusReaderOutput AudioReaderOutput
		{
			get
			{
				return audioReaderOutput;
			}
		}

		public float DecibelsForce { get; private set; }


		public override void _Ready()
		{
			audioBusReader = new AudioBusReader(musicPlayer.Bus);

			musicPlayer.Stream = music;

			virtualSlider.MaxValue = musicPlayer.Stream.GetLength();

			musicSlider.MaxValue = musicPlayer.Stream.GetLength();
			musicSlider.ValueChanged += (value) => {
				musicPlayer.Seek((float)value);

				virtualTicks = TimeToTicks(value);
			};
		}

		public override void _Process(double delta)
		{
			audioBusReader.CalculateOutput(ref audioReaderOutput);

			Vector3 rotation = scenario.RotationDegrees;

			rotationZ += (float)(sliderRotationSpeedZ.Value * delta);

			rotation.X = (float)sliderRotationX.Value;
			rotation.Y = (float)sliderRotationY.Value;
			rotation.Z = rotationZ;

			int busIndex = AudioServer.GetBusIndex("MusicOutput");
			AudioServer.SetBusVolumeDb(busIndex, (float)Mathf.LinearToDb(sliderMusicVolume.Value));

			DecibelsForce = (float)sliderDecibelsForce.Value;
			scenario.Player.Speed = (float)sliderPlayerSpeed.Value;

			scenario.RotationDegrees = rotation;


			double playbackPosition = musicPlayer.GetPlaybackPosition();
			songTicks = TimeToTicks(playbackPosition);
			musicSlider.SetValueNoSignal(playbackPosition);

			debugLabel.Text = "FPS: " + Engine.GetFramesPerSecond() +
							"\nDraw Calls: " + Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame) +
							"\nAverage Audio Data: " + audioReaderOutput.averageData +
							"\nDecibels: " + audioReaderOutput.decibels +
							"\nTicks: " + virtualTicks + " / " + songTicks;

			if (!musicPlayer.Playing) {
				musicPlayer.Play();
				virtualTicks = 0;
			}
		}


		public override void _PhysicsProcess(double delta)
		{
			while (songTicks > virtualTicks) {
				virtualTicks++;
			}

			virtualTime = TicksToTime(virtualTicks);
			virtualSlider.SetValueNoSignal(virtualTime);
		}


		public static double TicksToTime(long ticks)
		{
			return ticks * (1.0 / ticksPerSecond);
		}

		public static long TimeToTicks(double time)
		{
			return (long)(time / (1.0 / ticksPerSecond));
		}
	}
}
