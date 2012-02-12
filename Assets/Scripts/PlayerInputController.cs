using UnityEngine;
using System.Collections;

public class PlayerInputController : InputController
{
    public Ray ray;
    public RaycastHit hit;
    public Vector3 TargetPosition = Vector3.zero;
    public Transform TargetTransfrom;


    void Awake()
    {
        PlayerControlled = true;
    }

	// Use this for initialization
	void Start () 
    {
        turret = GetComponentInChildren<TurretScript>();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 8000.0f)) //, 1 << 9
        {
            TargetPosition = hit.point;
            turret.FinalTargetPosition = TargetPosition;
            //hit.normal;
            //print("Hit something at: " + hit.point);
        }
        
        //int hgjh = Input.GetMouseButton(1);
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
		GameObject mapCamera = GameObject.FindWithTag("MapCamera");
		GameObject mainCamera = GameObject.FindWithTag("MainCamera");
		if (Input.GetButton("Map"))
		{	
			mapCamera.camera.enabled = true;
			mainCamera.camera.enabled = false;
			RaycastHit hit;
			if(Input.GetMouseButtonDown(0))
			{
				if(Physics.Raycast(mapCamera.camera.ScreenPointToRay(Input.mousePosition), out hit))
				{
					gameObject.transform.position = new Vector3(hit.point.x, 2000, hit.point.z);
					Hovercraft hover = gameObject.GetComponent<Hovercraft>();
					hover.respawnTimer = 30;
				}
			}
		}
		else
		{
			if (mapCamera.camera.enabled != false)
			{
				mapCamera.camera.enabled = false;
				mainCamera.camera.enabled = true;
			}
		}
    }
}
