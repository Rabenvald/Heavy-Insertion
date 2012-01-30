using UnityEngine;
using System.Collections;

public class ImportantObject : MonoBehaviour {
    public GameObject target;
	// Use this for initialization
	void Start () 
    {
        if (target == null)
        {
            target = GameObject.Find("CameraFocus");
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void TakeFocus()
    {
        target.transform.parent = transform;
    }

    public void TakeFocus(Vector3 offset)
    {
        target.transform.parent = transform;
        target.transform.localPosition = offset;
    }

    public void OffsetFocus(Vector3 offset)
    {
        target.transform.localPosition = offset;
    }
}
