//================================================
// Hovercraft
//================================================
// Parent of all Hover vehicles
//
// Todo: 
// Add self-righting function
// Boost
// Modules
// Impact Effects
// Sounds
// Divide functions for multiplayer support
//================================================

using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AudioSource))]
//[RequireComponent(typeof(InputController))]

public class Hovercraft : ImportantObject
{
    //Optimized for a Rigidbody w/
    //Mass = 5000
    //Drag = 0.1f
    //Angular Drag = 2

    public float suspensionRange = 2.50f;	
    public float suspensionForce = 85000;
    public float suspensionDamp = 8000;

    private RaycastHit hit;
    private Rigidbody parent;

    private GameObject levelParent;

    public float respawnTimer;

    public float topSpeed = 100; //(meters/sec) Broken

    public bool bTouchingGround;

    public float steeringTightness;

    public float throttleInput;
    public Vector3 throttle;

    private BoxCollider HovCollider;

    public int Health = 300;

    private Vector3 CentralRepulsorOffset = Vector3.zero;
    private Vector3 RightRepulsorOffset = Vector3.zero;
    private Vector3 LeftRepulsorOffset = Vector3.zero;
    private Vector3 ForwardRepulsorOffset = Vector3.zero;
    private Vector3 BackRepulsorOffset = Vector3.zero;

    public float dryForwardThrust = 5.0f;
    public float dryYawThrust = 50000.0f;

    public float stabilityFactor = 1.0f;

    //private Vector3 origionalCOM;
    private float origionalDrag;
    private float origionalAngularDrag;

    public float breakForce;

    private float currentVelocity;

    private float jumpInterval;
    public float timeSinceLastJump;
    public float jumpThrust;
	
	private bool dead = false;

    public bool Dead
    {
        get
        {
            return dead;
        }
    }

    private GameObject myterrain;
    private GameObject mySelf;
    private GameObject manager;
    private GameObject mapCamera;
    // Input

    private InputController Controller;

    void Awake()
    {
        levelParent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        levelParent.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        levelParent.transform.parent = transform;
    }

	// Use this for initialization
	void Start () 
    {
        myterrain = GameObject.FindWithTag("Terrain");
        mySelf = GameObject.FindWithTag("Player");
        manager = GameObject.FindWithTag("Manager");
		mapCamera = GameObject.FindWithTag("MapCamera");

		//Debug.Log(gameObject.transform.childCount);
        if (camTarget == null)
        {
            camTarget = GameObject.Find("CameraFocus");
        }

        Controller = GetComponent<InputController>();

        jumpThrust = 100000;

        jumpInterval = 5;
        timeSinceLastJump = jumpInterval;

        breakForce = 1;

        steeringTightness = 0.01f;

        //origionalCOM = rigidbody.centerOfMass;
        origionalDrag = rigidbody.drag;
        origionalAngularDrag = rigidbody.angularDrag;

        bTouchingGround = false;

        levelParent.transform.localEulerAngles = Vector3.zero;
        levelParent.transform.localPosition = Vector3.zero - CentralRepulsorOffset;

        parent = rigidbody;


        HovCollider = GetComponent<BoxCollider>();

        CentralRepulsorOffset = rigidbody.transform.up * transform.localScale.y * HovCollider.size.y * 0.5f - HovCollider.center;
        RightRepulsorOffset = CentralRepulsorOffset + rigidbody.transform.right * transform.localScale.x * HovCollider.size.x * 0.5f; 
        LeftRepulsorOffset = CentralRepulsorOffset - rigidbody.transform.right * transform.localScale.x * HovCollider.size.x * 0.5f; 
        ForwardRepulsorOffset = CentralRepulsorOffset + rigidbody.transform.forward * transform.localScale.z * HovCollider.size.z * 0.5f;
        BackRepulsorOffset = CentralRepulsorOffset - rigidbody.transform.forward * transform.localScale.z * HovCollider.size.z * 0.5f; 

        rigidbody.centerOfMass = -(CentralRepulsorOffset + CentralRepulsorOffset * 0.5f);

        //Camera.main.transform.parent = transform;
        //Camera.main.transform.localPosition = new Vector3(0.0f, 5.0f, -42.0f);
        //Camera.main.transform.eulerAngles = new Vector3(4.150256f, 0, 0);
        /*if (Controller.PlayerControlled)
            TakeFocus (new Vector3(0, 0.5f, 0));*/
	}
	
