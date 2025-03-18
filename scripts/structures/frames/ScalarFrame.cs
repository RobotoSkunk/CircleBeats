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


using ClockBombGames.CircleBeats.Utils;

using Godot;


namespace ClockBombGames.CircleBeats.Structures.Frames
{
	public class ScalarFrame(float start, float end, BezierCurve bezierCurve)
	{
		public float Start { get; private set; } = start;
		public float End   { get; private set; } = end;

		public BezierCurve BezierCurve { get; private set; } = bezierCurve;


		public float GetScalar(float time)
		{
			float bezier = BezierCurve.GetTimeYDimension(time);

			return Mathf.Lerp(Start, End, bezier);
		}
	}
}
