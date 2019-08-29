using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This script rotates the current GameObject based on a finger swipe angle.</summary>
	[ExecuteInEditMode]
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanCanvasArrow")]
	public class LeanCanvasArrow : MonoBehaviour
	{
		[Tooltip("The current angle")]
		public float Angle;

		public void RotateToDelta(Vector2 delta)
		{
			Angle = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;
		}

		protected virtual void Update()
		{
			transform.rotation = Quaternion.Euler(0.0f, 0.0f, -Angle);
		}
	}
}