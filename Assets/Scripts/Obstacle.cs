using UnityEngine;

using RobotoSkunk.Structures;


namespace RobotoSkunk.CircleBeats {
	public struct KFrame {
		public Vector2 p1, p2, time;
		public BezierCurve curve;

		public KFrame(Vector2 p1, Vector2 p2, BezierCurve curve, Vector2 time) {
			this.p1 = p1;
			this.p2 = p2;
			this.time = time;
			this.curve = curve;
		}
	}

	public class Obstacle : MonoBehaviour {
		public Vector2 lifetime;
		// public KDTree<KFrame> positions, scales, rotations;

		// public void UpdateTime(float t) {
		// 	KDTree<KFrame>.Node pos = positions.GetTime(t);
		// 	KDTree<KFrame>.Node scale = scales.GetTime(t);
		// 	KDTree<KFrame>.Node rot = rotations.GetTime(t);

		// 	float tPos = pos.obj.curve.GetTime((t - pos.obj.time.x) / (pos.obj.time.y - pos.obj.time.x)).y;
		// 	transform.localPosition = RSMath.Lerp(pos.obj.p1, pos.obj.p2, tPos);

		// 	float tScale = scale.obj.curve.GetTime((t - scale.obj.time.x) / (scale.obj.time.y - scale.obj.time.x)).y;
		// 	transform.localScale = RSMath.Lerp(scale.obj.p1, scale.obj.p2, tScale);

		// 	float tRot = rot.obj.curve.GetTime((t - rot.obj.time.x) / (rot.obj.time.y - rot.obj.time.x)).y;
		// 	transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(rot.obj.p1.x, rot.obj.p2.x, tRot));
		// }
	}
}
