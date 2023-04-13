using UnityEngine;

using RobotoSkunk.Structures;


namespace RobotoSkunk.CircleBeats {
	public class Obstacle : MonoBehaviour {
		public SpriteRenderer spriteRenderer;
		public Shadow shadow;

		[HideInInspector] public Interval lifeTime;
		[HideInInspector] public Vector2Timeline positions = new();
		[HideInInspector] public Vector2Timeline scales = new();
		[HideInInspector] public ColorTimeline colors = new();
		[HideInInspector] public ScalarTimeline rotations = new();

		[HideInInspector] public Vector2Timeline shakeStrengths = new();


		bool isPrepared = false;


		/// <summary>
		/// Prepares the obstacle for use.
		/// </summary>
		public void Prepare() {
			if (isPrepared) return;

			positions.Build();
			scales.Build();
			colors.Build();
			rotations.Build();
			shakeStrengths.Build();

			isPrepared = true;
		}

		public void SetTime(float time) {
			if (!lifeTime.Contains(time) || !isPrepared) return;
			float relativeTime = GetRelativeTime(time);

			transform.localPosition = positions.GetPosition(relativeTime);
			transform.localScale = (Vector3)scales.GetPosition(relativeTime) + Vector3.forward;
			transform.localRotation = Quaternion.Euler(0, 0, rotations.GetPosition(relativeTime));

			spriteRenderer.color = colors.GetColor(relativeTime);
			shadow.color = 0.2f * colors.GetColor(relativeTime) + new Color(0f, 0f, 0f, 1f);


			Vector2 randomPosition = 0.25f * Random.insideUnitCircle;
			Vector2 shakeStrength = RSMath.Clamp01(shakeStrengths.GetPosition(relativeTime));

			transform.localPosition += (Vector3)(shakeStrength * randomPosition);
		}

		float GetRelativeTime(float time) {
			return (time - lifeTime.start) / (lifeTime.end - lifeTime.start);
		}
	}
}
