using UnityEngine;
using System.Collections;

public class ImportantObject : MonoBehaviour {
    public GameObject camTarget;
	// Use this for initialization
	void Start () 
    {
        FindFocus();
	}

    protected void FindFocus()
    {
        if (camTarget == null)
        {
            camTarget = GameObject.Find("CameraFocus");
        }
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void TakeFocus()
    {
        camTarget.transform.parent = transform;
    }

    public void SetFocus(GameObject targ)
    {
        camTarget.transform.parent = targ.transform;
    }

    public void SetFocus(GameObject targ, Vector3 offset)
    {
        camTarget.transform.parent = targ.transform;
        camTarget.transform.localPosition = offset;
    }

    public void TakeFocus(Vector3 offset)
    {
        camTarget.transform.parent = transform;
        camTarget.transform.localPosition = offset;
    }

    public void OffsetFocus(Vector3 offset)
    {
        camTarget.transform.localPosition = offset;
    }
}
