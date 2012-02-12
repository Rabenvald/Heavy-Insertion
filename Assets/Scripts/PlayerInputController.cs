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
        MouseX = Input.GetAxis("Mouse X");
        MouseY = Input.GetAxis("Mouse Y");
        //Strafe = Input.GetAxis("Strafe");
    }
}
