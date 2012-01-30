using UnityEngine;
using System.Collections;

public class CarouselRotator : MonoBehaviour 
{
	public Vector3 rotationAxis = Vector3.up;
	public float rotationSpeed = 60;
    public bool pitchOnly = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
        
        
		transform.Rotate(rotationAxis,rotationSpeed * Time.deltaTime);
        /*Vector3 CurRotation = transform.rotation.eulerAngles;
        if (pitchOnly)
        {
            transform.rotation = Quaternion.Euler(new Vector3(CurRotation.x, CurRotation.y, CurRotation.z));
        }*/
        
	}
}
