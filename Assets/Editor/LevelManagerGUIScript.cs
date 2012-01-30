using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LevelManagerScript))]

public class LevelManagerGUIScript : Editor
{
    private LevelManagerScript manager;
    public GameObject ManagerContainer;
    
	// Use this for initialization
	void Awake () 
    {
        
        if (manager == null)
        {
            GetManager();
        }
	}

    void Start()
    {
        if (ManagerContainer != null)
            manager = ManagerContainer.GetComponent<LevelManagerScript>();

        if (manager == null)
        {
            GetManager();
        }
    }
	
	// Update is called once per frame
    void GetManager()
    {
        manager = GameObject.Find("LevelManagerContainer").GetComponent<LevelManagerScript>();
	}

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Get Manager"))
        {
            GetManager();
        }
        if (GUILayout.Button("Collect all Path Waypoints"))
        {
            manager.GetAllPathWaypoints();
        }
        GUILayout.Label("Last Path Waypoint" + manager.LastPathWaypointIndex);

        if (GUILayout.Button("Setup all Path Waypoint Connections"))
        {
            manager.SetupConnections();
        }
        //GUILayout.Label("All Path Waypoints:" + LevelManagerScript.AllPathWaypoints.ToString());  SetupConnections()
    }
}
