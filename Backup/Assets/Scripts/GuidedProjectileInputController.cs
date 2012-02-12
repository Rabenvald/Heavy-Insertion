using UnityEngine;
using System.Collections;

public class GuidedProjectileInputController : InputController
{
    public float DistanceToTarget = 0;
    public GameObject Target;
    public Transform TargetTransform;
    public Vector3 TargetPosition;

    private uint lastActionPerformed = 2;
    private Vector3 tp;//DEBUG

    private GameObject looker;

	// Use this for initialization
	void Start () 
    {
        looker = new GameObject();
        looker.transform.parent = transform;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {


        if (Target != null)
        {
            DistanceToTarget = Mathf.Clamp((Target.transform.position - rigidbody.transform.position).magnitude / rigidbody.velocity.magnitude, 0, 999999999);

            Intercept(Target);
        }
        else
        {
            if (TargetTransform != null)
            {
                TargetPosition = TargetTransform.position;
            }
            if (TargetPosition != null)
            {
                DistanceToTarget = Mathf.Clamp((TargetPosition - rigidbody.transform.position).magnitude / rigidbody.velocity.magnitude, 0, 999999999);


                Intercept(TargetPosition);
            }
        }
	}

    void Intercept(GameObject target)
    {
        if (target.GetComponent<Rigidbody>() == null)
        {
            Intercept(target.transform.position);
        }
        else
        {
            Intercept(target.rigidbody.transform.position + (target.rigidbody.velocity * DistanceToTarget));
        }
    }
    void IncrementLAP()
    {
        lastActionPerformed++;
        if (lastActionPerformed > 2)
        {
            lastActionPerformed = 0;
        }
    }

    void Intercept(Vector3 targetPosition)
    {
        float zyDot = 0;
        float xyDot = 0;
        float zxDot = 0;

        if (DistanceToTarget > 0.6f)
        {
            targetPosition -= rigidbody.velocity * Mathf.Clamp((1 / Mathf.Clamp(DistanceToTarget, 0.00001f, 999999)), 0.00001f, 0.4f) * (DistanceToTarget * 0.2f) + Physics.gravity * DistanceToTarget;
        }
        //targetPosition += (targetPosition - transform.position).normalized * rigidbody.velocity.magnitude;
        looker.transform.LookAt(targetPosition, rigidbody.transform.up);
        looker.transform.position = rigidbody.transform.position;
        tp = targetPosition;
        //Vector3 targetOrientation = (looker.transform.forward).normalized;

        /*IncrementLAP();
        if (lastActionPerformed == 0)
        {
            zyDot = Vector3.Dot(rigidbody.transform.forward, -looker.transform.up) * (DistanceToTarget * 0.2f);//Pitch
        }
        else if (lastActionPerformed == 1)
        {
            xyDot = Vector3.Dot(rigidbody.transform.right, -looker.transform.up) * (DistanceToTarget * 0.2f);//Roll
        }
        else
        {
            zxDot = Vector3.Dot(rigidbody.transform.forward, -looker.transform.right) * (DistanceToTarget * 0.2f);//Yaw
        }*/


        zyDot = Vector3.Dot(rigidbody.transform.forward, -looker.transform.up) * (DistanceToTarget * 0.2f);
        xyDot = Vector3.Dot(rigidbody.transform.right, -looker.transform.up) * (DistanceToTarget * 0.2f);
        zxDot = Vector3.Dot(rigidbody.transform.forward, -looker.transform.right) * (DistanceToTarget * 0.2f);
        //float zzDot = Vector3.Dot(rigidbody.transform.forward, looker.transform.forward); //Heading

        //float degOffBore = -Mathf.Asin(zxDot) * Mathf.Rad2Deg;  //turn only in ZX plane (Yaw)   (Acos)

        Pitch = zyDot;
        Roll = xyDot;
        Yaw = zxDot;

        Pitch = Mathf.Clamp(Pitch, -1, 1);
        Roll = Mathf.Clamp(Roll, -1, 1);
        Yaw = Mathf.Clamp(Yaw, -1, 1);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, tp);
    }
}
