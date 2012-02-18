using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class InputController : MonoBehaviour {
	
	public string id;
	
    public float Throttle;
    public float Pitch;
    public float Roll;
    public float Yaw;
    public float Strafe;
    public float Jump;

    public float MouseX;
    public float MouseY;

    public bool PrimaryFire;
    public bool SecondaryFire;

    public bool PlayerControlled = false;

    protected TurretScript turret;
    public TurretScript Turret
    {
        get
        {
            return turret;
        }
    }
    protected Hovercraft hull;
    public Hovercraft Hull
    {
        get
        {
            return hull;
        }
    }

	// Use this for initialization
	void Start () 
    {
        
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
