using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

/*
	Tom's Terrain Tools for Unity 3D
	version 2.1 - February 2011
	
	(C)2010 by Tom Vogt <tom@lemuria.org>
	
	http://lemuria.org/projects/wiki/TerrainTools2

*/

public class TerrainTools : EditorWindow {

	public Terrain MyTerrain;

	private Object MassMap;
	private bool DefaultsDone = false;
	private Texture2D[] SplatTex = new Texture2D[4];
	private GameObject[] TreeObj = new GameObject[3];
	private Texture2D GrassTex;
	
	public Texture2D SplatA;
	public Texture2D SplatB;

	public Texture2D treemap;
	public bool ResetTrees = true;
	public float TreeDensity = 0.5f;
	public float TreeThreshold = 0.2f;
	public float TreeSize = 1f;
	public float SizeVariation = 0.2f;

	public Texture2D grassmap;
	public Texture2D bushmap;
	public float grassmod = 1.0f;
	public float grassclumping = 0.5f;
	public float bushmod = 0.25f;
	
	public Texture2D OverlayMap;
	public float OverlayThreshold = 0.1f;
	public Texture2D OverlayTexture;
	public int TileSize = 15;
	public bool ClearTrees = false;
	public float ClearRadius = 1.0f;
	public bool ClearGrass = true;
	public float ChangeTerrain = 0.0f;

	
	// internal variables
	Vector2 scrollPosition = new Vector2(0,0);
	bool ToggleSplat = true;
	bool ToggleTrees = true;
	bool ToggleGrass = true;
	bool ToggleOverlay;
	bool ToggleMass;

	[MenuItem("Window/Terrain Tools")]
	static void Init() {
		TerrainTools window = (TerrainTools)EditorWindow.GetWindow(typeof(TerrainTools));
		window.Show();
	}

	void OnInspectorUpdate() {
		Repaint();
	}

