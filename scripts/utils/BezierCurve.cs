/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2024 Edgar Lima (RobotoSkunk) <contact@robotoskunk.com>

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
	public struct BezierCurve
	{
		public Vector2 p1, p2;

		public BezierCurve(Vector4 v)
		{
			p1 = new Vector2(v.X, v.Y);
			p2 = new Vector2(v.Z, v.W);
		}

		public BezierCurve(Vector2 p1, Vector2 p2)
		{
			this.p1 = p1;
			this.p2 = p2;
		}

		public BezierCurve(float p1x, float p1y, float p2x, float p2y)
		{
			p1 = new Vector2(p1x, p1y);
			p2 = new Vector2(p2x, p2y);
		}


		public Vector2 GetTime(float t)
		{
			t = Mathf.Clamp(t, 0, 1);

			// We don't need to process anything if the curve is linear.
			if (p1 == Vector2.Zero && p2 == Vector2.One) {
				return new Vector2(t, t);
			}

			Vector2 p0 = Vector2.Zero;
			Vector2 p3 = Vector2.One;

			float t2 = t * t;
			float t3 = t2 * t;

			float u = 1 - t;
			float u2 = u * u;
			float u3 = u2 * u;

			return u3 * p0 + 3 * u2 * t * p1 + 3 * u * t2 * p2 + t3 * p3;
		}

		public static BezierCurve Linear => new(0, 0, 1, 1);

		public static BezierCurve EaseIn => new(0.42f, 0, 1, 1);
		public static BezierCurve EaseOut => new(0, 0, 0.58f, 1);
		public static BezierCurve EaseInOut => new(0.42f, 0, 0.58f, 1);

		public static BezierCurve CubicIn => new(0.55f, 0.055f, 0.675f, 0.19f);
		public static BezierCurve CubicOut => new(0.215f, 0.61f, 0.355f, 1);
		public static BezierCurve CubicInOut => new(0.645f, 0.045f, 0.355f, 1);

		public static BezierCurve QuartIn => new(0.895f, 0.03f, 0.685f, 0.22f);
		public static BezierCurve QuartOut => new(0.165f, 0.84f, 0.44f, 1);
		public static BezierCurve QuartInOut => new(0.77f, 0, 0.175f, 1);

		public static BezierCurve QuintIn => new(0.755f, 0.05f, 0.855f, 0.06f);
		public static BezierCurve QuintOut => new(0.23f, 1, 0.32f, 1);
		public static BezierCurve QuintInOut => new(0.86f, 0, 0.07f, 1);

		public static BezierCurve SineIn => new(0.47f, 0, 0.745f, 0.715f);
		public static BezierCurve SineOut => new(0.39f, 0.575f, 0.565f, 1);
		public static BezierCurve SineInOut => new(0.445f, 0.05f, 0.55f, 0.95f);


		public static implicit operator BezierCurve((Vector2, Vector2) tuple) => new(tuple.Item1, tuple.Item2);
		public static implicit operator BezierCurve((float, float, float, float) tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		public static implicit operator BezierCurve(Vector4 v) => new(v);

		public static implicit operator Vector4(BezierCurve curve) => new(curve.p1.X, curve.p1.Y, curve.p2.X, curve.p2.Y);
		public static implicit operator (Vector2, Vector2)(BezierCurve curve) => (curve.p1, curve.p2);
		public static implicit operator (float, float, float, float)(BezierCurve curve) => (curve.p1.X, curve.p1.Y, curve.p2.X, curve.p2.Y);

		public static implicit operator Vector2[](BezierCurve curve) => new Vector2[] { curve.p1, curve.p2 };
		public static implicit operator BezierCurve(Vector2[] v) => new(v[0], v[1]);
	}
}
