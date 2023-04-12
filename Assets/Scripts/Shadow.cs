using UnityEngine;


namespace RobotoSkunk.CircleBeats {
	public class Shadow : MonoBehaviour {
		public SpriteRenderer spriteRenderer, toCopy;

		private void Start() {
			spriteRenderer.sprite = toCopy.sprite;
			transform.localPosition = Vector3.forward * Globals.bgDistance;
		}
	}
}
