
using UnityEngine;



namespace RobotoSkunk {
	public struct BezierCurve {
		public Vector2 p1, p2;

		public BezierCurve(Vector4 v) {
			p1 = new Vector2(v.x, v.y);
			p2 = new Vector2(v.z, v.w);
		}
		public BezierCurve(Vector2 p1, Vector2 p2) {
			this.p1 = p1;
			this.p2 = p2;
		}
		public BezierCurve(float p1x, float p1y, float p2x, float p2y) {
			p1 = new Vector2(p1x, p1y);
			p2 = new Vector2(p2x, p2y);
		}


		public Vector2 GetTime(float t) {
			t = Mathf.Clamp01(t);

			// We don't need to process anything if the curve is linear.
			if (p1 == Vector2.zero && p2 == Vector2.one) {
				return new Vector2(t, t);
			}

			Vector2 p0 = Vector2.zero;
			Vector2 p3 = Vector2.one;

			float t2 = t * t;
			float t3 = t2 * t;

			float u = 1 - t;
			float u2 = u * u;
			float u3 = u2 * u;

			return u3 * p0 + 3 * u2 * t * p1 + 3 * u * t2 * p2 + t3 * p3;
		}

		public static readonly BezierCurve linear = new(0, 0, 1, 1);

		public static readonly BezierCurve easeIn = new(0.42f, 0, 1, 1);
		public static readonly BezierCurve easeOut = new(0, 0, 0.58f, 1);
		public static readonly BezierCurve easeInOut = new(0.42f, 0, 0.58f, 1);

		public static readonly BezierCurve cubicIn = new(0.55f, 0.055f, 0.675f, 0.19f);
		public static readonly BezierCurve cubicOut = new(0.215f, 0.61f, 0.355f, 1);
		public static readonly BezierCurve cubicInOut = new(0.645f, 0.045f, 0.355f, 1);

		public static readonly BezierCurve quartIn = new(0.895f, 0.03f, 0.685f, 0.22f);
		public static readonly BezierCurve quartOut = new(0.165f, 0.84f, 0.44f, 1);
		public static readonly BezierCurve quartInOut = new(0.77f, 0, 0.175f, 1);

		public static readonly BezierCurve quintIn = new(0.755f, 0.05f, 0.855f, 0.06f);
		public static readonly BezierCurve quintOut = new(0.23f, 1, 0.32f, 1);
		public static readonly BezierCurve quintInOut = new(0.86f, 0, 0.07f, 1);

		public static readonly BezierCurve sineIn = new(0.47f, 0, 0.745f, 0.715f);
		public static readonly BezierCurve sineOut = new(0.39f, 0.575f, 0.565f, 1);
		public static readonly BezierCurve sineInOut = new(0.445f, 0.05f, 0.55f, 0.95f);


		public static implicit operator BezierCurve((Vector2, Vector2) tuple) => new(tuple.Item1, tuple.Item2);
		public static implicit operator BezierCurve((float, float, float, float) tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		public static implicit operator BezierCurve(Vector4 v) => new(v);

		public static implicit operator Vector4(BezierCurve curve) => new(curve.p1.x, curve.p1.y, curve.p2.x, curve.p2.y);
		public static implicit operator (Vector2, Vector2)(BezierCurve curve) => (curve.p1, curve.p2);
		public static implicit operator (float, float, float, float)(BezierCurve curve) => (curve.p1.x, curve.p1.y, curve.p2.x, curve.p2.y);

		public static implicit operator Vector2[](BezierCurve curve) => new Vector2[] { curve.p1, curve.p2 };
		public static implicit operator BezierCurve(Vector2[] v) => new(v[0], v[1]);
	}
}
