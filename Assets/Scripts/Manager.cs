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
	}
	
	public void OnUserEnterRoom (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];		
		//Debug.Log ("user entered room " + user.Name + " with id of " + user.Id);
	}

	public void spawnMe(Vector3 pos)
    {
		//GameObject cF = Instantiate(cameraFocus, pos, Quaternion.identity) as GameObject;
		GameObject tank = Instantiate(playerTankPrefab, pos, Quaternion.identity) as GameObject;
		//tank.GetComponent<Hovercraft>().SetFocus(cF);
		myTank = tank;
        myTank.GetComponent<NetTag>().Id = myId + "-00-" + "00"; 
		updatePhysList();
        localController = GetLocalController();
		spawned = true;
	}
	
	private void spawnTank(User user, Vector3 pos){
		GameObject tank = (GameObject)Instantiate(OtherPlayerTankPrefab, pos, Quaternion.identity);
        //InputController ic = tank.GetComponent<InputController>();
        //NetTag nt = tank.GetComponent<NetTag>();
        tank.GetComponent<InputController>().id = user.Id.ToString();
		tank.GetComponent<NetTag>().Id = user.Id.ToString() + "-00-" + "00";  //ID Schema: UserId + Type + InstanceNumber
	}
	
	public void OnUserLeaveRoom (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
	}
	
	public void OnUserCountChange (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
		
		//Debug.Log ("User count change based on " + user.Name + " with user Id of " + user.Id);
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
		
       // Debug.Log("Recieved message about" + obj.GetUtfString("PID") + ".  I am:" + myId);
		
		//making sure that the object has an Id
        if (obj.ContainsKey("Id"))
        {
            //Debug.Log("Incomming IDed info");
            //Debug.Log("Its about" + obj.GetUtfString("Id"));
			
			//split up the id string
            string[] tempId = obj.GetUtfString("Id").Split('-');
			
            GameObject thisGameObj = GetNetObject(obj.GetUtfString("Id"));
			
			if (thisGameObj == null)
			{
				CreateNewGameObject(obj, sender);
				thisGameObj = GetNetObject(obj.GetUtfString("Id"));
			}
           	//Debug.Log(thisGameObj);
			
			//if the object has any position, rotation, velocity, or angular velocity data
			if(tempId[1] != "00")
			{
				//Debug.Log("PX and other values are set");
				
	            thisGameObj.transform.position = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
	
	            thisGameObj.transform.rotation = Quaternion.Euler(new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")));
	
	            thisGameObj.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));
				
				thisGameObj.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));
			}
			
			//checking if self
			if (tempId[0] == myId && tempId[1] == "00")
	        {
	            //localController;
	            //Debug.Log("Recieved message about Me!"); 
	            if (obj.ContainsKey("PhysMaster"))
	            {
	                if (obj.GetBool("PhysMaster"))
	                {
	                    //Debug.Log("Its from the Phys Master!"); 
	                    //remoteController = GetRemoteController(obj.GetUtfString("PID"));
	
	                    //localController.Extrapolate();
	
	                    localController.Hull.Health = obj.GetInt("Health");
	
	                    localController.Hull.transform.position = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
	
	                    localController.Hull.transform.rotation = Quaternion.Euler(new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")));
	
	                    localController.Hull.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));
						
	                    localController.Hull.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));
						
						//Debug.Log("My position: " + localController.Hull.transform.position);
	
	                    localController.TimeSinceLastUpdate = Time.time;
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
						//set all the data for the other tanks
						
	                    remoteController = GetRemoteController(obj.GetUtfString("PID"));
	
	                    remoteController.Hull.Health = obj.GetInt("Health");
						
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
	                        updatePhysList();
	                }
	            }
				
				//regardless who from, update values based on inputs from user
	            if (obj.ContainsKey("inputs"))
	            {
	                remoteController = GetRemoteController(obj.GetUtfString("PID"));
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
			/*else if(tempId[1] != "00") //commented out temporarily
			{
				//Debug.Log("PX and other values are set");
				
	            thisGameObj.transform.position = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
	
	            thisGameObj.transform.rotation = Quaternion.Euler(new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")));
	
	            thisGameObj.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));
				
				//thisGameObj.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));
			}*/
			//spawning a tank - this might no longer be needed
			if(obj.ContainsKey("spawnPos"))
	        {
				Vector3 pos = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));
				spawnTank(sender, pos);
				
				Debug.Log("Spawn Location = " + obj.GetFloat("px") + ", " + obj.GetFloat("py") + ", " + obj.GetFloat("pz"));
			}
			//create attack
			if(obj.GetUtfString("Command") == "CreateAttack") //removed else because we are still sending those pieces of data
	        {
				string id = obj.GetUtfString("Id");
	            
				//parse id to determine type
				string[] temp = id.Split('-');
				//Debug.Log(temp[0] + " " + temp[1] + " " + temp[2]);
	
				int type = int.Parse(temp[1]);
	            GameObject NetObject = GetNetObject(temp[0] + "-00-00");
	            //Debug.Log(NetObject);
	
	            //Debug.Log("Type " + type);
	                //switch to determine what to create
	            GameObject proj;
	            switch (type)
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
	                    //create projectile
	                    //give it the values
	                    //give it the id
	                    break;
	                case 2: //missile
	                    proj = (GameObject)GameObject.Instantiate(ATMissile, new Vector3(obj.GetFloat("ppx"), obj.GetFloat("ppy"), obj.GetFloat("ppz")), Quaternion.Euler(new Vector3(obj.GetFloat("prx"), obj.GetFloat("pry"), obj.GetFloat("prz"))));
	                    proj.GetComponent<GuidedProjectileInputController>().TargetPosition = new Vector3(obj.GetFloat("tx"),obj.GetFloat("ty"),obj.GetFloat("tz"));
	                    proj.rigidbody.velocity = new Vector3(obj.GetFloat("pvx"), obj.GetFloat("pvy"), obj.GetFloat("pvz")); 
	                    Debug.Log("created missile for player " + temp[0]);
	                    //create missile
	                    //give it the values
	                    //give it the id
	                    break;
	                default:
	                    break;
	            }
        	}
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
	
	public void CreateNewGameObject(ISFSObject obj, User user)
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
		        //InputController ic = tank.GetComponent<InputController>();
		        //NetTag nt = tank.GetComponent<NetTag>();
		        newObject.GetComponent<InputController>().id = user.Id.ToString();
				newObject.GetComponent<NetTag>().Id = user.Id.ToString() + "-00-" + temp[2];
				updatePhysList();
				Debug.Log("Spawning New Tank with ID: " + newObject.GetComponent<NetTag>().Id);
				break;
			
			case 1: //projectile
				newObject = (GameObject)Instantiate(heatProjectile, new Vector3(obj.GetFloat("ppx"), obj.GetFloat("ppy"), obj.GetFloat("ppz")), Quaternion.Euler(new Vector3(obj.GetFloat("prx"), obj.GetFloat("pry"), obj.GetFloat("prz"))));
                newObject.rigidbody.velocity = new Vector3(obj.GetFloat("pvx"), obj.GetFloat("pvy"), obj.GetFloat("pvz"));

                newObject.transform.position += newObject.rigidbody.velocity.normalized * 7;

				newObject.GetComponent<NetTag>().Id = user.Id.ToString() + "-1-" + temp[2];
				primaryCount++;
				updatePhysList();
				Debug.Log("Spawning New Projectile with ID: " + newObject.GetComponent<NetTag>().Id);
				break;
			
			case 2: //missile
				newObject = (GameObject)Instantiate(ATMissile, new Vector3(obj.GetFloat("ppx"),obj.GetFloat("ppy"),obj.GetFloat("ppz")), Quaternion.Euler(new Vector3(obj.GetFloat("prx"),obj.GetFloat("pry"),obj.GetFloat("prz"))));
                newObject.GetComponent<GuidedProjectileInputController>().TargetPosition = new Vector3(obj.GetFloat("tx"), obj.GetFloat("ty"), obj.GetFloat("tz")); //ATMissile
                newObject.rigidbody.velocity = new Vector3(obj.GetFloat("pvx"), obj.GetFloat("pvy"), obj.GetFloat("pvz")); //ATMissile

                newObject.transform.position += newObject.rigidbody.velocity.normalized * 7;

				newObject.GetComponent<NetTag>().Id = user.Id.ToString() + "-2-" + temp[2];
				secondaryCount++;
				updatePhysList();
				Debug.Log("Spawning New Missile with ID: " + newObject.GetComponent<NetTag>().Id);
				break;
			
			default:
                Debug.Log("Type was: " + type);
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
        if (gO.GetComponent<Hovercraft>())
        {
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