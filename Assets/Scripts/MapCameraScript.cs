using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapCameraScript : MonoBehaviour
{
    /*public GameObject focus;
    public float shakeMagnitude;
    public Vector2 shakeInterval;*/
	
	public Texture PlayerTexture;
	public Texture EnemyTexture;
	public Texture TeammateTexture;

    public Texture ValidArea;
    public Texture InvalidArea;
	
	private GUIStyle blankStyle;
	private GameObject mainCamera;
	
	private bool drawCheck = false;
    private float terrainWidth;

    GameObject[] enemies;
    GameObject player;

    //Random random; //Camera Shaking

	void Start () 
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        player = GameObject.FindWithTag("Player");
		
		terrainWidth = Terrain.activeTerrain.terrainData.size.x;
		//Debug.Log(Terrain.activeTerrain.terrainData.size);
		
		float diagonal = Mathf.Sqrt(2 * Mathf.Pow((terrainWidth/2), 2));
		//Debug.Log(diagonal);

		gameObject.transform.position = new Vector3 (terrainWidth/2, (diagonal * Mathf.Tan((30F / 180) * Mathf.PI)) + Terrain.activeTerrain.terrainData.size.y, terrainWidth/2);
		gameObject.transform.forward = Vector3.down;
		//Debug.Log(gameObject.transform.position);
		
		gameObject.camera.orthographicSize = terrainWidth/2;
		
		mainCamera = GameObject.FindWithTag("MainCamera");
		//Debug.Log(mainCamera.camera.enabled);

        Screen.showCursor = false;
	}

	void Update () 
    {
        if (gameObject.camera.enabled && !Manager.Instance.Spawned)
        {
            RaycastHit hit;
            if (Physics.Raycast(this.camera.ScreenPointToRay(Input.mousePosition), out hit)
                && MathTester.AreVector3Close(new Vector3(hit.point.x, 0, hit.point.z), new Vector3(terrainWidth / 2, 0, terrainWidth / 2), terrainWidth / 4))
            {
                drawCheck = true;
                if (Input.GetMouseButtonDown(0))
                {
					Vector3 pos = new Vector3(hit.point.x, 2000, hit.point.z);
					mainCamera.transform.position = new Vector3(hit.point.x, 1700, hit.point.z);
					mainCamera.camera.enabled = true;
					gameObject.camera.enabled = false;
					Manager.Instance.spawnMe(pos);
					Manager.Instance.sendSpawnData(pos);
					mainCamera.GetComponent<MainCameraScript>().setPlayer();
                    Screen.showCursor = true;
					
                    //Debug.Log(Camera.main);
					//Debug.Log("MapCamera = " + gameObject.camera.enabled);
					//Debug.Log("MainCamera = " + mainCamera.camera.enabled);
				}
			}
			
			else
				drawCheck = false;
		}
		else
			drawCheck = false;
		
	}

    void FixedUpdate()
    {

    }

    void OnGUI()
    {
		if (gameObject.camera.enabled)
		{
	      	player = GameObject.FindWithTag("Player");
			Vector3 screenPos;
			if(Manager.Instance.Spawned){
				screenPos = gameObject.camera.WorldToScreenPoint(new Vector3 (player.transform.position.x, 100, player.transform.position.z));
				GUI.DrawTexture(new Rect(screenPos.x - 10, Screen.height - screenPos.y - 10, 20, 20), PlayerTexture);
			}
			
			for (int i = 0; i < enemies.Length; i++)
			{
				screenPos = gameObject.camera.WorldToScreenPoint(new Vector3 (enemies[i].transform.position.x, 100, enemies[i].transform.position.z));
				GUI.DrawTexture(new Rect(screenPos.x - 10, Screen.height - screenPos.y - 10, 20, 20), EnemyTexture);
			}
			
			if (drawCheck)
				GUI.DrawTexture(new Rect(Input.mousePosition.x - 16, Screen.height - Input.mousePosition.y - 16, 32, 32), ValidArea);
			else
				GUI.DrawTexture(new Rect(Input.mousePosition.x - 16, Screen.height - Input.mousePosition.y - 16, 32, 32), InvalidArea);
		}
    }
	
	public void setEnemies(){
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
	}
    
}
