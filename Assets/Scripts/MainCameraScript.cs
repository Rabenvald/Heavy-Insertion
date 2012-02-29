using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainCameraScript : MonoBehaviour
{
    /*public GameObject focus;
    public float shakeMagnitude;
    public Vector2 shakeInterval;*/

    List<Rect> ButtonLocations;

    /*public float timer;
    public float interval;*/

    int buttonBuffer;
    int buttonWidth;
    int buttonHeight;

    public Texture MenuTexture;
    public Texture MainMenuTexture;
    public Texture SettingsTexture;
    public Texture ExitTexture;

    public Texture BlankBackground;

    public Texture GrphicsTexture;
    public Texture BestTexture;
    public Texture AverageTexture;
    public Texture FastestTexture;
    public Texture VolumeTexture;

    public Texture HealthArea;
    public Texture HealthBar;

    public Texture Radar;

    public bool typing;
    private string newMessage = "";
    private ArrayList messages = new ArrayList();
    private Vector2 chatScrollPosition;

    Rect menuButtonLoc;

    Rect settingsButtonLoc;
    Rect mainmenuButtonLoc;
    Rect exitButtonLoc;

    bool bDisplayMenu;
    bool SettingsDisplayMenu;

    private GUIStyle blankStyle = new GUIStyle();

    private float volumeSliderValue = 1F;

    private GameObject player;

    private GameObject[] enemies;

    //Random random; //Camera Shaking

	void Start () 
    {
        //Camera Shaking
       /* random = new Random();

        //focus = Vector3.forward * 40;

        focus = new GameObject();
        focus.transform.parent = transform.parent;

        focus.transform.localEulerAngles = Vector3.zero;
        focus.transform.localPosition = Vector3.up;//transform.forward * 40;

        shakeInterval = new Vector2(0.001f, 0.1f);
        shakeMagnitude = 20;*/
        bDisplayMenu = false;
		SettingsDisplayMenu = false;

        ButtonLocations = new List<Rect>();
		
		ButtonLocations.Add(menuButtonLoc = (new Rect(Screen.width * 0.01f, Screen.height * 0.01f, 100, 25)));

        buttonBuffer = 10;
        buttonWidth = 100;
        buttonHeight = 25;

        ButtonLocations.Add(settingsButtonLoc = new Rect(ButtonLocations[ButtonLocations.Count - 1].x, ButtonLocations[ButtonLocations.Count - 1].y + ButtonLocations[ButtonLocations.Count - 1].height + buttonBuffer, buttonWidth, buttonHeight));
        ButtonLocations.Add(mainmenuButtonLoc = new Rect(ButtonLocations[ButtonLocations.Count - 1].x, ButtonLocations[ButtonLocations.Count - 1].y + ButtonLocations[ButtonLocations.Count - 1].height + buttonBuffer, buttonWidth, buttonHeight));
        ButtonLocations.Add(exitButtonLoc = new Rect(ButtonLocations[ButtonLocations.Count - 1].x, ButtonLocations[ButtonLocations.Count - 1].y + ButtonLocations[ButtonLocations.Count - 1].height + buttonBuffer, buttonWidth, buttonHeight));
		
		setPlayer();
		setEnemies();

        volumeSliderValue = AudioListener.volume;

        typing = false;
	}

	void Update () 
    {

        
        
	}

    void FixedUpdate()
    {
        //Camera Shaking
        /* if (timer >= interval) 
         {
             focus.transform.position = new Vector3(Random.RandomRange(-shakeMagnitude, shakeMagnitude), Random.RandomRange(-shakeMagnitude, shakeMagnitude), focus.transform.position.z);
             interval = Random.RandomRange(shakeInterval.x, shakeInterval.y);
             timer = 0;
         }
         else
             timer += Time.deltaTime;

         transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(focus.transform.position), Time.deltaTime * interval);*/
    }

    void OnGUI()
    {
        //top left button
        if (GUI.Button(menuButtonLoc, MenuTexture, blankStyle))
        {
            print("Clicked 'Menu'");
            bDisplayMenu = GameUtils.Toggle(bDisplayMenu);
            if (SettingsDisplayMenu)
                SettingsDisplayMenu = false;
        }

        //to display other menu buttons
        if (bDisplayMenu)
        {
            if (GUI.Button(settingsButtonLoc, SettingsTexture, blankStyle))
            {
                print("Clicked 'Settings'");
                SettingsDisplayMenu = !SettingsDisplayMenu;
            }

            if (GUI.Button(mainmenuButtonLoc, MainMenuTexture, blankStyle))
            {
                print("Clicked 'Singleplayer / Debug'");
                Application.LoadLevel("Main Menu");
            }

            if (GUI.Button(exitButtonLoc, ExitTexture, blankStyle))
            {
                print("Clicked 'Exit'");
                Application.Quit();
            }
        }

        //to display settings
        if (SettingsDisplayMenu)
        {
            GUI.DrawTexture(new Rect(75, 150, (Screen.width * .9f) - 3, Screen.height * .6f), BlankBackground);

            volumeSliderValue = GUI.HorizontalSlider(new Rect(500, 432, 200, 50), volumeSliderValue, 0.0F, 1F);

            if (GUI.changed)
            {
                //Changes volume levels globally
                AudioListener.volume = volumeSliderValue;
            }

            GUI.DrawTexture(new Rect(200, 200, 256, 64), GrphicsTexture);

            GUI.DrawTexture(new Rect(200, 400, 256, 64), VolumeTexture);

            if (GUI.Button(new Rect(800, 300, 256, 64), FastestTexture, blankStyle)) //Switched the position so best appears in the 4:3 web browser
            {
                print("Clicked 'Fastest'");
                QualitySettings.currentLevel = QualityLevel.Fastest;
            }

            if (GUI.Button(new Rect(500, 300, 256, 64), AverageTexture, blankStyle))
            {
                print("Clicked 'Average'");
                QualitySettings.currentLevel = QualityLevel.Good;
            }

            if (GUI.Button(new Rect(200, 300, 256, 64), BestTexture, blankStyle))
            {
                print("Clicked 'Best'");
                QualitySettings.currentLevel = QualityLevel.Fantastic;
            }

        }

        //if this camera is enabled and we are alive
        if (gameObject.camera.enabled)
        {
            if (Manager.Instance.Spawned)
            {
                //health
		        GUI.DrawTexture(new Rect(0, Screen.height - 128, 256, 128), HealthArea);
				Hovercraft playerCraft = player.GetComponent<Hovercraft>();
		        float health = playerCraft.Health;
		        if (health > 0)
	            	GUI.DrawTexture(new Rect(0, Screen.height - 70, (health / 310) * 256, 64), HealthBar);

                //Spedometer
                GUI.Box(new Rect(Screen.width - (Screen.width / 16) - 50, Screen.height * 0.7f - 25, 100, 25), (int)(playerCraft.CurrVelocity * 3.6f) + " km/h");

                //Enemy hud and radar
                GUI.DrawTexture(new Rect(Screen.width - (Screen.width / 8), Screen.height - (Screen.width / 8), Screen.width / 8, Screen.width / 8), Radar);

                RaycastHit hit;
                Vector3 rayDirection;
                Vector3 camRayDirection;
                Vector3 testPos = new Vector3(player.transform.position.x, player.transform.position.y + 5, player.transform.position.z);
                for (int i = 0; i < enemies.Length; i++)
                {
                    rayDirection = enemies[i].transform.position - testPos;
                    camRayDirection = enemies[i].transform.position - gameObject.camera.transform.position;

                    if ((Physics.Raycast(testPos, rayDirection, out hit)) && (MathTester.AreVector3Close(hit.point, enemies[i].rigidbody.transform.position, 8)))
                    {
                        float angle = Vector3.Angle(camRayDirection, gameObject.camera.transform.forward);
                        float raydarAngle = Vector2.Angle(new Vector2(gameObject.camera.transform.forward.x, gameObject.camera.transform.forward.z), new Vector2(camRayDirection.x, camRayDirection.z));
						if (gameObject.camera.transform.forward.x < camRayDirection.x)
							raydarAngle *= -1;
                        GUI.DrawTexture(new Rect(Screen.width - (Screen.width / 16) + (Mathf.Cos((raydarAngle + 90) * Mathf.Deg2Rad) * (Screen.width / 17)) - 4, Screen.height - (Screen.width / 16) + (Mathf.Sin((raydarAngle - 90) * Mathf.Deg2Rad) * (Screen.width / 17)) - 4, 8, 8), BlankBackground);
                        if (angle < gameObject.camera.fieldOfView)
                        {
                            Vector3 screenPos = gameObject.camera.WorldToScreenPoint(enemies[i].transform.position);
                            GUI.DrawTexture(new Rect(screenPos.x - 10, Screen.height - screenPos.y - 5, 10, 10), BlankBackground);
                        }
                    }
                }
            }
        }

        //chat
        GUI.Box(new Rect(10, Screen.height - 128 - 330, 300, 300), "Chat");

        GUILayout.BeginArea(new Rect(20, 110, 300, 300));
        chatScrollPosition = GUILayout.BeginScrollView(chatScrollPosition, GUILayout.Width(300), GUILayout.Height(300));
        GUILayout.BeginVertical();
        foreach (string message in messages)
        {
            //this displays text from messages arraylist in the chat window
            GUILayout.Label(message);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (typing)
        {
            GUI.SetNextControlName("MyTextField");
            newMessage = GUI.TextField(new Rect(10, Screen.height - 128 - 24, 300, 20), newMessage, 50);
            GUI.FocusControl("MyTextField");
            if (GUI.Button(new Rect(315, Screen.height - 128 - 26, 90, 24), "Send") || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
            {
                sendMsg();
            }
        }
    }

    public void messageReceived(string msg)
    {
        messages.Add(msg);
        chatScrollPosition.y = Mathf.Infinity;
    }

    public void sendMsg()
    {
        if (newMessage != "")
            Manager.Instance.SendMsg(newMessage);
        newMessage = "";
        typing = false;
    }


    void OnDrawGizmos()
    {
        /*RaycastHit hit;
        Vector3 rayDirection;
        Vector3 camRayDirection;
        Vector3 testPos = new Vector3(player.transform.position.x, player.transform.position.y + 10, player.transform.position.z);
        Debug.Log(enemies.Length);
        for (int i = 0; i < enemies.Length; i++)
        {
            rayDirection = enemies[i].transform.position - testPos;
            //camRayDirection = enemies[i].transform.position - gameObject.camera.transform.position;
            Gizmos.DrawRay(testPos, rayDirection);

        }*/
    }
	
	public void setPlayer(){
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	public void setEnemies(){
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
	}
}