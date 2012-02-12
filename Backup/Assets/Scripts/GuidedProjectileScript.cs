using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InputController))]

public class GuidedProjectileScript : ProjectileScript
{
    private InputController Controller;
    private BoxCollider myCollider;
    private Vector3 SeekerHeadOffset;
    private RaycastHit hit;
    //public GameObject Target;
    public float OptimalRange = 10;
    public float MaxTurnThrust = 10000;
    public float MaxThrust = 2270;

	void Start () 
    {
        Controller = GetComponent<GuidedProjectileInputController>();
        myCollider = GetComponent<BoxCollider>();
        rigidbody.AddForce(transform.forward * MaxThrust, ForceMode.Acceleration);
	}
	
	
	void FixedUpdate () 
    {
        ApplySteering(Controller.Pitch, Controller.Roll, Controller.Yaw);
        LookForTarget();
	}

    void LookForTarget()
    {
        if (Physics.Raycast(rigidbody.transform.position - SeekerHeadOffset, rigidbody.transform.forward, out hit, OptimalRange))// && hit.collider.transform.root == Target.transform.root)
        {
            explode();
        }
    }

    void ApplySteering(float pitchInput, float rollInput, float yawInput)
    {
        rigidbody.AddForce(transform.forward * MaxThrust, ForceMode.Acceleration);
        rigidbody.AddRelativeTorque(new Vector3(rollInput, yawInput, pitchInput) * MaxTurnThrust, ForceMode.Force);
        //rigidbody.AddTorque(new Vector3(rollInput, yawInput, pitchInput) * MaxTurnThrust, ForceMode.Force);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, rigidbody.transform.forward * OptimalRange);
    }
}
