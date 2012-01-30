using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathWaypointScript : MonoBehaviour 
{
    //public List<GameObject> Connections { get { return connections; } }
    public List<GameObject> connections = new List<GameObject>();
    public float PathRadius = 5;

    private int index = -1;
    public int Index
    {
        get { return index; }
        set { if (index < 0 ) index = value; }
    }

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {/*
        if (connections.Count > 0)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                int next = i + 1;
                if (next >= connections.Count) next = 0;
                {
                    Vector3 VectorToNextWaypoint = connections[next].transform.position - transform.position;
                    if (VectorToNextWaypoint.magnitude > 0 && !Physics.Raycast(transform.position, VectorToNextWaypoint, VectorToNextWaypoint.magnitude))
                        connections.Add(connections[next]);
                }
            }
        }
	    */
	}

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "WaypointGizmo_small");
        Gizmos.color = Color.yellow;
        if (connections.Count > 1)
        {
            foreach (GameObject w in connections)
            {
                Gizmos.DrawLine(transform.position, w.transform.position);
            }
        }
        /*if (connections.Count > 0)
        {
            
            /*for (int i = 0; i < connections.Count; i++)
            {
                /*int next = i + 1;

                if (next >= connections.Count) 
                    next = 0;
                */
               /* Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, connections[i].transform.position);
            }*/
        //}
    }
}
