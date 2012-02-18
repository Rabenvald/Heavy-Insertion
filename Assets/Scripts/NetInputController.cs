using UnityEngine;
using System.Collections;

public class NetInputController : InputController 
{
    public float TimeSinceLastUpdate = 0;

    public Vector3 LastPosition = Vector3.zero;
    public Vector3 LastRotation = Vector3.zero;

    public Vector3 PositionExtrapolation = Vector3.zero;
    public Vector3 RotationExtrapolation = Vector3.zero;

	public void Extrapolate () 
    {
        TimeSinceLastUpdate = Time.time - TimeSinceLastUpdate;
        PositionExtrapolation = LastPosition + rigidbody.velocity * TimeSinceLastUpdate;
        RotationExtrapolation = LastRotation + rigidbody.angularVelocity * TimeSinceLastUpdate;
	}
}
