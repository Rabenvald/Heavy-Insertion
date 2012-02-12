using UnityEngine;
using System.Collections;

public class CraterMaker : MonoBehaviour {
	public Terrain MyTerrain;
	public int insidetextureindex=0;

	public void Create(Vector3 position, float radius, float depth, float noise) {
		Create(new Vector2(position.x, position.z), radius, depth, noise);
	}
	
	public void Create(Vector2 position, float radius, float depth, float noise) {
		StartCoroutine(RealCreate(position, radius, depth, noise));
	}
		
	public IEnumerator RealCreate(Vector2 position, float radius, float depth, float noise) {
		TerrainData tdata = MyTerrain.terrainData;
		Vector3 size = tdata.size;
		Vector3 pos = MyTerrain.transform.position;

		position.x -= pos.x;
		position.y -= pos.y;
		float scale = (float)tdata.heightmapResolution / size.x;
		int width = (int)Mathf.Floor( radius * scale);
		int xpos = (int)Mathf.Floor((position.x-radius)*scale);
		int ypos = (int)Mathf.Floor((position.y-radius)*scale);

		float[,] heights = tdata.GetHeights(xpos, ypos, width*2, width*2);
		float heightscale = depth / (size.y*2);

		for (int i=0; i<width*2; i++) for (int j=0; j<width*2; j++) {
			float mod = Mathf.SmoothStep((float)1, (float)0, Mathf.Abs((float)width-(float)i)/(float)width) * Mathf.SmoothStep((float)1, (float)0, Mathf.Abs((float)width-(float)j)/(float)width);
			mod = mod*heightscale;
			if (noise>0) mod += mod * heightscale * depth * Random.value * noise;
			heights[i,j] -= mod;
		}
		tdata.SetHeights(xpos, ypos, heights);
		
		// wait one frame because otherwise we cause even more stuttering
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		
		scale = (float)tdata.alphamapResolution / size.x;
		width = (int)Mathf.Floor( radius * scale);
		xpos = (int)Mathf.Floor((position.x-radius)*scale);
		ypos = (int)Mathf.Floor((position.y-radius)*scale);
		
		float[,,] textures = tdata.GetAlphamaps(xpos, ypos, width*2, width*2);
		int splats = textures.Length/(width*width*4);
		for (int i=0; i<width*2; i++) for (int j=0; j<width*2; j++) {
			float mod = Mathf.SmoothStep((float)1, (float)0, Mathf.Abs((float)width-(float)i)/(float)width) * Mathf.SmoothStep((float)1, (float)0, Mathf.Abs((float)width-(float)j)/(float)width);
			textures[i,j,insidetextureindex] += mod;
			for (int s=0; s<splats; s++) {
				if (s==insidetextureindex) {
					textures[i,j,s] += mod;
				} else {
					textures[i,j,s] -= textures[i,j,s]*mod;
				}
			}
			// re-normalize
			float sum = 0;
			for (int s=0; s<splats; s++) {
				sum += textures[i,j,s];
			}
			for (int s=0; s<splats; s++) {
				textures[i,j,s] *= (float)1/sum;
			}
		}
		tdata.SetAlphamaps(xpos, ypos, textures);
	}

}
