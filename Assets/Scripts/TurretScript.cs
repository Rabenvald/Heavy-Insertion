using UnityEngine;
using System.Collections;

public class TurretScript : ImportantObject
{
    //public GameObject MainHull;
    private InputController Controller;

    public GameObject PitchComponet;
    public GameObject Muzzle;
    public float StabilizationAmount = 0.5f;
    //public float RotationRate = 1;

    public float SlerpRotationRate = 1;

    private float MinXRot = -30;
    private float MaxXRot = 45;
    private float XRot = 0;

    private float YRot = 0;

    private float MaxTargetDistance = 200000;

    //Vector3 CurBodyRotation = Vector3.zero;
    //Vector3 CurPitchComponetRotation = Vector3.zero;


    Quaternion targetRotation = Quaternion.identity;
    //Quaternion YawtargetRotation = Quaternion.identity;

    //private GameObject seeker;
    private GameObject camGimbal;

    public GameObject Projectile;
    public GameObject Missile;
    public GameObject target;
    public Vector3 TargetPosition = Vector3.zero;
    public Vector3 FinalTargetPosition = Vector3.zero;
    public Vector3 TargetVector = Vector3.zero;
    public Transform TargetTransform;

    private MainCameraScript mainCamScript;

    public float ProjectileSpeed = 4000.0f;
    public float FireRate = 1.5f;
	
	public float respawnTimer;
	
    private float lastFired = 0.0f;

    private Vector3 randomness = Vector3.zero;

    /* Camera rotates seperately from turret (attched to new gameObject on the hull
     * Camera raycast to screen and assigns future position
     * Target lerps to future position
     * Target.position = Lerp (FuturePos, RotationRate * Vector3.Distance(FuturePos.normalized, Target.transform.position.normalized))
     * 
     */

    void Awake()
    {
        FindFocus();

        //seeker = new GameObject();
        //seeker.transform.parent = transform;

        camGimbal = new GameObject();
        camGimbal.transform.parent = transform.parent;
        camGimbal.transform.localPosition = transform.localPosition;
    }
	void Start () 
    {
        Controller = transform.parent.GetComponent<InputController>();
        if (StabilizationAmount < 0)
            StabilizationAmount *= -1;
        if (transform.parent.GetComponent<InputController>().PlayerControlled)
            SetFocus(camGimbal, new Vector3(0, 0.5f, 0));

        mainCamScript = GameObject.FindWithTag("MainCamera").GetComponent<MainCameraScript>();
		
		respawnTimer = 5;
	}
    void Update()
    {
        //camGimbal.transform.localRotation = Quaternion.Euler(0, (float)camGimbal.transform.localRotation.y + Controller.MouseX, 0);//Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, camGimbal.transform.localRotation.y + Controller.MouseX, 0), SlerpRotationRate);
        //camGimbal.transform.rotation = camGimbal.transform.rotation * Quaternion.Euler(new Vector3(-Controller.MouseY * Time.deltaTime * 100, Controller.MouseX * Time.deltaTime * 100, 0));
    }
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        //CurBodyRotation = transform.rotation.eulerAngles;
        //CurPitchComponetRotation = transform.rotation.eulerAngles;

        //TargetPosition = Vector3.Lerp(TargetPosition, FinalTargetPosition, Mathf.Clamp(RotationRate / Vector3.Distance(FinalTargetPosition.normalized, TargetPosition.normalized), 0.000001f, 2));
		
		if (respawnTimer > 0)
			respawnTimer -= Time.fixedDeltaTime;
		
