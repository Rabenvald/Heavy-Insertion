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
	
	private GUIStyle blankStyle;
    private GameObject myself;
	private GameObject mainCamera;

    GameObject[] enemies;
    GameObject player;

    //Random random; //Camera Shaking

	void Start () 
    {
        enemies = MathTester.FindGameObjectsWithLayer(9);
        player = GameObject.FindWithTag("Player");
		
		float terrainWidth = Terrain.activeTerrain.terrainData.size.x;
		Debug.Log(Terrain.activeTerrain.terrainData.size);
		
		float diagonal = Mathf.Sqrt(2 * Mathf.Pow((terrainWidth/2), 2));
		Debug.Log(diagonal);

		gameObject.transform.position = new Vector3 (terrainWidth/2, (diagonal * Mathf.Tan((30F / 180) * Mathf.PI)) + Terrain.activeTerrain.terrainData.size.y, terrainWidth/2);
		gameObject.transform.forward = Vector3.down;
		Debug.Log(gameObject.transform.position);
		
		myself = GameObject.FindWithTag("MapCamera");
		Debug.Log(myself.camera.enabled);
		
		mainCamera = GameObject.FindWithTag("MainCamera");
		Debug.Log(mainCamera.camera.enabled);
	}

	void Update () 
    {
		if(myself.camera.enabled && !Manager.Instance.Spawned){
			RaycastHit hit;
			if(Input.GetMouseButtonDown(0))
			{
				if(Physics.Raycast(this.camera.ScreenPointToRay(Input.mousePosition), out hit))
				{
					Vector3 pos = new Vector3(hit.point.x, 2000, hit.point.z);
					Manager.Instance.spawnMe(pos);
					Manager.Instance.sendSpawnData(pos);
					Manager.Instance.Spawned = true;
					
					mainCamera.GetComponent<MainCameraScript>().setPlayer();
				}
				Debug.Log("SPAWN ME DAMN IT!");
			}
		}
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
		}
    }
    
}
