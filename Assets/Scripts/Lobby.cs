using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Variables;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;


public class Lobby : MonoBehaviour {

	private SmartFox smartFox;
	private string zone = "BBB";
	private string serverName = "129.21.29.6";
	private int serverPort = 9933;
	public string username = "";
	private string loginErrorMessage = "";
	private bool isLoggedIn;
	private uint count;
	
	private string newMessage = "";
	private ArrayList messages = new ArrayList();
		
	public GUISkin gSkin;
	
	//keep track of room we're in
	private Room currentActiveRoom;
	public Room CurrentActiveRoom{ get {return currentActiveRoom;} }
				
	private Vector2 roomScrollPosition, userScrollPosition, chatScrollPosition;
	private int roomSelection = -1;	  //For clicking on list box 
	private string[] roomNameStrings; //Names of rooms
	private string[] roomFullStrings; //Names and descriptions
	private int screenW;
	private int screenH;
	
	//for lobby positioning
	private int chatW;
	private int chatH;
	private int sendW;
	private int sendH;
	private int userW;
	private int userH;
	private int roomW;
	private int roomH;
	private int vSpacing;
	private int hSpacing;
	private int offset;
	
	void Start()
	{
		Security.PrefetchSocketPolicy(serverName, serverPort); 
		bool debug = true;
		if (SmartFoxConnection.IsInitialized)
		{
			//If we've been here before, the connection has already been initialized. 
			//and we don't want to re-create this scene, therefore destroy the new one
			smartFox = SmartFoxConnection.Connection;
			Destroy(gameObject); 
		}
		else
		{
			//If this is the first time we've been here, keep the Lobby around
			//even when we load another scene, this will remain with all its data
			smartFox = new SmartFox(debug);
			DontDestroyOnLoad(gameObject);
		}
		
		count = 0;
		
		smartFox.AddLogListener(LogLevel.INFO, OnDebugMessage);
		screenW = Screen.width;
		screenH = Screen.height;
		vSpacing = 10;
		hSpacing = 10;
		offset = 10;
		
		
		userW = 180;
		userH = screenH - 20;
		chatW = screenW - userW - hSpacing*3;
		chatH = 200;
		sendW = chatW - (chatW/9);
		sendH = 24;
		roomW = screenW - userW - hSpacing*3;
		roomH = screenH - chatH - sendH - 48;
	}
	
