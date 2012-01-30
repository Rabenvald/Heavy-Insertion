using UnityEngine;
using System.Collections;

public abstract class InputController : MonoBehaviour {

    public float Throttle;
    public float Pitch;
    public float Roll;
    public float Yaw;
    public float Strafe;
    public float Jump;

    public bool PrimaryFire;
    public bool SecondaryFire;

    public bool PlayerControlled = false;

    protected TurretScript turret;

	// Use this for initialization
	void Start () 
    {
        
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
