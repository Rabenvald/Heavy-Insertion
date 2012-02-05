#pragma strict
#pragma implicit
#pragma downcast

@script ExecuteInEditMode
@script AddComponentMenu("Image Effects/CameraMBlur")
@script RequireComponent(Camera)
private var values : Matrix4x4;
var compositeShader : Shader;
var Strength = 250.0;
var Clamps = 0.3;
var DistanceMultiplier = 2.0;
var RotationalMultiplier = 2.0;
private var m_CompositeMaterial : Material;


 
 function Start(){
Camera.main.depthTextureMode = DepthTextureMode.Depth;
 }

 
private function GetCompositeMaterial() : Material {
	if (m_CompositeMaterial == null) {
		m_CompositeMaterial = new Material( compositeShader );
		m_CompositeMaterial.hideFlags = HideFlags.HideAndDontSave;
	}
	return m_CompositeMaterial;
} 
function OnDisable() {	
	DestroyImmediate (m_CompositeMaterial);
}
function OnPreCull()
{





}

// Called by the camera to apply the image effect
function OnRenderImage (source : RenderTexture, destination : RenderTexture) : void
{


			var compositeMat = GetCompositeMaterial();
compositeMat.SetMatrix("_Myviewprev", values);

//var projectionmatrix=Camera.main.projectionMatrix;
		var viewmatrix=camera.worldToCameraMatrix.transpose;
		var fix=viewmatrix.GetRow(0);
	var fix2=viewmatrix.GetRow(1);
		var fix3=viewmatrix.GetRow(2);
			var fix4=viewmatrix.GetRow(3);
						//var fix5=projectionmatrix.GetRow(2);
fix.z=-fix.z;
fix2.z=-fix2.z;
fix3.x=-fix3.x;
fix3.y=-fix3.y;
fix4.z=-fix4.z;


//fix5.x=-fix5.x;
//fix5.y=-fix5.y;
//fix5.z=-fix5.z;
//fix5.w=-fix5.w;


//	projectionmatrix.SetRow(2,fix5);

	viewmatrix.SetRow(0,fix);
	viewmatrix.SetRow(1,fix2);
	viewmatrix.SetRow(2,fix3);
	viewmatrix.SetRow(3,fix4);

		var Iview=(viewmatrix); 
		values=(Iview);
		

	compositeMat.SetFloat("_Strength", Strength);
	compositeMat.SetFloat("_texWidth", Screen.width);
		compositeMat.SetFloat("_texHeight", Screen.height);

				compositeMat.SetMatrix("_Myview", values.inverse);
compositeMat.SetFloat("_smoothTime", Time.smoothDeltaTime);
compositeMat.SetFloat("_Bonus", DistanceMultiplier);
compositeMat.SetFloat("_RotBonus", RotationalMultiplier);
compositeMat.SetFloat("_Clamp", Clamps);






	Graphics.Blit(source, destination, compositeMat);

}


