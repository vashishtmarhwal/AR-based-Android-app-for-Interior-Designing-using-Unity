using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace Lean.Touch
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanSelectable))]
	public class LeanSelectable_Editor : Editor
	{
		private bool showEvents;

		// Draw the whole inspector
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginDisabledGroup(true);
				DrawDefault("isSelected");
			EditorGUI.EndDisabledGroup();
			DrawDefault("DeselectOnUp");
			DrawDefault("HideWithFinger");
			DrawDefault("IsolateSelectingFingers");

			EditorGUILayout.Separator();

			showEvents = EditorGUILayout.Foldout(showEvents, "Show Events");

			if (showEvents == true)
			{
				DrawDefault("OnSelect");
				DrawDefault("OnSelectSet");
				DrawDefault("OnSelectUp");
				DrawDefault("OnDeselect");
			}
		}

		private void DrawDefault(string name)
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(serializedObject.FindProperty(name));

			if (EditorGUI.EndChangeCheck() == true)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
#endif

namespace Lean.Touch
{
	/// <summary>This component makes this GameObject selectable.
	/// If your game is 3D then make sure this GameObject or a child has a Collider component.
	/// If your game is 2D then make sure this GameObject or a child has a Collider2D component.
	/// If your game is UI based then make sure this GameObject or a child has a graphic with "Raycast Target" enabled.
	/// To then select it, you can add the LeanSelect and LeanFingerTap components to your scene. You can then link up the LeanFingerTap.OnTap event to LeanSelect.SelectScreenPosition.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanSelectable")]
	public class LeanSelectable : MonoBehaviour
	{
		// Event signature
		[System.Serializable] public class LeanFingerEvent : UnityEvent<LeanFinger> {}

		public static List<LeanSelectable> Instances = new List<LeanSelectable>();

		[Tooltip("Should this get deselected when the selecting finger goes up?")]
		public bool DeselectOnUp;

		[Tooltip("Should IsSelected temporarily return false if the selecting finger is still being held? This is useful when selecting multiple objects using a complex gesture (e.g. RTS style selection box)")]
		public bool HideWithFinger;

		[Tooltip("If the selecting fingers are still active, only return those to RequiredSelectable queries?")]
		public bool IsolateSelectingFingers;

		/// <summary>Returns isSelected, or false if HideWithFinger is true and SelectingFinger is still set.</summary>
		public bool IsSelected
		{
			get
			{
				// Hide IsSelected?
				if (HideWithFinger == true && isSelected == true && selectingFingers.Count > 0)
				{
					return false;
				}

				return isSelected;
			}
		}

		/// <summary>Bypass HideWithFinger.</summary>
		public bool IsSelectedRaw
		{
			get
			{
				return isSelected;
			}
		}

		/// <summary>This tells you how many LeanSelectable objects in your scene are currently selected.</summary>
		public static int IsSelectedCount
		{
			get
			{
				var count = 0;

				for (var i = Instances.Count - 1; i >= 0; i--)
				{
					if (Instances[i].IsSelected == true)
					{
						count += 1;
					}
				}

				return count;
			}
		}

		/// <summary>This event is called when selection begins (finger = the finger that selected this).</summary>
		public LeanFingerEvent OnSelect;

		/// <summary>This event is called every frame this selectable is selected with a finger (finger = the finger that selected this).</summary>
		public LeanFingerEvent OnSelectSet;

		/// <summary>This event is called when the selecting finger goes up (finger = the finger that selected this).</summary>
		public LeanFingerEvent OnSelectUp;

		/// <summary>This event is called when this is deselected, if OnSelectUp hasn't been called yet, it will get called first.</summary>
		public UnityEvent OnDeselect;

		/// <summary>If you want to change this, do it via the Select/Deselect methods (accessible from the context menu gear icon in editor)</summary>
		[Tooltip("If you want to change this, do it via the Select/Deselect methods (accessible from the context menu gear icon in editor)")]
		[SerializeField]
		private bool isSelected;

		// The fingers that were used to select this GameObject
		// If a finger goes up then it will be removed from this list
		[System.NonSerialized]
		private List<LeanFinger> selectingFingers = new List<LeanFinger>();

		/// <summary>This tells you the first or earliest still active finger that initiated selection of this object.
		/// NOTE: If the selecting finger went up then this may return null.</summary>
		public LeanFinger SelectingFinger
		{
			get
			{
				if (selectingFingers.Count > 0)
				{
					return selectingFingers[0];
				}

				return null;
			}
		}

		/// <summary>This tells you every currently active finger that selected this object.</summary>
		public List<LeanFinger> SelectingFingers
		{
			get
			{
				return selectingFingers;
			}
		}

		/// <summary>If requiredSelectable is set and not selected, the fingers list will be empty. If selected then the fingers list will only contain the selecting finger.</summary>
		public static List<LeanFinger> GetFingers(bool ignoreIfStartedOverGui, bool ignoreIfOverGui, int requiredFingerCount = 0, LeanSelectable requiredSelectable = null)
		{
			var fingers = LeanTouch.GetFingers(ignoreIfStartedOverGui, ignoreIfOverGui, requiredFingerCount);

			if (requiredSelectable != null)
			{
				if (requiredSelectable.IsSelected == false)
				{
					fingers.Clear();
				}

				if (requiredSelectable.IsolateSelectingFingers == true)
				{
					fingers.Clear();

					fingers.AddRange(requiredSelectable.selectingFingers);
				}
			}

			return fingers;
		}

