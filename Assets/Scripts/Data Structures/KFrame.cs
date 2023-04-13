using UnityEngine;


namespace RobotoSkunk.Structures {
	/// <summary>
	/// A structure that represents a key frame.
	/// </summary>
	public struct KFrame {
		public float startPosition;
		public float endPosition;
		public Interval lifeTime;
		public BezierCurve curve;

		public KFrame(float startPosition, float endPosition, BezierCurve curve, Interval lifeTime) {
			this.startPosition = startPosition;
			this.endPosition = endPosition;
			this.lifeTime = lifeTime;
			this.curve = curve;
		}
		public KFrame(float startPosition, float endPosition, BezierCurve curve, float startTime, float endTime) {
			this.startPosition = startPosition;
			this.endPosition = endPosition;
			this.lifeTime = new Interval(startTime, endTime);
			this.curve = curve;
		}


		public bool ContainsTime(float time) => lifeTime.Contains(time);

		public float GetPosition(float time) {
			float curveTime = curve.GetTime(GetRelativeTime(time)).y;

			// Debug.Log(GetRelativeTime(time) + " | " + curveTime);

			return Mathf.Lerp(startPosition, endPosition, curveTime);
		}


		float GetRelativeTime(float time) {
			return Mathf.Clamp01((time - lifeTime.start) / (lifeTime.end - lifeTime.start));
		}
	}


	public struct KFrameColor {
		public Color startColor;
		public Color endColor;
		public Interval lifeTime;
		public BezierCurve curve;

		public KFrameColor(Color startColor, Color endColor, BezierCurve curve, Interval lifeTime) {
			this.startColor = startColor;
			this.endColor = endColor;
			this.lifeTime = lifeTime;
			this.curve = curve;
		}
		public KFrameColor(Color startColor, Color endColor, BezierCurve curve, float start, float end) {
			this.startColor = startColor;
			this.endColor = endColor;
			this.lifeTime = new Interval(start, end);
			this.curve = curve;
		}


		public bool ContainsTime(float time) => lifeTime.Contains(time);

		public Color GetColor(float time) {
			float curveTime = curve.GetTime(GetRelativeTime(time)).y;

			return Color.Lerp(startColor, endColor, curveTime);
		}

		float GetRelativeTime(float time) {
			return (time - lifeTime.start) / (lifeTime.end - lifeTime.start);
		}
	}



	public class Vector2Timeline {
		public IntervalTree<KFrame> framesX = new();
		public IntervalTree<KFrame> framesY = new();

		KFrame lastX;
		KFrame lastY;


		Interval<KFrame> PrepareKFrame(
			float startTime,
			float endTime,
			float startPosition,
			float endPosition,
			BezierCurve curve
		) {
			startTime = Mathf.Clamp01(startTime);
			endTime = Mathf.Clamp01(endTime);

			return new Interval<KFrame>(
				startTime,
				endTime,
				new KFrame(startPosition, endPosition, curve, startTime, endTime)
			);
		}


		public void AddX(float startTime, float endTime, float startPosition, float endPosition, BezierCurve curve) {
			framesX.Add(PrepareKFrame(startTime, endTime, startPosition, endPosition, curve));
		}

		public void AddY(float startTime, float endTime, float startPosition, float endPosition, BezierCurve curve) {
			framesY.Add(PrepareKFrame(startTime, endTime, startPosition, endPosition, curve));
		}

		public void AddX(Interval interval, float startPosition, float endPosition, BezierCurve curve) {
			AddX(interval.start, interval.end, startPosition, endPosition, curve);
		}
		public void AddY(Interval interval, float startPosition, float endPosition, BezierCurve curve) {
			AddY(interval.start, interval.end, startPosition, endPosition, curve);
		}


		public void Build() {
			framesX.Build();
			framesY.Build();
		}


		public Vector2 GetPosition(float time) {
			KFrame? frameX = framesX.Search(time)?.value ?? null;
			KFrame? frameY = framesY.Search(time)?.value ?? null;

			Vector2 result = new(
				frameX?.GetPosition(time) ?? lastX.GetPosition(time),
				frameY?.GetPosition(time) ?? lastY.GetPosition(time)
			);

			lastX = frameX ?? lastX;
			lastY = frameY ?? lastY;


			if (float.IsNaN(result.x)) result.x = 0;
			if (float.IsNaN(result.y)) result.y = 0;

			return result;
		}
	}
}
