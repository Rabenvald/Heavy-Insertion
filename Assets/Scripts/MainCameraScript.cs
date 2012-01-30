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

    Rect menuButtonLoc;

    Rect multiplayerButtonLoc;
    Rect singlePlayerButtonLoc;
    Rect customizeButtonLoc;
    Rect settingsButtonLoc;
    Rect exitButtonLoc;

    bool bDisplayMenu;

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

        menuButtonLoc = (new Rect(Screen.width * 0.01f, Screen.height * 0.01f, 100, 25));

        ButtonLocations = new List<Rect>();

        buttonBuffer = 10;
        buttonWidth = 150;
        buttonHeight = 50;

        ButtonLocations.Add(multiplayerButtonLoc = new Rect(Screen.width * 0.25f - 100, Screen.height * 0.5f, buttonWidth, buttonHeight));
        ButtonLocations.Add(singlePlayerButtonLoc = new Rect(ButtonLocations[ButtonLocations.Count - 1].x, ButtonLocations[ButtonLocations.Count - 1].y + ButtonLocations[ButtonLocations.Count - 1].height + buttonBuffer, buttonWidth, buttonHeight));
        ButtonLocations.Add(customizeButtonLoc = new Rect(ButtonLocations[ButtonLocations.Count - 1].x, ButtonLocations[ButtonLocations.Count - 1].y + ButtonLocations[ButtonLocations.Count - 1].height + buttonBuffer, buttonWidth, buttonHeight));
        ButtonLocations.Add(settingsButtonLoc = new Rect(ButtonLocations[ButtonLocations.Count - 1].x, ButtonLocations[ButtonLocations.Count - 1].y + ButtonLocations[ButtonLocations.Count - 1].height + buttonBuffer, buttonWidth, buttonHeight));
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
        if (GUI.Button(menuButtonLoc, "Menu"))
        {
            print("Clicked 'Menu'");
            bDisplayMenu = GameUtils.Toggle(bDisplayMenu);
        }

        if (bDisplayMenu)
        {
            if (GUI.Button(multiplayerButtonLoc, "Multiplayer"))
            {
                print("Clicked 'Multiplayer'");
            }
            if (GUI.Button(singlePlayerButtonLoc, "Singleplayer / Debug"))
            {
                print("Clicked 'Singleplayer / Debug'");
                Application.LoadLevel(0);
            }
            if (GUI.Button(customizeButtonLoc, "Customize"))
            {
                print("Clicked 'Customize'");
            }
            if (GUI.Button(settingsButtonLoc, "Settings"))
            {
                print("Clicked 'Settings'");
            }
            if (GUI.Button(exitButtonLoc, "Exit"))
            {
                print("Clicked 'Exit'");
                Application.Quit();
            }
        }
    }

}
