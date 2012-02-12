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

    Rect menuButtonLoc;

    Rect settingsButtonLoc;
    Rect mainmenuButtonLoc;
    Rect exitButtonLoc;

    bool bDisplayMenu;
	bool SettingsDisplayMenu;
	
	private GUIStyle blankStyle = new GUIStyle();
	
	private float volumeSlider = 1F;

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
        if (GUI.Button(menuButtonLoc, MenuTexture, blankStyle))
        {
            print("Clicked 'Menu'");
            bDisplayMenu = GameUtils.Toggle(bDisplayMenu);
			if (SettingsDisplayMenu)
				SettingsDisplayMenu = false;
        }

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
		
		if (SettingsDisplayMenu)
		{						
			GUI.DrawTexture(new Rect(75, 150, (Screen.width * .9f) - 3, Screen.height * .6f), BlankBackground);
			
			volumeSlider = GUI.HorizontalSlider(new Rect(500, 432, 200, 50), volumeSlider, 0.0F, 1F);
			
			if (GUI.changed)
			{
				//Change volume levels on singleton
			}
			
			GUI.DrawTexture(new Rect(200, 200, 256, 64), GrphicsTexture);
			
			GUI.DrawTexture(new Rect(200, 400, 256, 64), VolumeTexture);
			
			if (GUI.Button(new Rect(200, 300, 256, 64), FastestTexture, blankStyle))
	        {
	            print("Clicked 'Fastest'");
	            QualitySettings.currentLevel = QualityLevel.Fastest;
	        }
			
			if (GUI.Button(new Rect(500, 300, 256, 64), AverageTexture, blankStyle))
	        {
	            print("Clicked 'Average'");
	            QualitySettings.currentLevel = QualityLevel.Good;
	        }
			
			if (GUI.Button(new Rect(800, 300, 256, 64), BestTexture, blankStyle))
	        {
	            print("Clicked 'Best'");
	            QualitySettings.currentLevel = QualityLevel.Fantastic;
	        }
			
		}
    }

}
