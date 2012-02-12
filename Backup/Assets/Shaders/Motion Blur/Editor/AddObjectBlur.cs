using UnityEngine;
using UnityEditor;

public class AddObjectBlur {
	
	[MenuItem("Custom/Add Object Blur")]
	static void AddBlur() {
		foreach (Transform t in Selection.transforms) {
			AddBlurRecursive(t);
		}
	}
	
	static void AddBlurRecursive(Transform t) {
		if (t.GetComponent<MeshRenderer>() && !t.GetComponent<ObjectBlur>()) {
			t.gameObject.AddComponent(typeof(ObjectBlur));
		}
		foreach (Transform child in t) {
			AddBlurRecursive(child);
		}
	}
	
	[MenuItem("Custom/Add Object Blur", true)]
	static bool ValidateAddBlur() {
		return Selection.activeTransform;
	}
	
}