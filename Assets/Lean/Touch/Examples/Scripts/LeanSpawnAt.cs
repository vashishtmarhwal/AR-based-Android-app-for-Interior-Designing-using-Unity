using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component allows you to spawn a prefab at a point relative to a finger and the specified ScreenDepth.
	/// To trigger the prefab spawn you must call the Spawn method on this component from somewhere.</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanSpawnAt")]
	public class LeanSpawnAt : MonoBehaviour
	{
		[Tooltip("The prefab that gets spawned")]
		public Transform Prefab;

		[Tooltip("The conversion method used to find a world point from a screen point")]
		public LeanScreenDepth ScreenDepth;

		public void Spawn(LeanFinger finger)
		{
			if (Prefab != null && finger != null)
			{
				var instance   = Instantiate(Prefab);
				var worldPoint = ScreenDepth.Convert(finger.ScreenPosition, gameObject);

				instance.position = worldPoint;
				instance.rotation = transform.rotation;
			}
		}
	}
}