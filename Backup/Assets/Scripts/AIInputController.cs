using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIInputController : InputController
{
    List<GameObject> myPath;

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

    public float PathfollowingFudgeFactor = 0;

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
            if ( thisDistance < nearestDistance)
            {
                nearest = manager.AllPathWaypoints[i];
                nearestDistance = thisDistance;
            }
        }
        return nearest;
    }

	// Update is called once per frame
	void FixedUpdate () 
    {
        if (Objective != null)
        {
            Approach(Objective);
        }
        else
        {
            Approach(FollowPath());
        }
	}

    public Vector3 Seek(Vector3 pos) //returns realitive pos
    {
        return (pos - rigidbody.transform.position).normalized;
    }

    public Vector3 Flee(Vector3 pos) //returns realitive pos
    {
        return (rigidbody.transform.position - pos).normalized;
    }

    // same as seek pos above, but parameter is game object
    public Vector3 Seek(GameObject gO)
    {
        return Seek(gO.transform.position);
    }

    public Vector3 Flee(GameObject gO)
    {
        return Flee(gO.transform.position);
    }
    
    private void SetNextWaypoint()
    {
        //print("NextWaypointSet"); 
        PrevWaypoint = NextWaypoint;
        PrevWaypointData = NextWaypoint.GetComponent<PathWaypointScript>();

        if (NextWaypointData == null)
        {
            NextWaypointData = NextWaypoint.GetComponent<PathWaypointScript>();
        }

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
        NearestPointOnPath = transWaypointVector.normalized * ((futurePosition - PrevWaypoint.transform.position).magnitude + PathfollowingFudgeFactor * Mathf.Cos(AngleBetweenMeNext)) + PrevWaypoint.transform.position;


        RelativeVectorFromPrev = NearestPointOnPath - PrevWaypoint.transform.position;
        RelativeVectorFromNext = NearestPointOnPath - NextWaypoint.transform.position;

        float PositionRealitiveToWaypoints = (RelativeVectorFromPrev.normalized + RelativeVectorFromNext.normalized).magnitude;
        //print(RelativeVectorFromPrev.normalized + ", " + RelativeVectorFromNext.normalized + ", Mag: " + PositionRealitiveToWaypoints);

        Steering = NextWaypoint.transform.position;//Seek(NextWaypoint);

        if (PositionRealitiveToWaypoints > 1.5)
        {
            if (!justLeftPath)
            {
                SetNextWaypoint();
            }
            //print("Beyond the Path");
            justLeftPath = true;
        }
        else
        {
            justLeftPath = false;

            if (PositionRealitiveToWaypoints < -1)
            {
                Steering = PrevWaypoint.transform.position;//Seek(PrevWaypoint);
                //print("Seeking PrevWaypoint");
            }
            else if ((NearestPointOnPath - (rigidbody.transform.position + rigidbody.velocity)).magnitude > NextWaypointData.PathRadius)
            {
                //print("Off the Path");//******************************************************
                Steering = NearestPointOnPath;
            }
        }

        steering = Steering;
        return Steering;
    }

    void Approach(Vector3 targetPosition) // + rigidbody.velocity
    {
        // NOT FIXED ===================================================================================================VVVVVVVVVVVVVVVVVVVVVVVVV
        targetPosition -= rigidbody.velocity;
        looker.transform.LookAt(targetPosition, rigidbody.transform.up);
        Vector3 myOrientation = (rigidbody.transform.right).normalized; 
        looker.transform.position = rigidbody.transform.position;
        Vector3 targetOrientation = (looker.transform.forward).normalized; 

        //print("Going to: "+ targetPosition); //*******************************************


        float zxDot = Vector3.Dot(myOrientation, targetOrientation);
        float zzDot = Vector3.Dot(rigidbody.transform.forward, targetOrientation);

        float degOffBore = -Mathf.Asin(zxDot) * Mathf.Rad2Deg;  //turn only in ZX plane (Yaw)   (Acos)

        Yaw = zxDot;

        if (degOffBore == 0)
        {
            Throttle = 1;
        }
        else
        {
            if (zzDot < 0)
            {
                Throttle = -Mathf.Abs(1 / degOffBore);
                Jump = -1;
                //Yaw *= -1;
            }
            else
            {
                Throttle = Mathf.Abs(1 / degOffBore);
                Jump = 0;
            }
        }


        // Slow down if we ae going past our target
        Vector3 VecToTarg = targetPosition - rigidbody.transform.position;
        if (((VecToTarg + rigidbody.velocity).normalized - VecToTarg.normalized).magnitude > 1)
        {
            Jump = -1;
        }

        //

        Jump = Mathf.Clamp(Jump, -1, 1);
        Yaw = Mathf.Clamp(Yaw, -1, 1);
        Throttle = Mathf.Clamp(Throttle, -1, 1);
    }

    void Approach(GameObject target)
    {
        Approach(target.transform.position);
    }

    void OnDrawGizmos()
    {
        if (AllWaypointsSet)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position - Vector3.up, PrevWaypoint.transform.position);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position - Vector3.up, NextWaypoint.transform.position);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(PrevWaypoint.transform.position, PrevWaypoint.transform.position + RelativeVectorFromPrev + (Vector3.up * 0.5f));//NearestPointOnPath);//(PrevWaypoint.transform.position, NearestPointOnPath);

            //Gizmos.color = Color.black;
            //Gizmos.DrawRay(rigidbody.transform.position, rigidbody.transform.position + RelativeVectorFromPrev);

            //Gizmos.color = Color.cyan;
            //Gizmos.DrawRay(transform.position, steering);                                                                                                                                                                                                                                                                                                                                                                                                    

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(NextWaypoint.transform.position, NextWaypoint.transform.position + RelativeVectorFromNext + (Vector3.up * 0.5f));

            Gizmos.color = Color.white;
            Gizmos.DrawRay(looker.transform.position, 01 * (looker.transform.forward));

            //Gizmos.DrawRay(PrevWaypoint.transform.position + (Vector3.up * 2), transWaypointVector + (Vector3.up * 2));
            
        }
    }

}
