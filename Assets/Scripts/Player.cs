using UnityEngine;
using UnityEngine.InputSystem;


namespace RobotoSkunk.CircleBeats {
	public class Player : MonoBehaviour {
		public SpriteRenderer dart;
		public float speed = 1f;

		float angle = 0f, angleDelta = 0f, effect = 0f;
		Vector2 move;

		private void Update() {
			angleDelta += move.x * speed * Time.deltaTime;
			angle = Mathf.Lerp(angle, angleDelta, 0.65f);
			transform.localEulerAngles = angle * Vector3.forward;

			effect = Mathf.Lerp(effect, move.x * 25f, 0.5f);
			dart.transform.localEulerAngles = effect * Vector3.forward;
		}


		public void OnMove(InputAction.CallbackContext context) => move = context.ReadValue<Vector2>() * -1f;
	}
}