	// Update is called once per frame
	void Update () 
    {
        //audio.pitch = Mathf.Clamp(rigidbody.velocity.magnitude / 30, 1, 4.0f);
    }


    void OnCollisionEnter(Collision other ) 
    {

        // if we impact the ground at some point:
        //Todo: add Impact Effects/Sounds here
        int damage = (int)(other.impactForceSum.magnitude);
        if (respawnTimer > 0 && other.gameObject == myterrain)
        {
            //Do nothing
        }
        else if (damage > 50)
        {
            Health -= damage;
        }
        //if (Health < 0) explode();
    }

    void ApplySteering(float throttleInput, float yawInput, float jumpAndBreakInput, float strafeInput)
    {
        timeSinceLastJump += Time.deltaTime;

        float effectiveBreakForce = breakForce;
        float effectiveJumpThrust = jumpThrust;
        float yawThrottle = yawInput * dryYawThrust;

        if (rigidbody.velocity.magnitude < topSpeed)
        {
            throttle = rigidbody.transform.forward.normalized * dryForwardThrust * throttleInput;
        }

        else
        {
            rigidbody.velocity = topSpeed * rigidbody.velocity.normalized;
            throttle = rigidbody.velocity;
        }

        if (bTouchingGround && Controller.Throttle >= 0)
        {
            rigidbody.velocity = (rigidbody.velocity.normalized + rigidbody.transform.forward * steeringTightness).normalized * rigidbody.velocity.magnitude;  //induce lateral drag
        }

        else
        {
            throttle /= 2;
            yawThrottle /= 10;
            rigidbody.drag = 0.2f;
            rigidbody.angularDrag = 0.05f;
            effectiveBreakForce /= 2;
            effectiveJumpThrust /= 2;
        }
        rigidbody.AddForce(throttle + Physics.gravity, ForceMode.Acceleration);
        rigidbody.AddRelativeTorque(0.0f, yawThrottle, 0.0f, ForceMode.Force);
        rigidbody.AddForce(rigidbody.transform.right * 3 * strafeInput, ForceMode.Acceleration);
        if (jumpAndBreakInput > 0 && timeSinceLastJump >= jumpInterval) //Jump
        {
            rigidbody.AddForce(rigidbody.transform.up * effectiveJumpThrust, ForceMode.Impulse);
            timeSinceLastJump = 0;
        }
        if (jumpAndBreakInput < 0) //Apply Breaks
            rigidbody.drag = effectiveBreakForce;
    }