		/// <summary>This allows you to limit how many objects can be selected in your scene.</summary>
		public static void Cull(int maxCount)
		{
			var count = 0;

			for (var i = Instances.Count - 1; i >= 0; i--)
			{
				var selectable = Instances[i];

				if (selectable.IsSelected == true)
				{
					count += 1;

					if (count > maxCount)
					{
						selectable.Deselect();
					}
				}
			}
		}

		/// <summary>If the specified finger selected an object, this will return the first one.</summary>
		public static LeanSelectable FindSelectable(LeanFinger finger)
		{
			for (var i = Instances.Count - 1; i >= 0; i--)
			{
				var selectable = Instances[i];

				if (selectable.IsSelectedBy(finger) == true)
				{
					return selectable;
				}
			}

			return null;
		}

		/// <summary>This allows you to replace the currently selected objects with the ones in the specified list. This is useful if you're doing box selection or switching selection groups.</summary>
		public static void ReplaceSelection(LeanFinger finger, List<LeanSelectable> selectables)
		{
			var selectableCount = 0;

			// Deselect missing selectables
			if (selectables != null)
			{
				for (var i = Instances.Count - 1; i >= 0; i--)
				{
					var selectable = Instances[i];

					if (selectable.isSelected == true && selectables.Contains(selectable) == false)
					{
						selectable.Deselect();
					}
				}
			}

			// Add new selectables
			if (selectables != null)
			{
				for (var i = selectables.Count - 1; i >= 0; i--)
				{
					var selectable = selectables[i];

					if (selectable != null)
					{
						if (selectable.isSelected == false)
						{
							selectable.Select(finger);
						}

						selectableCount += 1;
					}
				}
			}

			// Nothing was selected?
			if (selectableCount == 0)
			{
				DeselectAll();
			}
		}

		/// <summary>This tells you if the current selectable was selected by the specified finger.</summary>
		public bool IsSelectedBy(LeanFinger finger)
		{
			for (var i = selectingFingers.Count - 1; i >= 0; i--)
			{
				if (selectingFingers[i] == finger)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>This tells you the IsSelected or IsSelectedRaw value.</summary>
		public bool GetIsSelected(bool raw)
		{
			return raw == true ? IsSelectedRaw : IsSelected;
		}

		/// <summary>This selects the current object.</summary>
		[ContextMenu("Select")]
		public void Select()
		{
			Select(null);
		}

		/// <summary>This selects the current object with the specified finger.</summary>
		public void Select(LeanFinger finger)
		{
			isSelected = true;

			if (finger != null)
			{
				if (IsSelectedBy(finger) == false)
				{
					selectingFingers.Add(finger);
				}
			}

			if (OnSelect != null)
			{
				OnSelect.Invoke(finger);
			}

			// Make sure FingerUp is only registered once
			LeanTouch.OnFingerUp -= FingerUp;
			LeanTouch.OnFingerUp += FingerUp;

			// Make sure FingerSet is only registered once
			LeanTouch.OnFingerSet -= FingerSet;
			LeanTouch.OnFingerSet += FingerSet;
		}

		/// <summary>This deselects the current object.</summary>
		[ContextMenu("Deselect")]
		public void Deselect()
		{
			// Make sure we don't deselect multiple times
			if (isSelected == true)
			{
				isSelected = false;

				for (var i = selectingFingers.Count - 1; i >= 0; i--)
				{
					var selectingFinger = selectingFingers[i];

					if (OnSelectUp != null && selectingFinger != null)
					{
						OnSelectUp.Invoke(selectingFinger);
					}
				}

				selectingFingers.Clear();

				if (OnDeselect != null)
				{
					OnDeselect.Invoke();
				}
			}
		}

		/// <summary>This deselects all objects in the scene.</summary>
		public static void DeselectAll()
		{
			for (var i = Instances.Count - 1; i >= 0; i--)
			{
				Instances[i].Deselect();
			}
		}

		protected virtual void OnEnable()
		{
			// Register instance
			Instances.Add(this);
		}

		protected virtual void OnDisable()
		{
			// Unregister instance
			Instances.Remove(this);

			if (isSelected == true)
			{
				Deselect();
			}
		}

		protected virtual void LateUpdate()
		{
			// Null the selecting finger?
			// NOTE: This is done in LateUpdate so certain OnFingerUp actions that require checking SelectingFinger can still work properly
			for (var i = selectingFingers.Count - 1; i >= 0; i--)
			{
				var selectingFinger = selectingFingers[i];

				if (selectingFinger.Set == false || isSelected == false)
				{
					selectingFingers.RemoveAt(i);
				}
			}
		}

		private static void FingerSet(LeanFinger finger)
		{
			// Loop through all selectables
			for (var i = Instances.Count - 1; i >= 0; i--)
			{
				var selectable = Instances[i];

				// Was this selected with this finger?
				if (selectable.IsSelectedBy(finger) == true)
				{
					if (selectable.OnSelectSet != null)
					{
						selectable.OnSelectSet.Invoke(finger);
					}
				}
			}
		}

		private static void FingerUp(LeanFinger finger)
		{
			// Loop through all selectables
			for (var i = Instances.Count - 1; i >= 0; i--)
			{
				var selectable = Instances[i];

				// Was this selected with this finger?
				for (var j = selectable.selectingFingers.Count - 1; j >= 0; j--)
				{
					if (selectable.selectingFingers[j] == finger)
					{
						if (selectable.DeselectOnUp == true && selectable.IsSelected == true && selectable.selectingFingers.Count == 1)
						{
							selectable.Deselect();
						}
						// Deselection will call OnSelectUp
						else
						{
							// Null the finger and call OnSelectUp
							selectable.selectingFingers.RemoveAt(j);

							if (selectable.OnSelectUp != null)
							{
								selectable.OnSelectUp.Invoke(finger);
							}
						}
					}
				}
			}
		}
	}
}