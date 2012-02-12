using UnityEngine;
using UnityEditor;

public class PlaceOnTerrain : ScriptableObject {

	public Texture2D lmap;
	
	[MenuItem ("Terrain/Place Selection on Terrain")]
   static void PlaceSelectionOnTerrain() {
      foreach (Transform t in Selection.transforms) {
			Undo.RegisterUndo(t, "Move " + t.name);
			RaycastHit hit;
			if (Physics.Raycast(t.position, -Vector3.up, out hit)) {
				float distanceToGround = hit.distance;
				t.Translate(-Vector3.up * distanceToGround);
			} else if (Physics.Raycast(t.position, Vector3.up, out hit)) {
				float distanceToGround = hit.distance;
				t.Translate(Vector3.up * distanceToGround);
			}
			
		}
	}
 
	[MenuItem ("Terrain/Place Selection on Terrain", true)]
	static bool ValidatePlaceSelectionOnTerrain () {
		return Selection.activeTransform != null;
	}
  	
}
