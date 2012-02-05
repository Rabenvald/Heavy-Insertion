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

public class Test1Manager : MonoBehaviour {
	private SmartFox smartFox; 
	private TestLobby lobby;
	private Room currentRoom;
	private bool running = false;
	
	public GameObject daCube;
	
	private string clientName;
	public string ClientName {
		get { return clientName;}
	}
	
	private static Test1Manager instance;
	public static Test1Manager Instance {
		get { return instance;}
	}
	
	private static bool isPhysX;
	public bool IsPhysX{
		get { return isPhysX; }	
	}
	
	void Awake() {
		instance = this;	
	}
	
	// Use this for initialization
	void Start () {
		bool debug = true;
		running = true;
		if (SmartFoxConnection.IsInitialized){
			smartFox = SmartFoxConnection.Connection;
		}
		else{
			smartFox = new SmartFox(debug);
		}
		
		currentRoom = smartFox.LastJoinedRoom;
		clientName = smartFox.MySelf.Name;
		
		if(currentRoom.UserCount == 1){
			isPhysX = true;	
		}
		else{
			isPhysX = false;	
		}
		
		smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
		smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
		smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessageReceived);
		smartFox.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariablesUpdate); 
		smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariablesUpdate);
	}
	
	// Update is called once per frame
	void Update () {
		if (!running) return;
		smartFox.ProcessEvents();
		sendData();
	}
	
	public void OnUserEnterRoom (BaseEvent evt){
		User user = (User)evt.Params["user"];
		Debug.Log ("user entered room " + user.Name);
	}
	
	public void OnUserLeaveRoom (BaseEvent evt){
		User user = (User)evt.Params["user"];
	}
	
	public void OnUserCountChange (BaseEvent evt){
		
	}
	
	public void OnConnectionLost (BaseEvent evt){
		smartFox.RemoveAllEventListeners();
	}
	
	public void OnObjectMessageReceived (BaseEvent evt){
		User sender = (User)evt.Params["sender"];
		ISFSObject obj = (SFSObject)evt.Params["message"];
		
		if(obj.GetBool("isPhysX")){
			daCube.transform.position = new Vector3(obj.GetFloat("x"), obj.GetFloat("y"), obj.GetFloat("z"));
			daCube.transform.rotation = Quaternion.Euler(new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")));
			daCube.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));
		}
		
		Debug.Log(obj.GetFloat("x") + " " + obj.GetFloat("y") + " " + obj.GetFloat("z"));
	}
	
	public void OnUserVariablesUpdate (BaseEvent evt){
		User user = (User)evt.Params["user"];
	}
	
	public void OnRoomVariablesUpdate (BaseEvent evt){
		Room room = (Room)evt.Params["room"];
	}
	
	private void sendData(){
		ISFSObject data = new SFSObject();
				
		data.PutFloat("x", daCube.transform.position.x);
		data.PutFloat("y", daCube.transform.position.y);
		data.PutFloat("z", daCube.transform.position.z);
		
		data.PutFloat("rx", daCube.transform.localEulerAngles.x);
		data.PutFloat("ry", daCube.transform.localEulerAngles.y);
		data.PutFloat("rz", daCube.transform.localEulerAngles.z);
		
		data.PutFloat("vx", daCube.rigidbody.velocity.x);
		data.PutFloat("vy", daCube.rigidbody.velocity.y);
		data.PutFloat("vz", daCube.rigidbody.velocity.z);
		
		data.PutBool("isPhysX", isPhysX);
		
		smartFox.Send(new ObjectMessageRequest(data));
	}
}