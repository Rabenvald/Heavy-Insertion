using UnityEngine;
using System.Collections;

public class PlayerInputController : InputController
{
    private float prevThrottle;
    public float PrevThrottle
    {
        get
        {
            return prevThrottle;
        }
        set
        {
            prevThrottle = value;
        }
    }
    private float prevPitch;
    public float PrevPitch
    {
        get
        {
            return prevPitch;
        }
        set
        {
            prevPitch = value;
        }
    }
    private float prevRoll;
    public float PrevRoll
    {
        get
        {
            return prevRoll;
        }
        set
        {
            prevRoll = value;
        }
    }
    private float prevYaw;
    public float PrevYaw
    {
        get
        {
            return prevYaw;
        }
        set
        {
            prevYaw = value;
        }
    }
    private float prevStrafe;
    public float PrevStrafe
    {
        get
        {
            return prevStrafe;
        }
        set
        {
            prevStrafe = value;
        }
    }
    private float prevJump;
    public float PrevJump
    {
        get
        {
            return prevJump;
        }
        set
        {
            prevJump = value;
        }
    }

    public Ray ray;
    public RaycastHit hit;
    public Vector3 TargetPosition = Vector3.zero;
    public Transform TargetTransfrom;
    public bool Driving
    {
        get
        {
            return driving;
        }
    }
    private bool driving = true;

    private GameObject mapCamera;
    private GameObject mainCamera;

    void Awake()
    {
        PlayerControlled = true;
    }

	// Use this for initialization
	void Start () 
    {
        turret = GetComponentInChildren<TurretScript>();
        hull = GetComponentInChildren<Hovercraft>();
        mapCamera = GameObject.FindWithTag("MapCamera");
        mainCamera = GameObject.FindWithTag("MainCamera");
        //GameObject mainCamera = GameObject.FindWithTag("MainCamera");
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (!hull.Dead && driving)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 200000.0f)) //, 1 << 9
            {
                TargetPosition = hit.point;
                turret.FinalTargetPosition = TargetPosition;
                //hit.normal;
                //print("Hit something at: " + hit.point);
            }
        }
        /*prevJump = Jump;
        prevPitch = Pitch;
        prevRoll = Roll;
        prevStrafe = Strafe;
        prevThrottle = Throttle;
        prevYaw = Yaw;*/
	}

    void Update()
    {
        Throttle = Input.GetAxis("Vertical");
        Yaw = Input.GetAxis("Horizontal");
        Jump = Input.GetAxis("Jump");
        PrimaryFire = Input.GetButton("Fire1");
        SecondaryFire = Input.GetButton("Fire2");
		Strafe = Input.GetAxis("Strafe");
		
		//Code for map and respawn
        //mapCamera = GameObject.FindWithTag("MapCamera");
		
		if (Input.GetButton("Map"))
		{
            driving = false;
			mapCamera.camera.enabled = true;
            //Camera.main.enabled = false;
			mainCamera.camera.enabled = false;
			RaycastHit hit;
			if(Input.GetMouseButtonDown(0))
			{	
				if(Physics.Raycast(mapCamera.camera.ScreenPointToRay(Input.mousePosition), out hit))
				{
					gameObject.GetComponent<Hovercraft>().respawn(new Vector3(hit.point.x, 2000, hit.point.z));
				}
			}
		}
		else
		{
            driving = true;
			if (mapCamera.camera.enabled != false)
			{
				mapCamera.camera.enabled = false;
                //Camera.main.enabled = true;
                mainCamera.camera.enabled = true;
			}
		}
    }
}