	private void AddEventListeners() {
		
		smartFox.RemoveAllEventListeners();
		
		smartFox.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		smartFox.AddEventListener(SFSEvent.LOGIN, OnLogin);
		smartFox.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
		smartFox.AddEventListener(SFSEvent.LOGOUT, OnLogout);
		smartFox.AddEventListener(SFSEvent.ROOM_JOIN, OnJoinRoom);
		smartFox.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
		smartFox.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
		smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
		smartFox.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariablesUpdate); 
		smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
	}
	
	void FixedUpdate() {
		//this is necessary to have any smartfox action!
		smartFox.ProcessEvents();
	}
	
	private void UnregisterSFSSceneCallbacks() {
		smartFox.RemoveAllEventListeners();
	}
	
	public void OnConnection(BaseEvent evt) {
		bool success = (bool)evt.Params["success"];
		string error = (string)evt.Params["errorMessage"];
		
		//Debug.Log("On Connection callback got: " + success + " (error? : <" + error + ">)");

		if (success) {
			SmartFoxConnection.Connection = smartFox;

			//Debug.Log("Sending login request");
			smartFox.Send(new LoginRequest(username, "", zone));

		}
	}

	public void OnConnectionLost(BaseEvent evt) {
		//Debug.Log("OnConnectionLost");
		isLoggedIn = false;
		UnregisterSFSSceneCallbacks();
		currentActiveRoom = null;
		roomSelection = -1;	
		Application.LoadLevel("TestLobby");
	}

	// Various SFS callbacks
	public void OnLogin(BaseEvent evt) {
		try {
			if (evt.Params.ContainsKey("success") && !(bool)evt.Params["success"]) {
				loginErrorMessage = (string)evt.Params["errorMessage"];
				
			//set up user variables
			List<UserVariable> uData = new List<UserVariable> ();
			uData.Add (new SFSUserVariable ("Kills", 0));
			uData.Add (new SFSUserVariable ("Deaths", 0));
			uData.Add (new SFSUserVariable ("F'Yas", 0));
			smartFox.Send (new SetUserVariablesRequest (uData));
				
				//Debug.Log("Login error: "+loginErrorMessage);
			}
			else {
				//Debug.Log("Logged in successfully");
				smartFox.enableLagMonitor(true);
				PrepareLobby();	
			}
		}
		catch (Exception ex) {
			Debug.Log("Exception handling login request: "+ex.Message+" "+ex.StackTrace);
		}
	}

	public void OnLoginError(BaseEvent evt) {
		Debug.Log("Login error: "+(string)evt.Params["errorMessage"]);
	}
	
	void OnLogout(BaseEvent evt) {
		//Debug.Log("OnLogout");
		isLoggedIn = false;
		currentActiveRoom = null;
		smartFox.Disconnect();
	}
	
	public void OnDebugMessage(BaseEvent evt) {
		string message = (string)evt.Params["message"];
		Debug.Log("[SFS DEBUG] " + message);
	}
	
	
	public void OnRoomAdded(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];
		SetupRoomList();
		//Debug.Log("Room added: "+room.Name);
	}
	
	public void OnRoomCreationError(BaseEvent evt)
	{
		Debug.Log("Error creating room");
	}
	
	public void OnJoinRoom(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];
		User user = (User)evt.Params["user"];
		currentActiveRoom = room;
		if(room.Name=="The Lobby" )
			Application.LoadLevel("TestLobby");
		else if(room.Name == "Test Room"){
			Application.LoadLevel("Test1");
			smartFox.Send(new SpectatorToPlayerRequest());
		}
		else{
			Application.LoadLevel("M1");
			smartFox.Send(new SpectatorToPlayerRequest());
		}
		//Debug.Log(user.Name + " has entered the room: " + room.Name);
	}
	
	public void OnUserEnterRoom(BaseEvent evt) {
		User user = (User)evt.Params["user"];
		messages.Add( user.Name + " has entered the room: ");
	}

	private void OnUserLeaveRoom(BaseEvent evt) {
		User user = (User)evt.Params["user"];
		if(user.Name!=username){
			messages.Add( user.Name + " has left the room.");
		}	
	}

	public void OnUserCountChange(BaseEvent evt) {
		Room room = (Room)evt.Params["room"];
		if (room.IsGame ) {
			SetupRoomList();
		}
	}
	
	void OnPublicMessage(BaseEvent evt) {
		try {
			if (smartFox.LastJoinedRoom != null && smartFox.LastJoinedRoom.Name == "The Lobby")
			{
				string message = (string)evt.Params["message"];
				User sender = (User)evt.Params["sender"];
				messages.Add(sender.Name +": "+ message);
				
				chatScrollPosition.y = Mathf.Infinity;
				//Debug.Log("User " + sender.Name + " said: " + message); 
			}
		}
		catch (Exception ex) {
			Debug.Log("Exception handling public message: "+ex.Message+ex.StackTrace);
		}
	}
	
	public void OnUserVariablesUpdate(BaseEvent evt)
	{
		User user = (User) evt.Params["user"];
	}
	
	//PrepareLobby is called from OnLogin, the callback for login
	//so we can be assured that login was successful
	private void PrepareLobby() {
		//Debug.Log("Setting up the lobby");
		SetupRoomList();
		isLoggedIn = true;
	}
	
	
	void OnGUI() {
        if (Application.loadedLevelName == "TestLobby")
        {
            if (smartFox == null) return;
            screenW = Screen.width;
            //GUI.skin = gSkin;

            // Login
            if (!isLoggedIn)
            {
                DrawLoginGUI();
            }

            else if (currentActiveRoom != null)
            {

                // ****** Show full interface only in the Lobby ******* //
                if (currentActiveRoom.Name == "The Lobby")
                {
                    DrawLobbyGUI();
                }

                // ****** In other rooms, just show roomlist for switching ******* //
                DrawRoomsGUI();
            }

            if (GUI.Button(new Rect(10, 10, 100, 24), "Main Menu"))
                Application.LoadLevel("Main Menu");
        }
	}
	
	
	private void DrawLoginGUI(){
		//GUI.Label(new Rect(2, -2, 680, 70), "", "SFSLogo");
		GUI.Label(new Rect(100, 90, 100, 100), "Username: ");
		username = GUI.TextField(new Rect(200, 90, 200, 20), username, 25); 
	
		GUI.Label(new Rect(100, 180, 100, 100), "Server: ");
		serverName = GUI.TextField(new Rect(200, 180, 200, 20), serverName, 25);

		GUI.Label(new Rect(100, 210, 100, 100), "Port: ");
		serverPort = int.Parse(GUI.TextField(new Rect(200, 210, 200, 20), serverPort.ToString(), 4));

		GUI.Label(new Rect(100, 240, 100, 100), loginErrorMessage);

		if (GUI.Button(new Rect(300, 270, 100, 24), "Login")  || 
	    (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
		{
			AddEventListeners();
			smartFox.Connect(serverName, serverPort);
		}	
	}
			
	private void DrawLobbyGUI(){
		//GUI.Label(new Rect(2, -2, 680, 70), "", "SFSLogo");
		DrawUsersGUI();	
		DrawChatGUI();
		
		// Send message
		newMessage = GUI.TextField(new Rect(offset, screenH - sendH - vSpacing, sendW, sendH), newMessage, 50);
		if (GUI.Button(new Rect(sendW + hSpacing*2, screenH - sendH - vSpacing, 100, 24), "Send")  || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
		{
			smartFox.Send( new PublicMessageRequest(newMessage) );
			newMessage = "";
		}
		// Logout button
		if (GUI.Button (new Rect (120, offset, 100, 24), "Logout")) {
			smartFox.Send( new LogoutRequest() );
		}
		
		//make game button
		if (currentActiveRoom.Name == "The Lobby"){
			if (GUI.Button (new Rect (230, offset, 100, 24), "Make Game")) {
				RoomSettings settings = new RoomSettings(username + "'s Room");
				settings.Name = username + "'s Room";
				settings.MaxUsers = 32;
				settings.IsGame = true;
				smartFox.Send(new CreateRoomRequest(settings, false));
			}
		}
	}
		
		
	private void DrawUsersGUI(){
		GUI.Box (new Rect (screenW - userW - hSpacing, screenH - userH - vSpacing, userW, userH), "Users");
		GUILayout.BeginArea (new Rect (screenW - userW - offset, screenH - userH + hSpacing, userW, userH));
			userScrollPosition = GUILayout.BeginScrollView (userScrollPosition, GUILayout.Width (150), GUILayout.Height (150));
			GUILayout.BeginVertical ();
			
				List<User> userList = currentActiveRoom.UserList;
				foreach (User user in userList) {
					GUILayout.Label (user.Name); 
				}
			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}
	
	private void DrawRoomsGUI(){
		GUI.Box (new Rect (offset, screenH - (chatH + sendH + roomH + vSpacing), roomW, roomH), "Room List");
		GUILayout.BeginArea (new Rect (offset*2, screenH - (chatH + sendH + roomH - 24), roomW - 20, roomH - 10));
		if (smartFox.RoomList.Count >= 1) {		
			roomScrollPosition = GUILayout.BeginScrollView (roomScrollPosition, GUILayout.Width (roomW - 30), GUILayout.Height (roomH - 20));
			roomSelection = GUILayout.SelectionGrid (roomSelection, roomFullStrings, 1);
			
			if (roomSelection >= 0 && roomNameStrings[roomSelection] != currentActiveRoom.Name) {
				smartFox.Send(new JoinRoomRequest(roomNameStrings[roomSelection]));
			}
			GUILayout.EndScrollView ();
			
		} else {
			GUILayout.Label ("No rooms available to join");
		}
		GUILayout.EndArea ();
		
		GUILayout.BeginArea(new Rect (350, 10, 100, 100));
			// Game Room button
		GUILayout.EndArea ();
	}
	
	private void DrawChatGUI(){
		GUI.Box(new Rect(offset, screenH - (chatH + sendH), chatW, chatH - vSpacing - offset), "Chat");
		
		GUILayout.BeginArea (new Rect(offset*2, screenH - (chatH + sendH - offset), chatW - 10, chatH - 10));
			chatScrollPosition = GUILayout.BeginScrollView (chatScrollPosition, GUILayout.Width (450), GUILayout.Height (350));
				GUILayout.BeginVertical();
					foreach (string message in messages) {
						//this displays text from messages arraylist in the chat window
						GUILayout.Label(message);
				}
				GUILayout.EndVertical();
			GUILayout.EndScrollView ();
		GUILayout.EndArea();		
	}
	
	private void SetupRoomList () {
		List<string> rooms = new List<string> ();
		List<string> roomsFull = new List<string> ();
		
		List<Room> allRooms = smartFox.RoomManager.GetRoomList();
		
		foreach (Room room in allRooms) {
			rooms.Add(room.Name);
			roomsFull.Add(room.Name + " (" + room.UserCount + "/" + room.MaxUsers + ")");
		}
		
		roomNameStrings = rooms.ToArray();
		roomFullStrings = roomsFull.ToArray();
		
		//Debug.Log("Room list is set up");
		
		if (smartFox.LastJoinedRoom==null) {
			smartFox.Send(new JoinRoomRequest("The Lobby"));
		}
	}
}