    void FixedUpdate()
    {
        /*timeSinceLastJump += Time.deltaTime;

        float effectiveBreakForce = breakForce;
        float effectiveJumpThrust = jumpThrust;
        float yawThrottle = 0.0f;*/

        if (!dead)
        {
            if (Health <= 0)
            {
                dead = true;
                if (mySelf == gameObject) //GameObject.FindWithTag("Player")
                {
					Manager.Instance.Spawned = false;	
				}

                SetFocus(manager);//GameObject.FindWithTag("Manager")
				
                GameObject.Destroy(gameObject);
				
				//GameObject temp = GameObject.FindWithTag("MapCamera");
				mapCamera.camera.enabled = true;
				
				/*gameObject.renderer.enabled = false;
                gameObject.transform.GetComponentInChildren<TurretScript>().enabled = false;
                Renderer[] childRenderers = gameObject.transform.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in childRenderers)
                {
                    r.enabled = false;
                }//*/
				return;
            }
        
	        if (respawnTimer > 0)
	            respawnTimer -= Time.fixedDeltaTime;
	
	        currentVelocity = rigidbody.velocity.magnitude;
	
	        rigidbody.drag = origionalDrag;
	        rigidbody.angularDrag = origionalAngularDrag;
	
	        bTouchingGround = false;
	
	        CentralRepulsorOffset = rigidbody.transform.up * transform.localScale.y * HovCollider.size.y * 0.49f - HovCollider.center;
	        RightRepulsorOffset = CentralRepulsorOffset + rigidbody.transform.right * transform.localScale.x * HovCollider.size.x * 0.5f;
	        LeftRepulsorOffset = CentralRepulsorOffset - rigidbody.transform.right * transform.localScale.x * HovCollider.size.x * 0.5f;
	        ForwardRepulsorOffset = CentralRepulsorOffset + rigidbody.transform.forward * transform.localScale.z * HovCollider.size.z * 0.5f;
	        BackRepulsorOffset = CentralRepulsorOffset - rigidbody.transform.forward * transform.localScale.z * HovCollider.size.z * 0.5f;
	
	        Lift(CentralRepulsorOffset);
	        Lift(RightRepulsorOffset);
	        Lift(LeftRepulsorOffset);
	        Lift(ForwardRepulsorOffset);
	        Lift(BackRepulsorOffset);
	
	        // lock down rotation
	        levelParent.transform.rotation = Quaternion.identity;
	
	        ApplySteering(Controller.Throttle, Controller.Yaw, Controller.Jump, Controller.Strafe);
	
	        //================================= Sounds ==============================
	        audio.pitch = currentVelocity / topSpeed + throttle.magnitude / 100 + .5f;
	        audio.volume = currentVelocity / topSpeed + throttle.magnitude / 100 + .5f;
		}
        
    }

    void Lift(Vector3 offset)
    {
        //Vector3 down = transform.TransformDirection(Vector3.down);
        Vector3 worldDown = levelParent.transform.TransformDirection(Vector3.down);

        if (Physics.Raycast(transform.position - offset, worldDown, out hit, suspensionRange) && hit.collider.transform.root != transform.root)
        {
            Vector3 velocityAtTouch = parent.GetPointVelocity(hit.point);

            float compression = hit.distance / (suspensionRange);
            compression = -compression + 1;
            Vector3 counterForce = -worldDown * compression * suspensionForce;

            Vector3 t = transform.InverseTransformDirection(velocityAtTouch);
            t.z = t.x = 0;
            Vector3 shockDrag = levelParent.transform.TransformDirection(t) * -suspensionDamp;

            parent.AddForceAtPosition(counterForce + shockDrag, hit.point);
            bTouchingGround = true;
        }
    }

    void OnGUI()
    {
        if (Controller.PlayerControlled)
        {
            GUI.Box(new Rect(Screen.width * 0.9f, Screen.height * 0.9f, 100, 25), (int)(currentVelocity * 3.6f) + " km/h");
        }
    }

    // Show yellow Rays (debug only)
    void OnDrawGizmos()
    {
        if (levelParent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position - CentralRepulsorOffset, levelParent.transform.TransformDirection(Vector3.up) * -suspensionRange);
            Gizmos.DrawRay(transform.position - RightRepulsorOffset, levelParent.transform.TransformDirection(Vector3.up) * -suspensionRange);
            Gizmos.DrawRay(transform.position - LeftRepulsorOffset, levelParent.transform.TransformDirection(Vector3.up) * -suspensionRange);
            Gizmos.DrawRay(transform.position - ForwardRepulsorOffset, levelParent.transform.TransformDirection(Vector3.up) * -suspensionRange);
            Gizmos.DrawRay(transform.position - BackRepulsorOffset, levelParent.transform.TransformDirection(Vector3.up) * -suspensionRange);
        }
    }
	
	public void respawn(Vector3 pos)
	{
        gameObject.renderer.enabled = true;
        gameObject.transform.GetComponentInChildren<TurretScript>().enabled = true;
        Renderer[] childRenderers = gameObject.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in childRenderers)
        {
            r.enabled = true;
        }
        respawnTimer = 30;
        dead = false;
        Health = 300;
        gameObject.transform.position = pos;
	}

    void OnDestroy()
    {
        //Explode
        //Hide mesh
        Destroy(levelParent); //Kill Children
        //Kill Explosion
        //Kill self
    }
}
