using UnityEngine;
using System.Collections;

public class MenuCameraFollow : MonoBehaviour 
{
	//allows us to set what object in the world we want the camera to follow from editor
	public GameObject target;
	
	//some camera following settings we want available in the editor
	public float distance = 30.0f;
	public float height = 25.0f;
	
	public float heightDamping = 2.0f;
	public float rotationDamping = 3.0f;
	
	//scripting only references
	private float wantedRotationAngle;
	private float wantedHeight;
	
	private float currentRotationAngle;
	private float currentHeight;
	
	private Quaternion currentRotation;
	
	private Vector3 myPosition;
	
	void Start()
	{
		if (!target)
		{
			//added a debug line as an example
			Debug.LogError("No Camera target set.");
		}
		//we could set the target in code from here as follows:
		//target = GameObject.Find("centroidPF");
		//but setting it in the editor makes this component highly reusable.
	}
	
	void LateUpdate () 
	{
		// Early out if we don't have a target
		if (!target)
		{
			return;
		}
	
		// Calculate the current rotation angles
		wantedRotationAngle = target.transform.eulerAngles.y;
		wantedHeight = target.transform.position.y + height;
		
		currentRotationAngle = transform.eulerAngles.y;
		currentHeight = transform.position.y;
	
		// Damp the rotation around the y-axis
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

		// Damp the height
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);

		// Convert the angle into a rotation
		currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
	
		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		transform.position = target.transform.position;
		transform.position -= currentRotation * Vector3.forward * distance;

		// Set the height of the camera
		
		//transform.position.y = currentHeight;
		//the Unity team uses many properties, calling a property on a property to set a 
		//variable does not work and you will need to do something like below
		myPosition = transform.position;
		myPosition.y = currentHeight;
		transform.position = myPosition;
		// Always look at the target
		transform.LookAt (target.transform);
}

	
}
