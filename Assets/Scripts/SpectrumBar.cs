using UnityEngine;


namespace RobotoSkunk.CircleBeats {
	public class SpectrumBar : MonoBehaviour {
		[SerializeField] SpriteRenderer _spriteRenderer;

		public float maxSize;
		public float angle;

		float _size;
		public float size {
			get => _size;
			set => _size = Mathf.Clamp(value, 0f, 1f);
		}


		Vector3 startPosition;
		Vector3 endPosition;


		private void Start() {
			startPosition = CalculatePosition(5f - maxSize);
			endPosition = CalculatePosition(5f);


			_spriteRenderer.size = new(0.25f, maxSize);

			transform.localEulerAngles = (angle - 90f) * Vector3.forward;
			transform.localPosition = startPosition;
			transform.localScale = Vector3.one;
		}


		public void Update() {
			transform.localPosition = Vector3.Lerp(startPosition, endPosition, size);

			_size = Mathf.Lerp(_size, 0f, 0.1f * RSTime.delta);
		}


		Vector3 CalculatePosition(float size) {
			return size * RSMath.GetDirVector(angle * Mathf.Deg2Rad) + Globals.bgDistance * Vector3.forward;
		}
	}
}
