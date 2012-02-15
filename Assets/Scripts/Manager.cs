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

public class Manager : MonoBehaviour {
	private SmartFox smartFox; 
	private TestLobby lobby;
	private Room currentRoom;
	private bool running = false;
	private GameObject[] PhysicObjects;
	
	public String myId;
	
	public GameObject daTank;
	
	private string clientName;
	public string ClientName {
		get { return clientName;}
	}
	
	private static Manager instance;
	public static Manager Instance {
		get { return instance;}
	}
	
	private static bool isPhysX;
	public bool IsPhysX{
		get { return isPhysX; }	
	}
	
	void Awake() {
		instance = this;
	}
	
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
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!running) return;
		smartFox.ProcessEvents();
		updatePhysList();
		sendData();
	}
	
	public void OnUserEnterRoom (BaseEvent evt){
		User user = (User)evt.Params["user"];
		
		GameObject tank;
		InputController ic; 
		
		tank = Instantiate (daTank, new Vector3(2151.378f, 36.38875f, 2893.621f), Quaternion.identity) as GameObject;
		ic = tank.GetComponent<InputController>();
		ic.id = user.Id.ToString();
		
		Debug.Log ("user entered room " + user.Name + " with id of " + user.Id);
	}
	
	public void OnUserLeaveRoom (BaseEvent evt){
		User user = (User)evt.Params["user"];
	}
	
	public void OnUserCountChange (BaseEvent evt){
		User user = (User)evt.Params["user"];
		
		Debug.Log ("User count change based on " + user.Name + " with user Id of " + user.Id);
	}
	
	public void OnConnectionLost (BaseEvent evt){
		smartFox.RemoveAllEventListeners();
	}
	
	public void OnObjectMessageReceived (BaseEvent evt){
		User sender = (User)evt.Params["sender"];
		ISFSObject obj = (SFSObject)evt.Params["message"];
		
	}
	
	public void OnUserVariablesUpdate (BaseEvent evt){
		User user = (User)evt.Params["user"];
	}
	
	public void OnRoomVariablesUpdate (BaseEvent evt){
		Room room = (Room)evt.Params["room"];
	}
	
	public void onExtensionResponse(BaseEvent evt){
		
	}
	
	private void sendData(){
		
	}
	
	private void updatePhysList(){
		PhysicObjects = GameObject.FindGameObjectsWithTag("PhysObj");
		
		
	}
}