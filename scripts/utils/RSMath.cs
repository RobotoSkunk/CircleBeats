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


namespace ClockBombGames.CircleBeats.Utils
{
	public partial class RSMath
	{
		private static readonly float targetFPS = 60f;

		public static float FixedDelta(double delta)
		{
			return Mathf.Clamp((float)(delta / (1f / targetFPS)), 0, 2);
		}

		public static float Clamp01(float x)
		{
			if (x < 0f) {
				return 0f;
			}

			if (x > 1f) {
				return 1f;
			}

			return x;
		}

		public static double Clamp01(double x)
		{
			if (x < 0d) {
				return 0d;
			}

			if (x > 1d) {
				return 1d;
			}

			return x;
		}
	}
}