	void OnGUI() {
		EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
				GUILayout.Label("Terrain Tools");
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
				GUILayout.Label("version 2.1");
				GUILayout.Space(20);
				if (GUILayout.Button("Read Manual")) {
					Help.BrowseURL("http://lemuria.org/projects/wiki/TerrainTools2");
				}
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		EditorGUILayout.Separator();

		ToggleMass = EditorGUILayout.Foldout(ToggleMass, "AutoMagic"); 
		if (ToggleMass) {
			if (!DefaultsDone) AutoMagicDefaults();
			GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Hint Object");
				GUILayout.FlexibleSpace();
				MassMap = EditorGUILayout.ObjectField(MassMap, typeof(Object));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Textures");
			GUILayout.FlexibleSpace();
			SplatTex[0] = (Texture2D)EditorGUILayout.ObjectField(SplatTex[0], typeof(Texture2D));
			SplatTex[1] = (Texture2D)EditorGUILayout.ObjectField(SplatTex[1], typeof(Texture2D));
			SplatTex[2] = (Texture2D)EditorGUILayout.ObjectField(SplatTex[2], typeof(Texture2D));
			SplatTex[3] = (Texture2D)EditorGUILayout.ObjectField(SplatTex[3], typeof(Texture2D));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Trees");
			GUILayout.FlexibleSpace();
			TreeObj[0] = (GameObject)EditorGUILayout.ObjectField(TreeObj[0], typeof(GameObject));
			TreeObj[1] = (GameObject)EditorGUILayout.ObjectField(TreeObj[1], typeof(GameObject));
			TreeObj[2] = (GameObject)EditorGUILayout.ObjectField(TreeObj[2], typeof(GameObject));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Grass");
			GUILayout.FlexibleSpace();
			GrassTex = (Texture2D)EditorGUILayout.ObjectField(GrassTex, typeof(Texture2D));
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Run AutoMagic")) {
					AutoMagic();
				}
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		EditorGUILayout.Separator();
		MyTerrain = (Terrain)EditorGUILayout.ObjectField("", MyTerrain, typeof(Terrain));
		EditorGUILayout.Separator();

		ToggleSplat = EditorGUILayout.Foldout(ToggleSplat, "Texturing"); 
		if (ToggleSplat) {
			GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("First Splatmap");
				GUILayout.FlexibleSpace();
				SplatA = (Texture2D)EditorGUILayout.ObjectField("", SplatA, typeof(Texture2D));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Second Splatmap (optional)");
				GUILayout.FlexibleSpace();
				SplatB = (Texture2D)EditorGUILayout.ObjectField("", SplatB, typeof(Texture2D));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Apply Splatmap(s)")) {
					if (CheckSplatmap()) ApplySplatmap();
				}
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		EditorGUILayout.Separator();

		ToggleTrees = EditorGUILayout.Foldout(ToggleTrees, "Tree Distribution"); 
		if (ToggleTrees) {			
			GUILayout.BeginHorizontal();
				GUILayout.Label("Tree map");
				GUILayout.FlexibleSpace();
				treemap = (Texture2D)EditorGUILayout.ObjectField("", treemap, typeof(Texture2D));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Clear Existing Trees?");
				GUILayout.FlexibleSpace();
				ResetTrees = EditorGUILayout.Toggle("remove trees", ResetTrees);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Tree Density");
				GUILayout.FlexibleSpace();
				TreeDensity = EditorGUILayout.Slider(TreeDensity, 0.1f, 1f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Threshold");
				GUILayout.FlexibleSpace();
				TreeThreshold = EditorGUILayout.Slider(TreeThreshold, 0.01f, 0.99f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Tree Size");
				GUILayout.FlexibleSpace();
				TreeSize = EditorGUILayout.Slider(TreeSize, 0.2f, 5f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Size Variation");
				GUILayout.FlexibleSpace();
				SizeVariation = EditorGUILayout.Slider(SizeVariation, 0f, 1f);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Generate Trees")) {
					if (CheckTreemap()) ApplyTreemap();
				}
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

		}

		EditorGUILayout.Separator();

		ToggleGrass = EditorGUILayout.Foldout(ToggleGrass, "Grass and Details"); 
		if (ToggleGrass) {
			GUILayout.BeginHorizontal();
				GUILayout.Label("Grass map");
				GUILayout.FlexibleSpace();
				grassmap = (Texture2D)EditorGUILayout.ObjectField("", grassmap, typeof(Texture2D));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Bush/Detail map");
				GUILayout.FlexibleSpace();
				bushmap = (Texture2D)EditorGUILayout.ObjectField("", bushmap, typeof(Texture2D));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Grass Density");
				GUILayout.FlexibleSpace();
				grassmod = EditorGUILayout.Slider(grassmod, 0.1f, 3f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Grass Clumping");
				GUILayout.FlexibleSpace();
				grassclumping = EditorGUILayout.Slider(grassclumping, 0f, 1f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Bush/Detail Density");
				GUILayout.FlexibleSpace();
				bushmod = EditorGUILayout.Slider(bushmod, 0.1f, 2f);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Generate Grass and Details")) {
					if (CheckGrassmap()) ApplyGrassmap();
				}
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		EditorGUILayout.Separator();

		ToggleOverlay = EditorGUILayout.Foldout(ToggleOverlay, "Overlays (roads, rivers, etc)"); 
		if (ToggleOverlay) {
			GUILayout.BeginHorizontal();
				GUILayout.Label("Overlay map");
				GUILayout.FlexibleSpace();
				OverlayMap = (Texture2D)EditorGUILayout.ObjectField("", OverlayMap, typeof(Texture2D));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Threshold");
				GUILayout.FlexibleSpace();
				OverlayThreshold = EditorGUILayout.Slider(OverlayThreshold, 0.1f, 0.9f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Overlay texture");
				GUILayout.FlexibleSpace();
				OverlayTexture = (Texture2D)EditorGUILayout.ObjectField("", OverlayTexture, typeof(Texture2D));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Tile size");
				GUILayout.FlexibleSpace();
				TileSize = EditorGUILayout.IntSlider(TileSize, 3, 127);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Clear trees");
				GUILayout.FlexibleSpace();
				ClearTrees = EditorGUILayout.Toggle("", ClearTrees);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Clear tree radius");
				GUILayout.FlexibleSpace();
				ClearRadius = EditorGUILayout.Slider(ClearRadius, 0.5f, 10.0f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Clear grass/detail");
				GUILayout.FlexibleSpace();
				ClearGrass = EditorGUILayout.Toggle("", ClearGrass);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
				GUILayout.Label("Raise/lower terrain");
				GUILayout.FlexibleSpace();
				ChangeTerrain = EditorGUILayout.Slider(ChangeTerrain, -50f, 50f);
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Generate Overlay")) {
					if (CheckOverlaymap()) ApplyOverlaymap();
				}
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

			
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
	}
	
	
	void FixFormat(Texture2D texture) {
		string path = AssetDatabase.GetAssetPath(texture);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		if (texture.format != TextureFormat.RGB24 || !textureImporter.isReadable) {
			Debug.Log(path+" needs fixing");
			textureImporter.mipmapEnabled = false;
			textureImporter.isReadable = true;
			textureImporter.textureFormat = TextureImporterFormat.RGB24;
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			Debug.Log("fixed texture format for "+path);
		}
	}
	
	public string Reverse(string text) {
	   if (text == null) return null;

	   // this was posted by petebob as well 
	   char[] array = text.ToCharArray();
	   System.Array.Reverse(array);
      return new string(array);
	}
	public string FindFile(string basename) {
		string[] extensions = {"tif", "tiff", "png", "jpg", "jpeg"};
		foreach (string ext in extensions) {
			string filename = basename + "." + ext;
			if (File.Exists(filename)) {
				return filename;
			}
		}
		return "";
	}
	
	void AutoMagic() {
		if (MassMap == null) return;

		string path = AssetDatabase.GetAssetPath(MassMap);
		string pathrev = Reverse(Path.GetFileNameWithoutExtension(path));
		string[] parts = pathrev.Split(new char[] { '-' }, 2);
		string basename = Reverse(parts[1]);
		string dirname = Path.GetDirectoryName(path);

		Debug.Log("applying TTT AutoMagic to "+basename);
		
		path = Path.Combine(dirname, basename);
		string heightmap_name = path+"-heightmap.raw";
		string splatmap_name = FindFile(path+"-splatmap");
		string treemap_name = FindFile(path+"-treemap");
		string grassmap_name = FindFile(path+"-grassmap");
		SplatA = AssetDatabase.LoadAssetAtPath(splatmap_name, typeof(Texture2D)) as Texture2D;
		CheckSplatmap();
		treemap = AssetDatabase.LoadAssetAtPath(treemap_name, typeof(Texture2D)) as Texture2D;
		CheckTreemap();
		grassmap = AssetDatabase.LoadAssetAtPath(grassmap_name, typeof(Texture2D)) as Texture2D;
		CheckGrassmap();
		CreateTerrain(path, SplatA.width);
		ReadRaw(heightmap_name, SplatA.width+1);
		MyTerrain.terrainData.splatPrototypes = SetupTextures();
		ApplySplatmap();
		MyTerrain.terrainData.treePrototypes = SetupTrees();
		MyTerrain.terrainData.RefreshPrototypes();
		ApplyTreemap();
		MyTerrain.terrainData.detailPrototypes = SetupGrass();
		MyTerrain.terrainData.RefreshPrototypes();
		ApplyGrassmap();
	}
	
	void AutoMagicDefaults() {
		Debug.Log("setting up AutoMagic defaults");
		DefaultsDone = true;
		SplatTex[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Terrain Assets/Textures/Rock (Moss).jpg", typeof(Texture2D));
		SplatTex[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Terrain Assets/Textures/Grass (Hill).jpg", typeof(Texture2D));
		SplatTex[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Terrain Assets/Textures/Cliff (Layered Rock).jpg", typeof(Texture2D));
		SplatTex[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Terrain Assets/Textures/Grass (Muddy).jpg", typeof(Texture2D));
		GrassTex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Terrain Assets/Grass/Grass.psd", typeof(Texture2D));
		TreeObj[0] = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Terrain Assets/Trees Ambient-Occlusion/Alder.fbx", typeof(GameObject));
		TreeObj[1] = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Terrain Assets/Trees Ambient-Occlusion/Sycamore.fbx", typeof(GameObject));
		TreeObj[2] = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Terrain Assets/Trees Ambient-Occlusion/Mimosa.fbx", typeof(GameObject));
	}
	
	SplatPrototype[] SetupTextures() {
		Vector2 tilesize = new Vector2(40f, 40f);
		SplatPrototype[] Splat = new SplatPrototype[4];
		Splat[0] = new SplatPrototype();
		Splat[0].texture = SplatTex[0];
		Splat[0].tileSize = tilesize;
		Splat[1] = new SplatPrototype();
		Splat[1].texture = SplatTex[1];
		Splat[1].tileSize = tilesize;
		Splat[2] = new SplatPrototype();
		Splat[2].texture = SplatTex[2];
		Splat[2].tileSize = tilesize;
		Splat[3] = new SplatPrototype();
		Splat[3].texture = SplatTex[3];
		Splat[3].tileSize = tilesize;
		
		return Splat;
	}
	
	TreePrototype[] SetupTrees() {
		TreePrototype[] Trees = new TreePrototype[3];
		Trees[0] = new TreePrototype();
		Trees[0].prefab = TreeObj[0];
		Trees[1] = new TreePrototype();
		Trees[1].prefab = TreeObj[1];
		Trees[2] = new TreePrototype();
		Trees[2].prefab = TreeObj[2];
		return Trees;
	}
	
	DetailPrototype[] SetupGrass() {
		DetailPrototype[] Grass = new DetailPrototype[3];
		Grass[0] = new DetailPrototype();
		Grass[0].prototypeTexture = GrassTex;
		Grass[1] = new DetailPrototype();
		Grass[1].prototypeTexture = GrassTex;
		Grass[2] = new DetailPrototype();
		Grass[2].prototypeTexture = GrassTex;
		return Grass;
	}
	
	bool CheckSplatmap() {
		if (SplatA==null) return false;
		FixFormat(SplatA);

		if (SplatA.height != SplatA.width) {
			EditorUtility.DisplayDialog("Wrong size", "First splatmap must be square (width and height must be the same)", "Cancel"); 
			return false;
		}
		if (Mathf.ClosestPowerOfTwo(SplatA.width) != SplatA.width) {
			EditorUtility.DisplayDialog("Wrong size", "Splatmap width and height must be a power of two", "Cancel"); 
			return false;	
		}

		if (SplatB!=null) {
			FixFormat(SplatB);
			if (SplatB.height != SplatB.width) {
				EditorUtility.DisplayDialog("Wrong size", "Second splatmap must be square (width and height must be the same)", "Cancel"); 
				return false;
			}
			if (Mathf.ClosestPowerOfTwo(SplatB.width) != SplatB.width) {
				EditorUtility.DisplayDialog("Wrong size", "Second splatmap width and height must be a power of two", "Cancel"); 
				return false;	
			}
		}

		return true;
	}
	
	void ApplySplatmap() {
		if (!MyTerrain) MyTerrain = Terrain.activeTerrain;
		TerrainData terrain = MyTerrain.terrainData;
		if (terrain.alphamapLayers<4 || (SplatB!=null && terrain.alphamapLayers<8)) {
			EditorUtility.DisplayDialog("Missing Textures", "Please set up at least 4 (one splatmap) or 8 (two splatmaps) textures in the terrain painter dialog.", "Cancel"); 
			return;	
		}
		Undo.RegisterUndo(terrain, "Apply splatmap(s)");
		int w = SplatA.width;
		bool TwoMaps = false;
		if (SplatB==null) {
			SplatB = new Texture2D(w, w, TextureFormat.ARGB32, false);
		} else {
			TwoMaps=true;
		}
		terrain.alphamapResolution = w;

		float[,,] splatmapData = terrain.GetAlphamaps(0, 0, w, w);
		Color[] splatmapColors = SplatA.GetPixels();
		Color[] splatmapColors_b = SplatB.GetPixels();
		if (!TwoMaps) {
			DestroyImmediate(SplatB);
			SplatB = null;
		}
		
		for (int y = 0; y < w; y++) {
			if (y%10 == 0) EditorUtility.DisplayProgressBar("Applying splatmap", "calculating...", Mathf.InverseLerp(0.0f, (float)w, (float)y));
			for (int x = 0; x < w; x++) {
				float sum;
				Color col = splatmapColors[((w-1)-x)*w + y];
				Color col_b = splatmapColors_b[((w-1)-x)*w + y];
				if (!TwoMaps) {
					sum = col.r+col.g+col.b;				
				} else {
					sum = col.r+col.g+col.b+col_b.r+col_b.g+col_b.b;
				}
				if (sum>1.0f) {
					// no final channel, and scale down
					splatmapData[x,y,0] = col.r / sum;
					splatmapData[x,y,1] = col.g / sum;
					splatmapData[x,y,2] = col.b / sum;
					if (!TwoMaps) {
						splatmapData[x,y,3] = 0.0f;
					} else {
						splatmapData[x,y,3] = col_b.r / sum;
						splatmapData[x,y,4] = col_b.g / sum;
						splatmapData[x,y,5] = col_b.b / sum;
						splatmapData[x,y,6] = 0.0f;
					}
				} else {
					// derive final channel from black
					splatmapData[x,y,0] = col.r;
					splatmapData[x,y,1] = col.g;
					splatmapData[x,y,2] = col.b;
					if (!TwoMaps) {
						splatmapData[x,y,3] = 1.0f - sum;
					} else {
						splatmapData[x,y,3] = col_b.r;
						splatmapData[x,y,4] = col_b.g;
						splatmapData[x,y,5] = col_b.b;
						splatmapData[x,y,6] = 1.0f - sum;
					}
				}
			}
		}
		EditorUtility.ClearProgressBar();

		terrain.SetAlphamaps(0, 0, splatmapData);
		Debug.Log("Splatmaps applied.");		
	}
	
	
	bool CheckTreemap() {
		if (treemap == null) return false;
		FixFormat(treemap);
		
		if (treemap.height != treemap.width) {
			EditorUtility.DisplayDialog("Wrong size", "treemap width and height must be the same", "Cancel"); 
			return false;
		}
		if (Mathf.ClosestPowerOfTwo(treemap.width) != treemap.width) {
			EditorUtility.DisplayDialog("Wrong size", "treemap width and height must be a power of two", "Cancel"); 
			return false;	
		}
		
		if (TreeDensity<0.1f || TreeDensity>1.0f) {
			EditorUtility.DisplayDialog("Invalid Value", "Tree Density must be between 0.1 and 1.0", "Cancel"); 
			return false;	
		}
		if (TreeThreshold<0.0f || TreeThreshold>1.0f) {
			EditorUtility.DisplayDialog("Invalid Value", "Threshold must be between 0.0 and 1.0", "Cancel"); 
			return false; 	
		}

		return true;
	}
	
	void ApplyTreemap() {
		// set up my data
		if (!MyTerrain) MyTerrain = Terrain.activeTerrain;
		TerrainData data = MyTerrain.terrainData;
		Undo.RegisterUndo(data, "Apply tree map");

		int w = treemap.width;

		Color[] mapColors = treemap.GetPixels();

		int index = -1;
		int trees = 0;
		int Step = Mathf.RoundToInt(1.0f/TreeDensity);
		float PositionVariation = (float)(Step*0.5f/(float)w);

		if (ResetTrees) {
			data.treeInstances = new TreeInstance[0];
		}

		for (int y = 1; y < w-1; y+=Step) {
			if (y%10 == 0) EditorUtility.DisplayProgressBar("Placing trees", "placed "+trees+" trees so far", Mathf.InverseLerp(0.0f, (float)w, (float)y));
			for (int x = 1; x < w-1; x+=Step) {
				// place the chosen tree, if the colours are right
				index = -1;
				Color col = mapColors[y*w + x];
				if (col.r>TreeThreshold+Random.Range(0.0f, 1.0f)) {
					index = 0;
				} else if (col.g>TreeThreshold+Random.Range(0.0f, 1.0f)) {
					index = 1;
				} else if (col.b>TreeThreshold+Random.Range(0.0f, 1.0f)) {
					index = 2;
				}

				if (index>=0) {
					// place a tree
					trees++;

					TreeInstance instance = new TreeInstance();

					// random placement modifier for a more natural look
					float xpos = (float)x / (float)w; float ypos=(float)y / (float)w;
					xpos = Mathf.Clamp01(xpos+Random.Range(-PositionVariation, PositionVariation));
					ypos = Mathf.Clamp01(1-ypos+Random.Range(-PositionVariation, PositionVariation));
					instance.position = new Vector3(xpos, 0f, ypos);

					instance.color = Color.white;
					instance.lightmapColor = Color.white;
					instance.prototypeIndex = index;

					instance.widthScale = TreeSize * (1f + Random.Range(-SizeVariation, SizeVariation));
					instance.heightScale = TreeSize * (1f + Random.Range(-SizeVariation, SizeVariation));

					MyTerrain.AddTreeInstance(instance);

				}
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("placed "+trees+" trees");
	}
	
	
	bool CheckGrassmap() {
		if (grassmap != null) { 
			FixFormat(grassmap);

			int w = grassmap.width;
			if (grassmap.height != w) {
				EditorUtility.DisplayDialog("Wrong size", "grassmap width and height must be the same", "Cancel"); 
				return false;
			}
			if (Mathf.ClosestPowerOfTwo(w) != w) {
				EditorUtility.DisplayDialog("Wrong size", "grassmap width and height must be a power of two", "Cancel"); 
				return false;	
			}
		}

		if (bushmap != null) { 
			FixFormat(bushmap);

			int w = bushmap.width;
			if (bushmap.height != w) {
				EditorUtility.DisplayDialog("Wrong size", "bushmap width and height must be the same", "Cancel"); 
				return false;
			}
			if (Mathf.ClosestPowerOfTwo(w) != w) {
				EditorUtility.DisplayDialog("Wrong size", "bushmap width and height must be a power of two", "Cancel"); 
				return false;	
			}
		}

		return true;
	}
	
	void ApplyGrassmap() {
		if (!MyTerrain) MyTerrain = Terrain.activeTerrain;
		TerrainData terrain = MyTerrain.terrainData;
		Undo.RegisterUndo(terrain, "Apply grass and bush maps");
		
		if (grassmap!=null) {
			SetDetailmap(grassmap, grassmod, 0, grassclumping, "Grass map");
			Debug.Log("Grass map applied.");
		}
		if (bushmap!=null) {
			SetDetailmap(bushmap, bushmod, 3, 0.0f, "Bush map");
			Debug.Log("Bush map applied.");
		}
		EditorUtility.ClearProgressBar();
	}
	
	void SetDetailmap(Texture2D map, float mod, int firstlayer, float clumping, string MapName) {
		if (!MyTerrain) MyTerrain = Terrain.activeTerrain;
		TerrainData data = MyTerrain.terrainData;

		Color[] detailColors = map.GetPixels();
		int w = map.width;
		int res = data.detailResolution;

		int[,] detail_a = new int[res,res];
		int[,] detail_b = new int[res,res];
		int[,] detail_c = new int[res,res];

		float scale = (float)w/(float)res;

		for (int y = 0; y < res; y++) {
			EditorUtility.DisplayProgressBar("Applying "+MapName, "Calculating...", Mathf.InverseLerp(0.0f, (float)res, (float)y));
			for (int x = 0; x < res; x++) {
				// place detail, depending on colours in map image
				int sx = Mathf.FloorToInt((float)(x)*scale);
				int sy = Mathf.FloorToInt((float)(y)*scale);
				Color col = detailColors[((w-1)-sx)*w + sy];
				
				detail_a[x,y] = DetailValue(col.r*mod);
				detail_b[x,y] = DetailValue(col.g*mod);
				detail_c[x,y] = DetailValue(col.b*mod);
			}
		}
		
		if (clumping>0.01f) {
			detail_a = MakeClumps(detail_a, clumping);
			detail_b = MakeClumps(detail_b, clumping);
			detail_c = MakeClumps(detail_c, clumping);
		}

		data.SetDetailLayer(0, 0, firstlayer+0, detail_a);
		data.SetDetailLayer(0, 0, firstlayer+1, detail_b);
		data.SetDetailLayer(0, 0, firstlayer+2, detail_c);
	}
	
	int DetailValue(float col) {
		// linearly translates a 0.0-1.0 number to a 0-16 integer
		int c = Mathf.FloorToInt(col*16);
		float r = col*16 - Mathf.FloorToInt(col*16);
		
		if (r>Random.Range(0.0f, 1.0f)) c++;
		return Mathf.Clamp(c, 0, 16);
	}
	
	int[,] MakeClumps(int[,] map, float clumping) {
		int res = map.GetLength(0);
		int [,] clumpmap = new int[res,res];

		// init - there's probably a better way to do this in C# that I just don't know
		for (int y = 0; y < res; y++) {
			for (int x = 0; x < res; x++) {
				clumpmap[x,y]=0;
			}
		}

		for (int y = 0; y < res; y++) {
			for (int x = 0; x < res; x++) {
				clumpmap[x,y]+=map[x,y];
				if (map[x,y]>0) {
					// there's grass here, we might want to raise the grass value of our neighbours
					for (int a=-1;a<=1;a++) for (int b=-1;b<=1;b++) {
						if (x+a<0||x+a>=res||y+b<0||y+b>=res) continue;
						if (a!=0||b!=0&&Random.Range(0.0f, 1.0f)<clumping) clumpmap[x+a,y+b]++;
					}
				}
			}
		}

		// values above 9 yield strange results
		for (int y = 0; y < res; y++) {
			for (int x = 0; x < res; x++) {
				if (clumpmap[x,y]>9) clumpmap[x,y]=9;
			}
		}
		
		return clumpmap;
	}
	
	
	bool CheckOverlaymap() {
		if (OverlayMap==null) return false;
		FixFormat(OverlayMap);

		if (OverlayMap.height != OverlayMap.width) {
			EditorUtility.DisplayDialog("Wrong size", "OverlayMap width and height must be the same", "Cancel"); 
			return false;
		}
		if (Mathf.ClosestPowerOfTwo(OverlayMap.width) != OverlayMap.width) {
			EditorUtility.DisplayDialog("Wrong size", "OverlayMap width and height must be a power of two", "Cancel"); 
			return false;	
		}

		if (OverlayMap.width!=MyTerrain.terrainData.alphamapResolution) {
			EditorUtility.DisplayDialog("Wrong size", "OverlayMap must have same size as existing splatmap ("+MyTerrain.terrainData.alphamapResolution+")", "Cancel"); 
			return false;	
		}
		
		return true;
	}
	
	void ApplyOverlaymap() {
		if (!MyTerrain) MyTerrain = Terrain.activeTerrain;
		TerrainData terrain = MyTerrain.terrainData;
		Undo.RegisterUndo(terrain, "Apply overlay map");
		int w = OverlayMap.width;
		Color[] OverlayMapColors = OverlayMap.GetPixels();
		int layer = MyTerrain.terrainData.alphamapLayers;

		int detailRes = terrain.detailWidth;
		int[] detailLayers = terrain.GetSupportedLayers(0, 0, detailRes, detailRes);
		int LayerCount = detailLayers.Length;

		AddTexture();
		float[,,] splatmapData = terrain.GetAlphamaps(0, 0, w, w);

		float terrainScale = (float)w / ((float)terrain.heightmapWidth-1);
		float terrainHeight = terrain.size.y;
		int terrainSample = Mathf.CeilToInt(terrainScale);

		ArrayList NewTrees = new ArrayList(terrain.treeInstances);
		if (ClearTrees) {
/*			
			int count = 0;
			foreach (TreeInstance tree in terrain.treeInstances) {
				count++;
				EditorUtility.DisplayProgressBar("Overlay map", "gathering tree data", (float)count/(float)terrain.treeInstances.Length);
				NewTrees.Add(tree);
			}
*/
		}
		int TreesRemoved = 0;
		for (int y = 0; y < w; y++) {
			if (y%10 == 0) {
				if (ClearTrees) {
					EditorUtility.DisplayProgressBar("Overlay map", "updating terrain and trees ("+TreesRemoved+" trees removed)", Mathf.InverseLerp(0.0f, (float)w, (float)y));
				} else {
					EditorUtility.DisplayProgressBar("Overlay map", "updating terrain", Mathf.InverseLerp(0.0f, (float)w, (float)y));
				}
			}
			for (int x = 0; x < w; x++) {
				float value = OverlayMapColors[((w-1)-x)*w + y].grayscale;
				if (value>OverlayThreshold) {
					splatmapData[x,y,layer] = value;
					// fix the other layers
					for (int l = 0; l<layer; l++) {
						splatmapData[x,y,l] *= (1.0f-value);
					}

					if (ChangeTerrain>0.01f || ChangeTerrain<-0.01f) {
						if (value>OverlayThreshold) {
							float change = ChangeTerrain * value / terrainHeight;
							int sx = Mathf.FloorToInt((float)y*terrainScale);
							int sy = Mathf.FloorToInt((float)x*terrainScale);
							float [,] data = terrain.GetHeights(sx, sy, terrainSample, terrainSample);
							for (int a=0;a<terrainSample;a++) for (int b=0;b<terrainSample;b++) {
								data[a,b]=Mathf.Max(0.0f, data[a,b]+change);
							}
							terrain.SetHeights(sx, sy, data);
						}
					}
					
					if (ClearTrees) {
						for (int i = NewTrees.Count -1; i >= 0; i--) {
							TreeInstance MyTree = (TreeInstance)NewTrees[i];
							float distance = Vector2.Distance(new Vector2(MyTree.position.z*w, MyTree.position.x*w), new Vector2((float)x, (float)y));
							if (distance < ClearRadius) {
								NewTrees.RemoveAt(i);
								TreesRemoved++;
							}
						}
					}

				} else {
					splatmapData[x,y,layer] = 0.0f;
				}
			}
		}
		if (ClearTrees) {
			terrain.treeInstances = (TreeInstance[])NewTrees.ToArray(typeof(TreeInstance));
		}


		terrain.SetAlphamaps(0, 0, splatmapData);
		Debug.Log("Overlay map applied.");

		if (ClearGrass) {
			float scale = (float)w / (float)detailRes;
			for (int l = 0; l<LayerCount; l++) {
				EditorUtility.DisplayProgressBar("Overlay map", "clearing away grass", Mathf.InverseLerp(0.0f, (float)l, (float)LayerCount));
				int[,] grass = terrain.GetDetailLayer(0, 0, detailRes, detailRes, l);
				for (int y = 0; y < detailRes; y++) {
					for (int x = 0; x < detailRes; x++) {
						int sx = Mathf.FloorToInt((float)(x)*scale);
						int sy = Mathf.FloorToInt((float)(y)*scale);
						float value = OverlayMapColors[((w-1)-sx)*w + sy].grayscale;
						if (value > OverlayThreshold && grass[x,y]>0) {
							if (value > 0.5f) grass[x,y]=0; else grass[x,y]=1;
						}
					}
				}
				terrain.SetDetailLayer(0, 0, l, grass);
			}
		}

		EditorUtility.ClearProgressBar();
	}

	void AddTexture() {
		if (!MyTerrain) MyTerrain = Terrain.activeTerrain;
		SplatPrototype[] oldPrototypes = MyTerrain.terrainData.splatPrototypes;
		SplatPrototype[] newPrototypes = new SplatPrototype[oldPrototypes.Length + 1];
		for (int x=0;x<oldPrototypes.Length;x++) {
			newPrototypes[x] = oldPrototypes[x];
		}
		newPrototypes[oldPrototypes.Length] = new SplatPrototype();
		newPrototypes[oldPrototypes.Length].texture = OverlayTexture;
		Vector2 vector = new Vector2(TileSize, TileSize);
		newPrototypes[oldPrototypes.Length].tileSize = vector;
		MyTerrain.terrainData.splatPrototypes = newPrototypes;
		EditorUtility.SetDirty(MyTerrain);
	}



	// these are copied from UnityEditor.dll
	private void CreateTerrain(string name, int size) {
		TerrainData theAsset = new TerrainData();
//		theAsset.heightmapResolution = 0x201;
		theAsset.heightmapResolution = size+1;
		theAsset.size = new Vector3(2000f, 1000f, 2000f);
//		AssetUtility.CreateAsset(theAsset, name);
		AssetDatabase.CreateAsset(theAsset, name+".asset");
//		theAsset.heightmapResolution = 0x200;
		theAsset.heightmapResolution = size;
		theAsset.baseMapResolution = 0x400;
//		theAsset.SetDetailResolution(0x400, theAsset.detailResolutionPerPatch);
		theAsset.SetDetailResolution(0x400, 16); // recommended value from documentation
//		AssetUtility.SaveAsset(theAsset);
		Selection.activeObject = Terrain.CreateTerrainGameObject(theAsset);
		AssetDatabase.SaveAssets();
		MyTerrain = Terrain.activeTerrain; // FIXME: not really what we want, but it works
	}

	
	private void ReadRaw(string path, int size) {
//		Debug.Log("reading heightmap "+path+" / size = "+size);
		byte[] buffer;
		using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read))) {
			buffer = reader.ReadBytes((size * size) * 2);
			reader.Close();
		}
		int heightmapWidth = MyTerrain.terrainData.heightmapWidth;
		int heightmapHeight = MyTerrain.terrainData.heightmapHeight;
//		Debug.Log("w/h = "+heightmapWidth+" / "+heightmapHeight);
		float[,] heights = new float[heightmapHeight, heightmapWidth];
		float num3 = 1.525879E-05f;
		for (int i = 0; i < heightmapHeight; i++) {
			if (i%10 == 0) EditorUtility.DisplayProgressBar("Importing heightmap", "calculating...", Mathf.InverseLerp(0.0f, (float)heightmapHeight, (float)i));
			for (int j = 0; j < heightmapWidth; j++) {
				int num6 = Mathf.Clamp(j, 0, size - 1) + (Mathf.Clamp(i, 0, size - 1) * size);
					byte num7 = buffer[num6 * 2];
					buffer[num6 * 2] = buffer[(num6 * 2) + 1];
					buffer[(num6 * 2) + 1] = num7;
				float num9 = System.BitConverter.ToUInt16(buffer, num6 * 2) * num3;
				heights[i, j] = num9;
			}
		}
		MyTerrain.terrainData.SetHeights(0, 0, heights);
	}

}

