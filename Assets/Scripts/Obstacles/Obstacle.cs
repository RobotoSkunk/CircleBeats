using UnityEngine;

using RobotoSkunk.Structures;


namespace RobotoSkunk.CircleBeats {
	public class Obstacle : MonoBehaviour {
		public Interval lifeTime;
		public Vector2Timeline positions = new();


		bool isPrepared = false;


		/// <summary>
		/// Prepares the obstacle for use.
		/// </summary>
		public void Prepare() {
			if (isPrepared) return;

			positions.Build();

			isPrepared = true;
		}

		public void SetTime(float time) {
			if (!lifeTime.Contains(time) || !isPrepared) return;
			float relativeTime = GetRelativeTime(time);

			// positions
			transform.localPosition = positions.GetPosition(relativeTime);
		}

		float GetRelativeTime(float time) {
			return (time - lifeTime.start) / (lifeTime.end - lifeTime.start);
		}
	}
}
