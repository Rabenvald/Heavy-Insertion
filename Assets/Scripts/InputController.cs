using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class InputController : MonoBehaviour
{

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

    public float TimeSinceLastUpdate = 0;

    public Vector3 LastPosition = Vector3.zero;
    public Vector3 LastRotation = Vector3.zero;

    public Vector3 PositionExtrapolation = Vector3.zero;
    public Vector3 RotationExtrapolation = Vector3.zero;

    public void Extrapolate()
    {
        TimeSinceLastUpdate = Time.time - TimeSinceLastUpdate;
        PositionExtrapolation = LastPosition + rigidbody.velocity * TimeSinceLastUpdate;
        RotationExtrapolation = LastRotation + rigidbody.angularVelocity * TimeSinceLastUpdate;
    }
}
