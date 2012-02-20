using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
 
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Exceptions;

public class Manager : MonoBehaviour 
{
	private SmartFox smartFox; 
	private TestLobby lobby;
	private Room currentRoom;
	private GameObject[] PhysObjects;

    private PlayerInputController localController;

	//important prefabs
	public GameObject daTank;
	public GameObject playerTank;
	public GameObject cameraFocus;
	public GameObject heatProjectile;
	public GameObject ATMissile;
	
	//my info
	public String myId;
	private GameObject myTank;
	
	
	private string clientName;
	public string ClientName 
    {
		get { return clientName;}
	}
	
	private static Manager instance;
	public static Manager Instance 
    {
		get { return instance;}
	}
	
	private static bool isPhysAuth;
	public bool IsPhysAuth{
		get { return isPhysAuth; }	
	}
	
	private static bool spawned;
	public bool Spawned{
		get { return spawned; }
        set { spawned = value; }
	}
	
	private static int primaryCount;
	public int PrimaryCount{
		get { return primaryCount; }
		set { primaryCount = value; }
	}
	
	private static int secondaryCount;
	public int SecondaryCount{
		get { return secondaryCount; }	
		set { secondaryCount = value; }
	}
	
    public float TimeBetweenUpdates = 0.2f;
    private float LastUpdateTime = 0;
    private uint ObjectSent = 0;

	void Awake() 
    {
		instance = this;
	}
	
