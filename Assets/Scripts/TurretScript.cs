using UnityEngine;
using System.Collections;

public class TurretScript : MonoBehaviour
{
    //public GameObject MainHull;

    public GameObject PitchComponet;
    public GameObject Muzzle;
    public float StabilizationAmount = 0.5f;

    private float MinXRot = -30;
    private float MaxXRot = 45;
    private float XRot = 0;

    private float YRot = 0;

    public float MaxTargetDistance = 8000;

    //Vector3 CurBodyRotation = Vector3.zero;
    //Vector3 CurPitchComponetRotation = Vector3.zero;


    Quaternion targetRotation = Quaternion.identity;
    //Quaternion YawtargetRotation = Quaternion.identity;

    private GameObject seeker;

    public GameObject Projectile;
    public GameObject Missile;
    public GameObject target;
    public Vector3 TargetPosition = Vector3.zero;
    public Vector3 TargetVector = Vector3.zero;
    public Transform TargetTransform;

    public float ProjectileSpeed = 4000.0f;
    public float FireRate = 1.5f;
    private float lastFired = 0.0f;

    private Vector3 randomness = Vector3.zero;
	// Use this for initialization

    void Awake()
    {
        seeker = new GameObject();
        seeker.transform.parent = transform;
    }
	void Start () 
    {
        if (StabilizationAmount < 0)
            StabilizationAmount *= -1;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        //CurBodyRotation = transform.rotation.eulerAngles;
        //CurPitchComponetRotation = transform.rotation.eulerAngles;

        if (target != null)
        {

            targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TrackingComputer.GetParabolicFiringSolution(PitchComponet.transform.position, target.transform.position, ProjectileSpeed, Physics.gravity, transform.parent.rigidbody.velocity, target.rigidbody.velocity)), Time.deltaTime * 1000f);
            //transform.rotation.x = targetRotation
        }
        else
        {
            if (TargetTransform != null)
            {
                TargetPosition = TargetTransform.position;
            }
            if (TargetPosition != null)
            {
                TargetPosition = new Vector3(Mathf.Clamp(TargetPosition.x, -MaxTargetDistance, MaxTargetDistance), Mathf.Clamp(TargetPosition.y, -MaxTargetDistance, MaxTargetDistance), Mathf.Clamp(TargetPosition.z, -MaxTargetDistance, MaxTargetDistance));
                TargetVector = TrackingComputer.GetParabolicFiringSolution(transform.position, TargetPosition, ProjectileSpeed, Physics.gravity, transform.parent.rigidbody.velocity);
                TargetVector = new Vector3(Mathf.Clamp(TargetVector.x, -MaxTargetDistance, MaxTargetDistance), Mathf.Clamp(TargetVector.y, -MaxTargetDistance, MaxTargetDistance), Mathf.Clamp(TargetVector.z, -MaxTargetDistance, MaxTargetDistance));
                targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-TargetVector), Time.deltaTime * 1000f);
                //YawtargetRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TargetPosition), Time.deltaTime * 1000f);
            }
            
        }

        /*if(CurPitchComponetRotation.x > 10)//this will limit looking up to 60
            PitchComponet.transform.rotation = Quaternion.Euler(new Vector3(10, CurPitchComponetRotation.y, CurPitchComponetRotation.z));
        else if(CurPitchComponetRotation.x < -30)//this will limit looking down to -60
            PitchComponet.transform.rotation = Quaternion.Euler(new Vector3(-30, CurPitchComponetRotation.y, CurPitchComponetRotation.z));*/

        //XRot = -Mathf.Atan2(targetRotation.eulerAngles.z - transform.position.z, targetRotation.eulerAngles.y - transform.position.y) * (180 / Mathf.PI);
        //XRot = Mathf.Clamp(XRot, MinXRot, MaxXRot);

        //YRot = -Mathf.Atan2(YawtargetRotation.eulerAngles.x - transform.position.x, YawtargetRotation.eulerAngles.z - transform.position.z) * (180 / Mathf.PI);
        //YRot = Mathf.Clamp(YRot, -StabilizationAmount, StabilizationAmount);

        PitchComponet.transform.eulerAngles = new Vector3(Mathf.Clamp(targetRotation.eulerAngles.x, -MaxTargetDistance, MaxTargetDistance), Mathf.Clamp(transform.eulerAngles.y, -MaxTargetDistance, MaxTargetDistance), Mathf.Clamp(transform.eulerAngles.z, -MaxTargetDistance, MaxTargetDistance));

        //PitchComponet.transform.localEulerAngles = new Vector3(Mathf.Clamp(PitchComponet.transform.localEulerAngles.x, MinXRot, MaxXRot), PitchComponet.transform.localEulerAngles.y, PitchComponet.transform.localEulerAngles.z);

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, YRot, transform.eulerAngles.z);
        seeker.transform.LookAt(transform.parent.transform.up, TargetPosition-transform.position);
        transform.LookAt(TargetPosition, transform.parent.transform.up);
        //transform.LookAt(transform.position + seeker.transform.up, transform.parent.transform.up);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(seeker.transform.up, transform.parent.transform.up), Time.deltaTime * 10000f);
        //PitchComponet.transform.rotation = Quaternion.Euler(new Vector3(Mathf.Clamp(CurBodyRotation.x, -30, 10), CurPitchComponetRotation.y, CurPitchComponetRotation.z));
        //transform.rotation = Quaternion.Euler(new Vector3(Mathf.Clamp(CurBodyRotation.x, -StabilizationAmount, StabilizationAmount), CurBodyRotation.y, Mathf.Clamp(CurBodyRotation.z, -StabilizationAmount, StabilizationAmount)));

        transform.localEulerAngles = new Vector3(0,transform.localEulerAngles.y,0); 

        if (Time.time - lastFired > FireRate && transform.parent.GetComponent<InputController>().PrimaryFire)
        {
            Fire();
            lastFired = Time.time;
        }

        if (Time.time - lastFired > FireRate && transform.parent.GetComponent<InputController>().SecondaryFire)
        {
            FireMissile();
            lastFired = Time.time;
        }
    }

    private void Fire()
    {
        randomness.x = Random.Range(-2, 2);
        randomness.y = Random.Range(-2, 2);
        randomness.z = Random.Range(-2, 2);

        GameObject projectile = (GameObject)Instantiate(Projectile, Muzzle.transform.position + Muzzle.transform.forward , Muzzle.transform.rotation);

        // Set its velocity
        projectile.rigidbody.velocity = Muzzle.transform.forward * ProjectileSpeed + randomness + transform.parent.rigidbody.velocity;
        // Recoil
        transform.parent.rigidbody.AddForceAtPosition(-projectile.rigidbody.velocity * projectile.rigidbody.mass, Muzzle.transform.position, ForceMode.Impulse);
    }

    private void FireMissile()
    {
        GameObject ATMissile = (GameObject)Instantiate(Missile, Muzzle.transform.position + Muzzle.transform.forward * 3, Muzzle.transform.rotation);
        ATMissile.GetComponent<GuidedProjectileInputController>().TargetPosition = TargetPosition;
        ATMissile.rigidbody.velocity += transform.parent.rigidbody.velocity; 
        // Set its velocity
        //projectile.rigidbody.velocity = Muzzle.transform.forward * ProjectileSpeed + randomness;
    }

    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, seeker.transform.up);
    }*/
}
