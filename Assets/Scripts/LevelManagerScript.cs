using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManagerScript : MonoBehaviour 
{
    private GameObject[] allPathWaypoints;
    public GameObject[] AllPathWaypoints { get { return allPathWaypoints; } }

    private GameObject[] allEnemies;
    public GameObject[] AllEnemies { get { return allEnemies; } }

    public int LastPathWaypointIndex { get { return lastPathWaypointIndex; } }
    private int lastPathWaypointIndex;

    public float MaxConnectionDistance;

    void Awake()
    {
        /*if (lastPathWaypointIndex == null)
        lastPathWaypointIndex = -1;*/
        GetAllPathWaypoints();
        allEnemies = GameObject.FindGameObjectsWithTag("EnemyAI");
    }

    void Start()
    {

    }

    void FixedUpdate()
    {
        if (allPathWaypoints == null)
        {
            GetAllPathWaypoints();
        }
    }

/*#if UNITY_EDITOR
    // Use this for initialization
	void Start () 
    {

	}

    void Update()
    {
        Debug.Log("Its time: " + Time.time);
    }
#endif*/

    public void SetupConnections()
    {
        RaycastHit hit;

        PathWaypointScript thisPathWaypoint;
        PathWaypointScript otherPathWaypoint;

        if (MaxConnectionDistance == null || MaxConnectionDistance < 0.000001)
            MaxConnectionDistance = 100;

        print("GettingWaypoints...");

        for (int i = 0; i < allPathWaypoints.Length; i++)
        {
            thisPathWaypoint = allPathWaypoints[i].GetComponent<PathWaypointScript>();
            for (int n = 0; n < allPathWaypoints.Length; n++)
            {
                otherPathWaypoint = allPathWaypoints[n].GetComponent<PathWaypointScript>();
                if (!Physics.Raycast(thisPathWaypoint.transform.position, otherPathWaypoint.transform.position - thisPathWaypoint.transform.position, out hit, MaxConnectionDistance) && thisPathWaypoint.Index != otherPathWaypoint.Index)
                {
                    thisPathWaypoint.connections.Add(allPathWaypoints[i]);
                    print("Connecting Waypoint in position: " + i + " with: " + n);
                }
            }
        }
    }

    public void GetAllPathWaypoints()
    {
        allPathWaypoints = GameObject.FindGameObjectsWithTag("PathWaypoints");
        for (int i = 0; i < allPathWaypoints.Length; i++)
        {
            PathWaypointScript lastPathWaypoint = allPathWaypoints[i].GetComponent<PathWaypointScript>();
            if (lastPathWaypoint.Index < 0)
            {
                lastPathWaypointIndex++;
                lastPathWaypoint.Index = lastPathWaypointIndex;
            }
        }
    }
}
