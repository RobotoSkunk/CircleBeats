/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2023 Edgar Lima <contact@robotoskunk.com>

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
using ClockBombGames.CircleBeats.Analyzers;
using Godot;


namespace ClockBombGames.CircleBeats
{
	public partial class Scenario : Node3D
	{
		[Export] AudioStreamPlayer musicPlayer;
		[Export] RichTextLabel debugLabel;
		
		[Export(PropertyHint.Range, "0, 1, 0.01")]
		float decibelsForce = 1f;

		AudioBusReader audioStreamReader;
		AudioEffectCapture audioEffectCapture;
		float scale;

		// readonly float minDb = -160;


		public override void _Ready()
		{
			audioStreamReader = new AudioBusReader(musicPlayer.Bus);
			musicPlayer.Play();

			int bus = AudioServer.GetBusIndex(musicPlayer.Bus);


			for (int i = 0; i < AudioServer.GetBusEffectCount(bus); i++)
			{
				if (AudioServer.GetBusEffect(bus, i) is AudioEffectCapture audioEffectCapture) {
					this.audioEffectCapture = audioEffectCapture;
				}
			}


			// var reader = new AudioStreamReader(musicPlayer.Stream);
			// byte[] buffer = null;

			// reader.ReadNonAlloc(ref buffer);

			// GD.Print(buffer);
		}

		public override void _Process(double delta)
		{
			// float db = Mathf.Clamp(audioStreamReader.Decibels, minDb, 0f) + Mathf.Abs(minDb);
			AudioBusReaderOutput output = audioStreamReader.CalculateOutput();

			float bumpMultiplier = Mathf.Clamp(output.averageData, 0, 1);
			float bump = 1f - 0.2f * decibelsForce + 0.5f * bumpMultiplier * decibelsForce;

			debugLabel.Text = $"Decibels: {output.decibels}\nCalculated: {bump}";


			// float _scale = 1f + db / Mathf.Abs(minDb) * 0.5f;

			scale = Mathf.Lerp(scale, bump, 0.75f);
			Scale = new Vector3(scale, scale, 1f);
		}
	}
}
