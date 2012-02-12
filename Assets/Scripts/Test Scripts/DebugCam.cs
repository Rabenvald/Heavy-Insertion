using UnityEngine;
using System.Collections;

public class DebugCam : MonoBehaviour
{
    
    float Pitch;
    float Yaw;
    float XInput;
    float YInput;
    float ZInput;

    public bool PrimaryFire;
    public bool SecondaryFire;

    public float MoveSpeed = 1;
    public float RotateSpeed = 1;

    public Ray ray;
    public RaycastHit hit;
    public Vector3 TargetPosition = Vector3.zero;

	void FixedUpdate () 
    {
        transform.position += transform.right * XInput;   //new Vector3(XInput, YInput, ZInput);
        transform.position += transform.up * YInput;
        transform.position += transform.forward * ZInput;
        //transform.eulerAngles = Input.mousePosition;

        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.EulerAngles(Input.mousePosition), 5);
        //transform.LookAt(transform.forward + transform.position, Vector3.up);
        //transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * Quaternion.Euler(new Vector3(-Pitch * Time.deltaTime * 500, Yaw * Time.deltaTime * 500, 0)), 500);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((transform.rotation * Quaternion.Euler(new Vector3(-Pitch * Time.deltaTime * 500, Yaw * Time.deltaTime * 500, 0))).eulerAngles, Vector3.up), 500);
        transform.rotation = transform.rotation * Quaternion.Euler(new Vector3(-Pitch * Time.deltaTime * 500, Yaw * Time.deltaTime * 500, 0));
        //Quaternion.LookRotation((transform.rotation * Quaternion.Euler(new Vector3(-Pitch * Time.deltaTime * 500, Yaw * Time.deltaTime * 500, 0))).eulerAngles, Vector3.up);

        if (Pitch != 0 || Yaw != 0)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 8000.0f)) //, 1 << 9
            {
                TargetPosition = hit.point;
            }
        }

	}
	
	void Update () 
    {
        ZInput = Input.GetAxis("Vertical");
        XInput = Input.GetAxis("Horizontal");
        YInput = Input.GetAxis("Jump");
        Pitch = Input.GetAxis("Mouse Y");
        Yaw = Input.GetAxis("Mouse X");
        PrimaryFire = Input.GetButton("Fire1");
        SecondaryFire = Input.GetButton("Fire2");

        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(TargetPosition), Time.time * RotateSpeed);
	}
}
