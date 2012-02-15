using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MathTester : MonoBehaviour
{
    private LevelManagerScript manager;

    public GameObject NextWaypoint;
    public GameObject PrevWaypoint;

    public GameObject Objective;

    PathWaypointScript NextWaypointData;
    PathWaypointScript PrevWaypointData;

    private Vector3 transWaypointVector;

    public bool AllowRetreat = false;
    bool AllWaypointsSet = false;

    private Vector3 NearestPointOnPath;
    private Vector3 RelativeVectorFromNext;
    private Vector3 RelativeVectorFromPrev;
    //private Vector3 negNearestPointOnPath;
    private Vector3 RelVectorToPath;

    private GameObject looker;
    private bool justLeftPath = false;
    private bool offThePath = false;
    Vector3 steering;

    public float Mult = 0.7f;

	// Use this for initialization
	void Start () 
    {
        manager = GameObject.Find("LevelManagerContainer").GetComponent<LevelManagerScript>();
        looker = new GameObject();
        looker.transform.parent = transform;
        NearestPointOnPath = Vector3.zero;

        if (manager.AllPathWaypoints.Length > 1)
        {
            if (NextWaypoint == null && PrevWaypoint == null)
            {
                NextWaypoint = FindNearestPathWaypoint();
                SetNextWaypoint();
            }
            else if (NextWaypoint == null)
            {
                SetNextWaypoint();
            }
            else if (PrevWaypoint == null)
            {
                PrevWaypoint = FindNearestPathWaypoint();
            }
            AllWaypointsSet = true;
        }

        NextWaypointData = NextWaypoint.GetComponent<PathWaypointScript>();
        PrevWaypointData = PrevWaypoint.GetComponent<PathWaypointScript>();

        transWaypointVector = NextWaypoint.transform.position - PrevWaypoint.transform.position;
	}

    private GameObject FindNearestPathWaypoint()
    {
        GameObject nearest = manager.AllPathWaypoints[0];
        float nearestDistance = Mathf.Infinity;
        for (int i = 0; i < manager.AllPathWaypoints.Length; i++)
        {
            float thisDistance = Vector3.Distance(rigidbody.transform.position, manager.AllPathWaypoints[i].transform.position);
            if (thisDistance < nearestDistance)
            {
                nearest = manager.AllPathWaypoints[i];
                nearestDistance = thisDistance;
            }
        }
        return nearest;
    }

    public static GameObject[] FindGameObjectsWithLayer(int layer)
    {
        GameObject[] goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        List<GameObject> goList = new List<GameObject>();
        for (int i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].layer == layer)
            {
                goList.Add(goArray[i]);
            }
        }
        if (goList.Count == 0)
        {
            return null;
        }
        return goList.ToArray();
    }

    private void SetNextWaypoint()
    {
        //print("NextWaypointSet");
        PrevWaypoint = NextWaypoint;
        PrevWaypointData = NextWaypoint.GetComponent<PathWaypointScript>();

        if (NextWaypointData == null)
            NextWaypointData = NextWaypoint.GetComponent<PathWaypointScript>();

        NextWaypoint = PrevWaypointData.connections[Random.Range(0, NextWaypointData.connections.Count)];
        NextWaypointData = NextWaypoint.GetComponent<PathWaypointScript>();
        if (!AllowRetreat && NextWaypointData.connections.Count > 1)
        {
            while (NextWaypointData.Index == PrevWaypointData.Index)
            {
                NextWaypoint = NextWaypointData.connections[Random.Range(0, NextWaypointData.connections.Count)];
                NextWaypointData = NextWaypoint.GetComponent<PathWaypointScript>();
            }
        }
        transWaypointVector = NextWaypoint.transform.position - PrevWaypoint.transform.position;
    }

    Vector3 FollowPath()
    {

        Vector3 futurePosition = rigidbody.transform.position + rigidbody.velocity;
        float AngleBetweenMeNext = Mathf.Acos(Vector3.Dot(futurePosition, transWaypointVector) / (futurePosition.magnitude * transWaypointVector.magnitude));

        Vector3 Steering;
        Vector3 RelativeProjection = Vector3.Project(rigidbody.transform.position, transWaypointVector);
        RelVectorToPath = NextWaypoint.transform.position - RelativeProjection;
        NearestPointOnPath = transWaypointVector.normalized * ((futurePosition - PrevWaypoint.transform.position).magnitude + 2 * Mathf.Cos(AngleBetweenMeNext)) + PrevWaypoint.transform.position;

        RelativeVectorFromPrev = NearestPointOnPath - PrevWaypoint.transform.position;
        RelativeVectorFromNext = NearestPointOnPath - NextWaypoint.transform.position;


        float PositionRealitiveToWaypoints = (RelativeVectorFromPrev.normalized + RelativeVectorFromNext.normalized).magnitude;
        print(RelativeVectorFromPrev.normalized + ", " + RelativeVectorFromNext.normalized + ", PRtoW Mag: " + PositionRealitiveToWaypoints);

        Steering = NextWaypoint.transform.position;

        if (PositionRealitiveToWaypoints > 1.5)
        {
            if (!justLeftPath)
            {
                SetNextWaypoint();
            }
            print("Beyond the Path");
            justLeftPath = true;
        }
        else
        {
            justLeftPath = false;

            if (PositionRealitiveToWaypoints < -1)
            {
                Steering = PrevWaypoint.transform.position;
                //print("Seeking PrevWaypoint");
            }
            else if ((NearestPointOnPath - (rigidbody.transform.position + rigidbody.velocity)).magnitude > NextWaypointData.PathRadius)
            {
                //print("Off Path");
                Steering = NearestPointOnPath;
            }
        }

        
        steering = Steering;
        return Steering;
    }

	
	// Update is called once per frame
	void FixedUpdate () 
    {
        FollowPath();
	}

    void OnDrawGizmos()
    {
        if (AllWaypointsSet)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position - Vector3.up, PrevWaypoint.transform.position - Vector3.up);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position - Vector3.up, NextWaypoint.transform.position - Vector3.up);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(PrevWaypoint.transform.position, PrevWaypoint.transform.position + RelativeVectorFromPrev);//NearestPointOnPath);//(PrevWaypoint.transform.position, NearestPointOnPath);

            Gizmos.color = Color.black;
            Gizmos.DrawRay(rigidbody.transform.position, rigidbody.transform.position + RelativeVectorFromPrev);

            Gizmos.color = Color.cyan;
            //Gizmos.DrawRay(PrevWaypoint.transform.position, Vector3.Project(rigidbody.transform.position, transWaypointVector));
            Gizmos.DrawLine(rigidbody.transform.position + (Vector3.up * 0.5f), NearestPointOnPath + (Vector3.up * 0.5f));

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(NextWaypoint.transform.position + (Vector3.up * 0.5f), NextWaypoint.transform.position + RelativeVectorFromNext + (Vector3.up * 0.5f));

            Gizmos.color = Color.white;
            Gizmos.DrawRay(looker.transform.position, 01 * (looker.transform.forward)); 

            Gizmos.DrawRay(PrevWaypoint.transform.position + (Vector3.up * 2), transWaypointVector + (Vector3.up * 2));  //From last waypoint in direction of

        }
    }
}