        if (target != null)
        {

            targetRotation = /*Quaternion.LookRotation(TrackingComputer.GetParabolicFiringSolution(PitchComponet.transform.position, target.transform.position, ProjectileSpeed, Physics.gravity, transform.parent.rigidbody.velocity, target.rigidbody.velocity));*/
                Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TrackingComputer.GetParabolicFiringSolution(PitchComponet.transform.position, target.transform.position, ProjectileSpeed, Physics.gravity, transform.parent.rigidbody.velocity, target.rigidbody.velocity)), Time.deltaTime * 1000/*SlerpRotationRate*/);

            //targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TrackingComputer.GetParabolicFiringSolution(PitchComponet.transform.position, target.transform.position, ProjectileSpeed, Physics.gravity, transform.parent.rigidbody.velocity, target.rigidbody.velocity)), Time.deltaTime * 1000f);
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
                targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-TargetVector), Time.deltaTime * 1000); //SlerpRotationRate
                
                //targetRotation.eulerAngles = Vector3.RotateTowards(transform.rotation.eulerAngles, Quaternion.LookRotation(-TargetVector).eulerAngles, 0.1f, SlerpRotationRate);

            }
            
        }

        PitchComponet.transform.eulerAngles = new Vector3(Mathf.Clamp(targetRotation.eulerAngles.x, -MaxTargetDistance, MaxTargetDistance), Mathf.Clamp(transform.eulerAngles.y, -MaxTargetDistance, MaxTargetDistance), Mathf.Clamp(transform.eulerAngles.z, -MaxTargetDistance, MaxTargetDistance));

        //PitchComponet.transform.localEulerAngles = new Vector3(Mathf.Clamp(PitchComponet.transform.localEulerAngles.x, MinXRot, MaxXRot), PitchComponet.transform.localEulerAngles.y, PitchComponet.transform.localEulerAngles.z);

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, YRot, transform.eulerAngles.z);
        //seeker.transform.LookAt(transform.parent.transform.up, TargetPosition-transform.position);
        transform.LookAt(TargetPosition, transform.parent.transform.up);
        
        transform.localEulerAngles = new Vector3(0,transform.localEulerAngles.y,0);

        if (mainCamScript.typing == false && respawnTimer <= 0)
        {
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
    }

    private void Fire()
    {
        randomness.x = Random.Range(-2, 2);
        randomness.y = Random.Range(-2, 2);
        randomness.z = Random.Range(-2, 2);
		
		//had projectiles spawn furth out, to avoid collision with self. Helps, but no fix
        GameObject projectile = (GameObject)Instantiate(Projectile, Muzzle.transform.position + (Muzzle.transform.forward * 3), Muzzle.transform.rotation);

        // Set its velocity
        projectile.rigidbody.velocity = Muzzle.transform.forward * ProjectileSpeed + randomness + transform.parent.rigidbody.velocity;
        // Recoil
        transform.parent.rigidbody.AddForceAtPosition(-projectile.rigidbody.velocity * projectile.rigidbody.mass, Muzzle.transform.position, ForceMode.Impulse);
		
		
		projectile.GetComponent<NetTag>().Id = Manager.Instance.myId + "-1" + "-" + Manager.Instance.PrimaryCount;
		Manager.Instance.updatePhysList();
		Manager.Instance.PrimaryCount++;
        Manager.Instance.sendAttack(projectile, projectile.transform.position, projectile.transform.rotation.eulerAngles, projectile.rigidbody.velocity);
    }

    private void FireMissile()
    {
        GameObject ATMissile = (GameObject)Instantiate(Missile, Muzzle.transform.position + (Muzzle.transform.forward * 3), Muzzle.transform.rotation);
        ATMissile.GetComponent<GuidedProjectileInputController>().TargetPosition = TargetPosition;
        ATMissile.rigidbody.velocity += transform.parent.rigidbody.velocity; 
        // Set its velocity
		
		ATMissile.GetComponent<NetTag>().Id = Manager.Instance.myId + "-2" + "-" + Manager.Instance.SecondaryCount;
		Manager.Instance.updatePhysList();
		Manager.Instance.SecondaryCount++;
        Manager.Instance.sendAttack(ATMissile, ATMissile.transform.position, ATMissile.transform.rotation.eulerAngles, ATMissile.rigidbody.velocity, TargetPosition);
    }

    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, seeker.transform.up);
    }*/
}
