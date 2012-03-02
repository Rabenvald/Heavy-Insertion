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
	private SFSArray hierarchy;

    private PlayerInputController localController;

	//important prefabs
	public GameObject OtherPlayerTankPrefab;
	public GameObject playerTankPrefab;
	public GameObject cameraFocus;
	public GameObject heatProjectile;
	public GameObject ATMissile;
	
	//my info
	public String myId;
	private GameObject myTank;
	public GameObject MyTank
	{
		get { return myTank; }	
	}
	
	
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
	
    public float TimeBetweenUpdates = 2.2f;
    private float LastUpdateTime = 0;
    private uint ObjectSent = 0;
	public bool IsGameRoom;
	
	private int kills;
	
	private Queue<ISFSObject> instantiateQueue;
	
	private const string encyption = "*:@#$5147grbSDvASDAWE4124NHFJFCmd5bB%^D%brnxrx454335";

	void Awake() 
    {
		instance = this;
	}
	
	void Start () 
    {
		if (SmartFoxConnection.IsInitialized)
        {
			smartFox = SmartFoxConnection.Connection;
			IsGameRoom = true;
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
		
		kills = 0;
		
		spawned = false;

		updatePhysList();
		
		myId = smartFox.MySelf.Id.ToString();
		
		instantiateQueue = new Queue<ISFSObject>();
		
		smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
		smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
		smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessageReceived);
		smartFox.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariablesUpdate); 
		smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariablesUpdate);
		smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
        //smartFox.AddEventListener(SFSEvent.UDP_INIT, OnUDPInit);
		smartFox.AddEventListener(SFSEvent.EXTENSION_RESPONSE, onExtensionResponse);
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
		
		if(spawned)
			sendInputs();

        if (isPhysAuth && PhysObjects.Length > 0)
        {
            if (LastUpdateTime >= TimeBetweenUpdates)
            {
                if (ObjectSent < PhysObjects.Length)
				{
	                sendTelemetry(PhysObjects[ObjectSent]);
	                ObjectSent++;
	                LastUpdateTime = 0;
				}
				else
				{
					ObjectSent = 0;
				}
            }
            LastUpdateTime += Time.deltaTime;
        }
		
		Debug.Log(instantiateQueue.Count);
		if (instantiateQueue.Count > 0)
		{
			CreateNewGameObject(instantiateQueue.Dequeue());
		}
	}
	
	public void OnUserEnterRoom (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
		//Debug.Log ("user entered room " + user.Name + " with id of " + user.Id);
	}

	public void spawnMe(Vector3 pos)
    {
		if(myTank == null){
			//GameObject cF = Instantiate(cameraFocus, pos, Quaternion.identity) as GameObject;
			GameObject tank = Instantiate(playerTankPrefab, pos, Quaternion.identity) as GameObject;
			//tank.GetComponent<Hovercraft>().SetFocus(cF);
			myTank = tank;
	        myTank.GetComponent<NetTag>().Id = myId + "-00-" + "00"; 
			updatePhysList();
	        localController = GetLocalController();
			spawned = true;
		}
		else{
			//move to spawn location
			myTank.GetComponent<Hovercraft>().respawn(pos);
			spawned = true;
		}
	}
	
	private void spawnTank(GameObject obj, User user, Vector3 pos)
	{
		if(obj != null){
			obj.GetComponent<Hovercraft>().respawn(pos);
		}
		else{
			/*GameObject tank = (GameObject)Instantiate(OtherPlayerTankPrefab, pos, Quaternion.identity);
	        tank.GetComponent<InputController>().id = user.Id.ToString();
			tank.GetComponent<NetTag>().Id = user.Id.ToString() + "-00-" + "00"; //ID Schema: UserId + Type + InstanceNumber
			updatePhysList();
			GameObject.FindWithTag("MainCamera").GetComponent<MainCameraScript>().setEnemies();
			GameObject.FindWithTag("MapCamera").GetComponent<MapCameraScript>().setEnemies();
			Debug.Log("I made an enemy");*/
		}
	}
	
	public void OnUserLeaveRoom (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
		GameObject thisGameObj = GetNetObject(user.Id.ToString() + "-00-00");
		if(thisGameObj != null)
		{
			Destroy(thisGameObj);
			updatePhysList();
		}
	}
	
	public void OnUserCountChange (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
		if(currentRoom.UserCount == 1)
        {
			isPhysAuth = true;
		}
		//Debug.Log ("User count change based on " + user.Name + " with user Id of " + user.Id);
	}
	
	public void OnConnectionLost (BaseEvent evt)
    {
		smartFox.RemoveAllEventListeners();
	}

    //chat
    void OnPublicMessage(BaseEvent evt)
    {
        try
        {
            string message = (string)evt.Params["message"];
            User sender = (User)evt.Params["sender"];
			Debug.Log("testing");
			if (message.Contains(encyption))
			{
				Debug.Log(message.Length);
				Debug.Log(encyption.Length - 1);
				string newMessage = message.Substring(encyption.Length);
				Debug.Log(newMessage);
				GameObject.FindWithTag("MainCamera").GetComponent<MainCameraScript>().messageReceived(newMessage);
			}
			else
			{
            	GameObject.FindWithTag("MainCamera").GetComponent<MainCameraScript>().messageReceived(sender.Name + ": " + message);
			}
        }
        catch (Exception ex)
        {
            Debug.Log("Exception handling public message: " + ex.Message + ex.StackTrace);
        }
    }

    //chat
    public void SendMsg(string msg)
    {
        smartFox.Send(new PublicMessageRequest(msg));
    }
	
	public void BroadcastDeath(string cause, string whoDied)
	{
		if (isPhysAuth)
		{
			string[] temp = whoDied.Split('-');
			string victim = smartFox.UserManager.GetUserById(int.Parse(temp[0])).Name;
			if(cause=="cube")
			{
				SendMsg(encyption + victim + " got hit by shrapnel.");
			}
			else if (cause=="terrain")
			{
				SendMsg(encyption + victim +  " smashed into the terrain.");
			}
			else
			{
				string[] temp2 = cause.Split('-');
				string killer = smartFox.UserManager.GetUserById(int.Parse(temp2[0])).Name;
				if(victim==killer)
					SendMsg(encyption + victim + " blew themselves up.");
				else
				{
					SendMsg(encyption + victim + " got blown up by " + killer + ".");
					SFSObject myData = new SFSObject();
					myData.PutUtfString("killer", temp2[0]);
					smartFox.Send(new ObjectMessageRequest(myData));
				}
			}
		}
	}
	
    public void OnObjectMessageReceived(BaseEvent evt) //You do not recieve these messages from yourself
    {
		User sender = (User)evt.Params["sender"];
		ISFSObject obj = (SFSObject)evt.Params["message"];
        NetInputController remoteController;
		
       	// Debug.Log("Recieved message about" + obj.GetUtfString("PID") + ".  I am:" + myId);
		//Debug.Log("Obj contains spawnPos? " + obj.ContainsKey("spawnPos"));
		
		//making sure that the object has an Id
		if (obj.ContainsKey("killer"))
		{
			if (obj.GetUtfString("killer") == myId)
			{
				kills++;
				SendMsg(encyption + smartFox.UserManager.GetUserById(int.Parse(myId)).Name + " has " + kills + " kills.");
				if (kills >= 10)
				{
					//SFSObject myData = new SFSObject();
					//myData.PutUtfString("killer", temp2[0]);
					//smartFox.Send(new ObjectMessageRequest(myData));
				}
			}
		}
		else if (obj.ContainsKey("Id") && obj.GetUtfString("Id") != null)
        {
            //Debug.Log("Incomming IDed info");
            //Debug.Log("Its about" + obj.GetUtfString("Id"));
			
			//split up the id string
            string[] tempId = obj.GetUtfString("Id").Split('-');
			
            GameObject thisGameObj = GetNetObject(obj.GetUtfString("Id"));
			
			
			//Debug.Log(thisGameObj);
			
			if (thisGameObj == null)
			{
				/*Debug.Log(obj.GetUtfString("Id"));
				Debug.Log(thisGameObj);
				Debug.Log("Should be null and spawning an object of type: " + tempId[0]);*/
				//CreateNewGameObject(obj, sender);
				foreach (ISFSObject qobj in instantiateQueue)
				{
					if (qobj.GetUtfString("Id") ==	obj.GetUtfString("Id"))
					{
						return;
						Debug.Log("Shouldn't get here");
					}
				}
				instantiateQueue.Enqueue(obj);
				return;
				Debug.Log("Shouldn't get here either");
			}
			
           	//Debug.Log(thisGameObj);
			
			//checking if self
			if (tempId[0] == myId && tempId[1] == "00")
	        {
	            //localController;
	            //Debug.Log("Recieved message about Me!"); 
	            if (obj.ContainsKey("PhysMaster"))
	            {
	                if (obj.GetBool("PhysMaster"))
	                {
						//Debug.Log("Local Controller: ");
						/*Debug.Log("Position: " + obj.GetFloat("px") + ", " +  obj.GetFloat("py") + ", " + obj.GetFloat("pz") + "\n" +
						          "Rotation: " + obj.GetFloat("rx") + ", " +  obj.GetFloat("ry") + ", " + obj.GetFloat("rz") + "\n" +
						          "Velocity: " + obj.GetFloat("vx") + ", " +  obj.GetFloat("vy") + ", " + obj.GetFloat("vz") + "\n" +
						          "Ang Vel.: " + obj.GetFloat("ax") + ", " +  obj.GetFloat("ay") + ", " + obj.GetFloat("az"));*/
	                    //Debug.Log("Its from the Phys Master!"); 
	                    //remoteController = GetRemoteController(obj.GetUtfString("PID"));
	
	                    //localController.Extrapolate();
						
						int tempHealth = obj.GetInt("Health");
						Vector3 tempPos = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
						
						//if the server hasnt declared me dead, and i think I'm dead, and the server has my health above 0, then i am not dead. Respawn me
						if (localController.Hull.knownDead == false && localController.Hull.Dead && tempHealth > 0)
						{
							spawnMe(tempPos);
							GameObject.FindWithTag("MainCamera").transform.position = new Vector3(tempPos.x, tempPos.y, tempPos.z);
						}
	
	                    localController.Hull.Health = tempHealth;
						
	                    localController.Hull.transform.position = tempPos;
	
	                    localController.Hull.transform.rotation = Quaternion.Euler(new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")));
	
	                    localController.Hull.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));
						
	                    localController.Hull.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));
						
						//Debug.Log("My position: " + localController.Hull.transform.position);
	
	                    localController.TimeSinceLastUpdate = Time.time;
						
						if(localController.Hull.Health <= 0)
						{
							localController.Hull.knownDead = true;
							localController.Hull.kill();
	                        //updatePhysList();
						}
	                }
	            }
	        }
			//if not self, and object already exists also a tank
			else if (tempId[0] != myId && GetRemoteController(tempId[0]) != null && tempId[1] == "00")
	        {
				//if PhysMaster value is in the object
	            if (obj.ContainsKey("PhysMaster"))
	            {
					//if from physics master
	                if (obj.GetBool("PhysMaster"))
	                {
						//Debug.Log("Remote Controller: " + obj.GetUtfString("Id"));
						/*Debug.Log("Position: " + obj.GetFloat("px") + ", " +  obj.GetFloat("py") + ", " + obj.GetFloat("pz") + "\n" +
						          "Rotation: " + obj.GetFloat("rx") + ", " +  obj.GetFloat("ry") + ", " + obj.GetFloat("rz") + "\n" +
						          "Velocity: " + obj.GetFloat("vx") + ", " +  obj.GetFloat("vy") + ", " + obj.GetFloat("vz") + "\n" +
						          "Ang Vel.: " + obj.GetFloat("ax") + ", " +  obj.GetFloat("ay") + ", " + obj.GetFloat("az"));*/
						//set all the data for the other tanks
						
	                    remoteController = GetRemoteController(tempId[0]);
						
						int tempHealth = obj.GetInt("Health");
						Vector3 tempPos = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
						
						//if the server hasnt declared this dead, and i think its dead, and the server has its health above 0, then its not dead. Respawn me
						if (remoteController.Hull.knownDead == false && remoteController.Hull.Dead && tempHealth > 0)
							spawnTank(thisGameObj, sender, tempPos);
	
	                    remoteController.Hull.Health = tempHealth;
						
						//remoteController.LastPosition
	                    remoteController.Hull.transform.position = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
						
						//remoteController.LastRotation
	                    remoteController.Hull.transform.rotation = Quaternion.Euler(new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")));
	
	                    //remoteController.Extrapolate();
	
	                    //remoteController.Hull.transform.position = remoteController.PositionExtrapolation;
	
	                    //remoteController.Hull.transform.rotation = Quaternion.Euler(remoteController.RotationExtrapolation);
	
	                    remoteController.Hull.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));
	                    
	                    remoteController.Hull.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));
	
	                    remoteController.TimeSinceLastUpdate = Time.time;
						
						//Debug.Log("User " + obj.GetUtfString("PID") + "'s position: " + remoteController.Hull.transform.position);
						
	                    if (remoteController.Hull.Health <= 0)
						{
							remoteController.Hull.knownDead = true;
							remoteController.Hull.kill();
	                        //updatePhysList();
						}
	                }
	            }
				
				//Debug.Log(tempId[0] + " " + tempId[1] + " " + tempId[2]);
				
				//regardless who from, update values based on inputs from user
	            if (obj.ContainsKey("inputs"))
	            {
	                remoteController = GetRemoteController(tempId[0]);
	                if (obj.ContainsKey("iT"))
					{
	                    remoteController.Throttle = obj.GetFloat("iT");
						//Debug.Log("iT = " + obj.GetFloat("iT"));
					}
	                if (obj.ContainsKey("iP"))
					{
	                    remoteController.Pitch = obj.GetFloat("iP");
						//Debug.Log("iP = " + obj.GetFloat("iP"));
					}
	                if (obj.ContainsKey("iR"))
					{
	                    remoteController.Roll = obj.GetFloat("iR");
						//Debug.Log("iR = " + obj.GetFloat("iR"));
					}
	                if (obj.ContainsKey("iY"))
					{
	                    remoteController.Yaw = obj.GetFloat("iY");
						//Debug.Log("iY = " + obj.GetFloat("iY"));
					}
	                if (obj.ContainsKey("iS"))
					{
	                    remoteController.Strafe = obj.GetFloat("iS");
						//Debug.Log("iS = " + obj.GetFloat("iS"));
					}
	                if (obj.ContainsKey("iJ"))
					{
	                    remoteController.Jump = obj.GetFloat("iJ");
						//Debug.Log("iJ = " + obj.GetFloat("iJ"));
					}
	            }
	        }//if the object is not a tank, update its data
			else if(tempId[1] != "00")
			{
				//Debug.Log("PX and other values are set");
				//Debug.Log("inside checking if not a tank: " + tempId[0] + " " + tempId[1] + " " + tempId[2]);
				//Debug.Log("thisGameObj's id: " + thisGameObj.GetComponent<NetTag>().Id);
				
	            thisGameObj.transform.position = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
	
	            thisGameObj.transform.rotation = Quaternion.Euler(new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")));
	
	            thisGameObj.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));
				
				thisGameObj.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));
			}//spawning a tank - this might no longer be needed
			if(obj.ContainsKey("spawnPos"))
		    {
				Vector3 pos = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
				Debug.Log(pos);
				spawnTank(thisGameObj, sender, pos);
			}
			//create attack
			/*if(obj.GetUtfString("Command") == "CreateAttack") //removed else because we are still sending those pieces of data
	        {
				string id = obj.GetUtfString("Id");
	            
				//parse id to determine type
				string[] temp = id.Split('-');
				//Debug.Log(temp[0] + " " + temp[1] + " " + temp[2]);
	
				int type = int.Parse(temp[1]);
	            GameObject NetObject = GetNetObject(temp[0] + "-00-00");
	            //Debug.Log(NetObject);
	
	            //Debug.Log("Type " + type);
	            GameObject proj;
                switch (type) //switch to determine what to create
	            {
	                case 1: //projectile
	                    NetInputController thisRemoteController = NetObject.GetComponent<NetInputController>();
	                    //Debug.Log("RC" + thisRemoteController);
	                    //Debug.Log("RC Muzz: " + thisRemoteController.Turret.Muzzle);
	                    GameObject Muzzle = thisRemoteController.Turret.Muzzle;
	                    //Debug.Log("RC id" + thisRemoteController.id);
	                    if (thisRemoteController != null)
	                    {
	                        Debug.Log("created projectile for player " + temp[0]);
	                        proj = (GameObject)GameObject.Instantiate(heatProjectile, new Vector3(obj.GetFloat("ppx"), obj.GetFloat("ppy"), obj.GetFloat("ppz")), Quaternion.Euler(new Vector3(obj.GetFloat("prx"), obj.GetFloat("pry"), obj.GetFloat("prz"))));
	
	                        proj.rigidbody.velocity = new Vector3(obj.GetFloat("pvx"), obj.GetFloat("pvy"), obj.GetFloat("pvz")); 
	                        // Recoil
	                        thisRemoteController.Hull.rigidbody.AddForceAtPosition(-proj.rigidbody.velocity * proj.rigidbody.mass, Muzzle.transform.position, ForceMode.Impulse);
	                    }
	                    else
	                    {
	                        Debug.Log("Null Object");
	                    }
	                    break;
	                case 2: //missile
	                    proj = (GameObject)GameObject.Instantiate(ATMissile, new Vector3(obj.GetFloat("ppx"), obj.GetFloat("ppy"), obj.GetFloat("ppz")), Quaternion.Euler(new Vector3(obj.GetFloat("prx"), obj.GetFloat("pry"), obj.GetFloat("prz"))));
	                    proj.GetComponent<GuidedProjectileInputController>().TargetPosition = new Vector3(obj.GetFloat("tx"),obj.GetFloat("ty"),obj.GetFloat("tz"));
	                    proj.rigidbody.velocity = new Vector3(obj.GetFloat("pvx"), obj.GetFloat("pvy"), obj.GetFloat("pvz")); 
	                    Debug.Log("created missile for player " + temp[0]);
	                    break;
	                default:
	                    break;
	            }
        	}*/
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
	
	public void CreateNewGameObject(ISFSObject obj)
	{
		string id = obj.GetUtfString("Id");
            
		//parse id to determine type
		string[] temp = id.Split('-');
		//Debug.Log(temp[0] + " " + temp[1] + " " + temp[2]);

		int type = int.Parse(temp[1]);
		
		GameObject newObject;

        //Debug.Log("Make new stuff");
		
		switch (type)
        {
            case 00: //tank
				Vector3 pos = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
				newObject = (GameObject)Instantiate(OtherPlayerTankPrefab, pos, Quaternion.identity);
		        newObject.GetComponent<InputController>().id = temp[0];
				newObject.GetComponent<NetTag>().Id = temp[0] + "-00-" + "00";
				GameObject.FindWithTag("MainCamera").GetComponent<MainCameraScript>().setEnemies();
				GameObject.FindWithTag("MapCamera").GetComponent<MapCameraScript>().setEnemies();
				updatePhysList();
				break;
			
			case 1: //projectile
				newObject = (GameObject)Instantiate(heatProjectile, new Vector3(obj.GetFloat("ppx"), obj.GetFloat("ppy"), obj.GetFloat("ppz")), Quaternion.Euler(new Vector3(obj.GetFloat("prx"), obj.GetFloat("pry"), obj.GetFloat("prz"))));
                newObject.rigidbody.velocity = new Vector3(obj.GetFloat("pvx"), obj.GetFloat("pvy"), obj.GetFloat("pvz"));
                newObject.transform.position += newObject.rigidbody.velocity.normalized * 7;
				newObject.GetComponent<NetTag>().Id = temp[0] + "-1-" + temp[2];
				//primaryCount++;
				updatePhysList();
				//Debug.Log("Spawning New Projectile with ID: " + newObject.GetComponent<NetTag>().Id);
				break;
			
			case 2: //missile
				newObject = (GameObject)Instantiate(ATMissile, new Vector3(obj.GetFloat("ppx"),obj.GetFloat("ppy"),obj.GetFloat("ppz")), Quaternion.Euler(new Vector3(obj.GetFloat("prx"),obj.GetFloat("pry"),obj.GetFloat("prz"))));
                newObject.GetComponent<GuidedProjectileInputController>().TargetPosition = new Vector3(obj.GetFloat("tx"), obj.GetFloat("ty"), obj.GetFloat("tz")); //ATMissile
                newObject.rigidbody.velocity = new Vector3(obj.GetFloat("pvx"), obj.GetFloat("pvy"), obj.GetFloat("pvz")); //ATMissile
                newObject.transform.position += newObject.rigidbody.velocity.normalized * 7;
				newObject.GetComponent<NetTag>().Id = temp[0] + "-2-" + temp[2];
				//secondaryCount++;
				updatePhysList();
				//Debug.Log("Spawning New Missile with ID: " + newObject.GetComponent<NetTag>().Id);
				break;
			
			default:
                //Debug.Log("Type was: " + type);
				break;
		}
	}
	
	//receives messages from server extension which handles the physics list
	public void onExtensionResponse(BaseEvent evt)
    {
		SFSObject obj;
		
		//if there was proper detection of who has the lowest ping
		if(evt.Params["data"] != null)
		{
			obj = evt.Params["data"] as SFSObject;
			hierarchy = obj.GetSFSArray("hierarchy") as SFSArray;
		}
		//if there was no proper detection, at least turn an array to use that is likely not optimal
		else if(evt.Params["randomData"] != null)
		{
			obj = evt.Params["randomData"] as SFSObject;
			hierarchy = obj.GetSFSArray("hierarchy") as SFSArray;
		}
	}
	
	private void sendTelemetry(GameObject gO)
    {
        SFSObject myData = new SFSObject();
		Hovercraft goHovercraft = gO.GetComponent<Hovercraft>();
        if (goHovercraft)
        {
			if (goHovercraft.Dead)
			{
				if (goHovercraft.knownDead)
					return;
				goHovercraft.knownDead = true;
			}
            string[] temp = gO.GetComponent<NetTag>().Id.Split('-');
            //Debug.Log(temp[0]gO.GetComponent<NetTag>().Id

            //myData.PutUtfString("PID", temp[0]);
            myData.PutInt("Health", gO.GetComponent<Hovercraft>().Health);
        }
        myData.PutUtfString("Id", gO.GetComponent<NetTag>().Id);
        //Debug.Log("Sending data about: " + gO.GetComponent<NetTag>().Id);

        myData.PutFloat("px", gO.transform.position.x);
        myData.PutFloat("py", gO.transform.position.y);
        myData.PutFloat("pz", gO.transform.position.z);

        myData.PutFloat("rx", gO.transform.rotation.eulerAngles.x);
        myData.PutFloat("ry", gO.transform.rotation.eulerAngles.y);
        myData.PutFloat("rz", gO.transform.rotation.eulerAngles.z);

        myData.PutFloat("vx", gO.rigidbody.velocity.x);
        myData.PutFloat("vy", gO.rigidbody.velocity.y);
        myData.PutFloat("vz", gO.rigidbody.velocity.z);
		
		myData.PutFloat("ax", gO.rigidbody.angularVelocity.x);
		myData.PutFloat("ay", gO.rigidbody.angularVelocity.y);
		myData.PutFloat("az", gO.rigidbody.angularVelocity.z);
		
        myData.PutBool("PhysMaster", isPhysAuth);

        smartFox.Send(new ObjectMessageRequest(myData));
		
		//Debug.Log("Sent Data for " + gO.GetComponent<NetTag>().Id);
		/*Debug.Log("Position: " + gO.transform.position.x + ", " +  gO.transform.position.y + ", " + gO.transform.position.z + "\n" +
		          "Rotation: " + gO.transform.rotation.eulerAngles.x + ", " +  gO.transform.rotation.eulerAngles.y + ", " + gO.transform.rotation.eulerAngles.z + "\n" +
		          "Velocity: " + gO.rigidbody.velocity.x + ", " +  gO.rigidbody.velocity.y + ", " + gO.rigidbody.velocity.z + "\n" +
		          "Ang Vel.: " + gO.rigidbody.angularVelocity.x + ", " +  gO.rigidbody.angularVelocity.y + ", " + gO.rigidbody.angularVelocity.z);*/
	}

    private void sendInputs()
    {
        if ((localController.PrevThrottle != localController.Throttle) || (localController.Pitch != localController.Pitch) || 
            (localController.PrevRoll != localController.Roll) || (localController.PrevYaw != localController.Yaw) || 
            (localController.PrevStrafe != localController.Strafe) || (localController.PrevJump != localController.Jump))
        {
            SFSObject myData = new SFSObject();

            //myData.PutUtfString("PID", myId);
            myData.PutUtfString("Id", myTank.GetComponent<NetTag>().Id);

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
		myData.PutBool("spawnPos", true);
		myData.PutUtfString("Id", myId + "-00-00");
		myData.PutFloat("px", pos.x);
		myData.PutFloat("py", pos.y);
		myData.PutFloat("pz", pos.z);
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
		
		//Debug.Log("Type of attack: " + gO.GetComponent<NetTag>().Id);
	}

    public void sendAttack(GameObject gO, Vector3 pos, Vector3 rot, Vector3 vel, Vector3 Targ)
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

        myData.PutFloat("ppx", pos.x);
        myData.PutFloat("ppy", pos.y);
        myData.PutFloat("ppz", pos.z);

        myData.PutFloat("prx", pos.x);
        myData.PutFloat("pry", pos.y);
        myData.PutFloat("prz", pos.z);

        myData.PutFloat("pvx", vel.x);
        myData.PutFloat("pvy", vel.y);
        myData.PutFloat("pvz", vel.z);

        myData.PutFloat("tx", Targ.x);
        myData.PutFloat("ty", Targ.y);
        myData.PutFloat("tz", Targ.z);

        myData.PutBool("PhysMaster", isPhysAuth);

        smartFox.Send(new ObjectMessageRequest(myData));

        //Debug.Log("Type of attack: " + gO.GetComponent<NetTag>().Id);
    }

    public void sendAttack(GameObject gO, Vector3 pos, Vector3 rot, Vector3 vel)
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

        myData.PutFloat("ppx", pos.x);
        myData.PutFloat("ppy", pos.y);
        myData.PutFloat("ppz", pos.z);

        myData.PutFloat("prx", pos.x);
        myData.PutFloat("pry", pos.y);
        myData.PutFloat("prz", pos.z);

        myData.PutFloat("pvx", vel.x);
        myData.PutFloat("pvy", vel.y);
        myData.PutFloat("pvz", vel.z);

        myData.PutBool("PhysMaster", isPhysAuth);

        smartFox.Send(new ObjectMessageRequest(myData));

        //Debug.Log("Type of attack: " + gO.GetComponent<NetTag>().Id);
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
        //Debug.Log(PhysObjects.ToString());
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

    private GameObject GetNetObject(string id)
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