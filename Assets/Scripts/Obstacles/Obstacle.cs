using UnityEngine;

using RobotoSkunk.Structures;


namespace RobotoSkunk.CircleBeats {
	public class Obstacle : MonoBehaviour {
		public Interval lifeTime;
		public Vector2Timeline positions => _positions;

		Vector2Timeline _positions = new();


		bool isPrepared = false;


		/// <summary>
		/// Prepares the obstacle for use.
		/// </summary>
		public void Prepare() {
			if (isPrepared) return;

			_positions.Build();

			isPrepared = true;
		}

		public void SetTime(float time) {
			if (!lifeTime.Contains(time)) return;
			float relativeTime = GetRelativeTime(time);

			// positions
			transform.localPosition = _positions.GetPosition(time);
		}

		float GetRelativeTime(float time) {
			return (time - lifeTime.start) / (lifeTime.end - lifeTime.start);
		}
	}
}