	void Start () 
    {
		if (SmartFoxConnection.IsInitialized)
        {
			smartFox = SmartFoxConnection.Connection;
		}
		else
        {
            smartFox = new SmartFox(Application.isEditor);
		}
		
		currentRoom = smartFox.LastJoinedRoom;
		clientName = smartFox.MySelf.Name;

		if(currentRoom.UserCount == 1)
        {
			isPhysAuth = true;	
		}
		else
        {
			isPhysAuth = false;	
		}
		
		spawned = false;

		updatePhysList();
		
		myId = smartFox.MySelf.Id.ToString();
		
		smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
		smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
		smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessageReceived);
		smartFox.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariablesUpdate); 
		smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariablesUpdate);
        //smartFox.AddEventListener(SFSEvent.UDP_INIT, OnUDPInit);
		//smartFox.AddEventListener(SFSEvent.EXTENSION_RESPONSE, onExtensionResponse);
        //smartFox.InitUDP("129.21.29.6", 9933); //FIX THIS: Should be dynamic ========================================
	}

    void OnUDPInit(BaseEvent evt) 
    {
        if ((bool)evt.Params["success"]) 
        {
            // Execute an extension call via UDP
            smartFox.Send(new ExtensionRequest("udpTest", new SFSObject(), null, true));
        } 
        else 
        {
            Console.WriteLine("UDP init failed!");
        }
    }
	
	void FixedUpdate () 
    {
		smartFox.ProcessEvents();
		updatePhysList();
		
		if(spawned)
			sendInputs();

        if (isPhysAuth)
        {
            if (LastUpdateTime >= TimeBetweenUpdates)
            {
                if (ObjectSent < PhysObjects.Length)
                    ObjectSent = 0;
                sendTelemetry(PhysObjects[ObjectSent]);
                ObjectSent++;

                LastUpdateTime = 0;
            }
            LastUpdateTime += Time.deltaTime;
        }
	}
	
	public void OnUserEnterRoom (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];		
		Debug.Log ("user entered room " + user.Name + " with id of " + user.Id);
	}

	private void spawnTank(Vector3 pos, User user){
		GameObject tank = (GameObject)Instantiate(daTank, pos, Quaternion.identity);
        InputController ic = tank.GetComponent<InputController>();
		ic.id = user.Id.ToString();  //ID Schema: UserId + Type + InstanceNumber
	}
	
	
	public void spawnMe(Vector3 pos){
		//GameObject cF = Instantiate(cameraFocus, pos, Quaternion.identity) as GameObject;
		GameObject tank = Instantiate(playerTank, pos, Quaternion.identity) as GameObject;
		//tank.GetComponent<Hovercraft>().SetFocus(cF);
		myTank = tank;
		updatePhysList();
        localController = GetLocalController();
	}
	
	private void spawnTank(User user, Vector3 pos){
		GameObject tank = (GameObject)Instantiate(daTank, pos, Quaternion.identity);
        NetTag nt = tank.GetComponent<NetTag>();
		nt.Id = user.Id.ToString() + "-00-" + "00";  //ID Schema: UserId + Type + InstanceNumber
	}
	
	public void OnUserLeaveRoom (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
	}
	
	public void OnUserCountChange (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
		
		Debug.Log ("User count change based on " + user.Name + " with user Id of " + user.Id);
	}
	
	public void OnConnectionLost (BaseEvent evt)
    {
		smartFox.RemoveAllEventListeners();
	}

    public void OnObjectMessageReceived(BaseEvent evt) //You do not recieve these messages from yourself
    {
		User sender = (User)evt.Params["sender"];
		ISFSObject obj = (SFSObject)evt.Params["message"];
        NetInputController remoteController;

        if (obj.GetUtfString("PID") == myId)
        {
            //localController;
            if (obj.ContainsKey("PhysMaster"))
            {
                if (obj.GetBool("PhysMaster"))
                {
                    //remoteController = GetRemoteController(obj.GetUtfString("PID"));

                    //localController.Extrapolate();

                    localController.Hull.transform.position = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));

                    localController.Hull.transform.rotation = Quaternion.Euler(new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")));

                    localController.Hull.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));

                    //remoteController.Hull.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));

                    //localController.TimeSinceLastUpdate = Time.time;
                }
            }

        }
        else if (GetRemoteController(obj.GetUtfString("PID")) != null)
        {
            if (obj.ContainsKey("PhysMaster"))
            {
                if (obj.GetBool("PhysMaster"))
                {
                    remoteController = GetRemoteController(obj.GetUtfString("PID"));

                    remoteController.Extrapolate();

                    remoteController.Hull.transform.position = remoteController.LastPosition = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz")) + remoteController.PositionExtrapolation;

                    remoteController.Hull.transform.rotation = Quaternion.Euler(remoteController.LastRotation = new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")) + remoteController.RotationExtrapolation);

                    remoteController.Hull.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));
                    
                    //remoteController.Hull.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));

                    remoteController.TimeSinceLastUpdate = Time.time;
                }
            }

            if (obj.ContainsKey("inputs"))
            {
                remoteController = GetRemoteController(obj.GetUtfString("PID"));
                if (obj.ContainsKey("iT"))
                    remoteController.Throttle = obj.GetFloat("iT");
                if (obj.ContainsKey("iP"))
                    remoteController.Pitch = obj.GetFloat("iP");
                if (obj.ContainsKey("iR"))
                    remoteController.Roll = obj.GetFloat("iR");
                if (obj.ContainsKey("iY"))
                    remoteController.Yaw = obj.GetFloat("iY");
                if (obj.ContainsKey("iS"))
                    remoteController.Strafe = obj.GetFloat("iS");
                if (obj.ContainsKey("iJ"))
                    remoteController.Jump = obj.GetFloat("iJ");
            }
        }
		else if(obj.ContainsKey("spawnPos")){
			SFSObject spawn = (SFSObject) obj.GetSFSObject("spawnPos");
			Vector3 pos = new Vector3(spawn.GetFloat("x"), spawn.GetFloat("y"), spawn.GetFloat("z"));
			
			spawnTank(pos, sender);
			Debug.Log("Spawn Location = " + spawn.GetFloat("x") + ", " + spawn.GetFloat("y") + ", " + spawn.GetFloat("z"));
		}
		else if(obj.GetUtfString("Command") == "CreateAttack"){
			string id = obj.GetUtfString("Id");
			
			//parse id to determine type
			string[] temp = id.Split('-');
			Debug.Log(temp[0] + " " + temp[1] + " " + temp[2]);
			int type = int.Parse(temp[1]);
			
			//switch to determine what to create
			switch(type){
				case 1: //projectile
					Debug.Log("created projectile for player " + temp[0]);
                    //GameObject.Instantiate(ATMissile, Muzzle.transform.position + Muzzle.transform.forward * 3, Muzzle.transform.rotation);
                    //create projectile  //transform.root.gameObject
					//give it the values
					//give it the id
					break;
				case 2: //missile
					Debug.Log("created missile for player " + temp[0]);
					//create missile
					//give it the values
					//give it the id
					break;
				default:
					break;
			}
			
			Debug.Log("Attack Id: " + id);
		}
	}
	
	public void OnUserVariablesUpdate (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
	}
	
	public void OnRoomVariablesUpdate (BaseEvent evt)
    {
		Room room = (Room)evt.Params["room"];
	}
	
	public void onExtensionResponse(BaseEvent evt)
    {
		
	}
	
	private void sendTelemetry(GameObject gO)
    {
        SFSObject myData = new SFSObject();
        myData.PutFloat("px", gO.transform.position.x);
        myData.PutFloat("py", gO.transform.position.y);
        myData.PutFloat("pz", gO.transform.position.z);

        myData.PutFloat("rx", gO.transform.rotation.eulerAngles.x);
        myData.PutFloat("ry", gO.transform.rotation.eulerAngles.y);
        myData.PutFloat("rz", gO.transform.rotation.eulerAngles.z);

        myData.PutFloat("vx", gO.rigidbody.velocity.x);
        myData.PutFloat("vy", gO.rigidbody.velocity.y);
        myData.PutFloat("vz", gO.rigidbody.velocity.z);

        smartFox.Send(new ObjectMessageRequest(myData));
	}

    private void sendInputs()
    {
        if ((localController.PrevThrottle != localController.Throttle) || (localController.Pitch != localController.Pitch) || 
            (localController.PrevRoll != localController.Roll) || (localController.PrevYaw != localController.Yaw) || 
            (localController.PrevStrafe != localController.Strafe) || (localController.PrevJump != localController.Jump))
        {
            SFSObject myData = new SFSObject();

            myData.PutUtfString("PID", myId);

            myData.PutBool("inputs", true);

            if (localController.PrevThrottle != localController.Throttle)
            {
                myData.PutFloat("iT", localController.Throttle);
                localController.PrevThrottle = localController.Throttle;
            }
            if (localController.Pitch != localController.Pitch)
            {
                myData.PutFloat("iP", localController.Pitch);
                localController.PrevPitch = localController.Pitch;
            }
            if (localController.PrevRoll != localController.Roll)
            {
                myData.PutFloat("iR", localController.Roll);
                localController.PrevRoll = localController.Roll;
            }
            if (localController.PrevYaw != localController.Yaw)
            {
                myData.PutFloat("iY", localController.Yaw);
                localController.PrevYaw = localController.Yaw;
            }
            if (localController.PrevStrafe != localController.Strafe)
            {
                myData.PutFloat("iS", localController.Strafe);
                localController.PrevStrafe = localController.Strafe;
            }
            if (localController.PrevJump != localController.Jump)
            {
                myData.PutFloat("iJ", localController.Jump);
                localController.PrevJump = localController.Jump;
            }

            smartFox.Send(new ObjectMessageRequest(myData));
        }
    }
	
	public void sendSpawnData(Vector3 pos)
    {
		SFSObject myData = new SFSObject();
		SFSObject temp = new SFSObject();
		temp.PutFloat("x", pos.x);
		temp.PutFloat("y", pos.y);
		temp.PutFloat("z", pos.z);
		
		myData.PutSFSObject("spawnPos", temp);
		smartFox.Send(new ObjectMessageRequest(myData));
	}
	
	public void sendAttack(GameObject gO)
    {
		SFSObject myData = new SFSObject();
		
        myData.PutUtfString("Command", "CreateAttack");
		
		myData.PutUtfString("Id", gO.GetComponent<NetTag>().Id);
		
        myData.PutFloat("px", gO.transform.position.x);
        myData.PutFloat("py", gO.transform.position.y);
        myData.PutFloat("pz", gO.transform.position.z);

        myData.PutFloat("rx", gO.transform.rotation.eulerAngles.x);
        myData.PutFloat("ry", gO.transform.rotation.eulerAngles.y);
        myData.PutFloat("rz", gO.transform.rotation.eulerAngles.z);

        myData.PutFloat("vx", gO.rigidbody.velocity.x);
        myData.PutFloat("vy", gO.rigidbody.velocity.y);
        myData.PutFloat("vz", gO.rigidbody.velocity.z);
		
		smartFox.Send(new ObjectMessageRequest(myData));
		
		Debug.Log("Type of attack: " + gO.GetComponent<NetTag>().Id);
	}
	
	// This is a very expensive operation, it should only be called when a relevant object is created/destroyed
	public void updatePhysList() 
    {
        List<GameObject> PhysObjs = new List<GameObject>();
		//PhysObjects = GameObject.FindGameObjectsWithTag("PhysObj");
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("PhysObj"))
            PhysObjs.Add(g);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            PhysObjs.Add(p);
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Enemy"))
            PhysObjs.Add(e);
        PhysObjects = PhysObjs.ToArray();
	}
	
	//Should probably check to see if we are a spectator first...
    private PlayerInputController GetLocalController() 
    {
        Debug.Log(PhysObjects.ToString());
        foreach (GameObject g in PhysObjects)
        {
            if (g.GetComponent<PlayerInputController>())
            {
                return g.GetComponent<PlayerInputController>();
            }
        }
        return null;
    }
	
    private NetInputController GetRemoteController(string id)
    {
        foreach (GameObject g in PhysObjects)
        {
            if (g.GetComponent<NetInputController>() && g.GetComponent<NetInputController>().id == id)
            {
                return g.GetComponent<NetInputController>();
            }
        }
        return null;
    }

    private GameObject GetSyncedObject(string id)
    {
        foreach (GameObject g in PhysObjects)
        {
            if (g.GetComponent<NetTag>() && g.GetComponent<NetTag>().Id == id)
            {
                return g.GetComponent<NetTag>().transform.root.gameObject;
            }
        }
        return null;
    }

	void OnDrawGizmos()
    {
		Gizmos.DrawIcon(transform.position, "Manager");
        Gizmos.color = Color.white;
    }
}