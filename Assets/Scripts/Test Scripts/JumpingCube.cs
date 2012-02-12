using UnityEngine;
using System.Collections;

public class JumpingCube : MonoBehaviour
{

    public float MinVelocity = 0.5f;
    public float JumpForce = 30;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	void FixedUpdate () 
    {
        if (rigidbody.velocity.magnitude < MinVelocity)
            rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
	}
}
