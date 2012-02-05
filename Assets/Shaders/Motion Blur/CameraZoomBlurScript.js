

private var values : Vector3 ;

var Strength = 0.5;
public var smoothing : float = 0.1;
var isStatic = 0.0;
var DistanceMultiplier = 15.0;
private var zVelocity = 0.0;
private var yVelocity = 0.0;
private var xVelocity = 0.0;

@script ExecuteInEditMode
@script AddComponentMenu("Image Effects/CameraZBlur")
@script RequireComponent(Camera)

class CameraZoomBlurScript extends PostEffectsBase {

	public var ApplyShader : Shader;
	private var _ApplyMaterial : Material = null;	
	
	function CreateMaterials () {

		if (!_ApplyMaterial) {
			if(!CheckShader(ApplyShader)) {
				enabled = false;
				return;
			}
			_ApplyMaterial = new Material(ApplyShader);	
			_ApplyMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		
		if(!SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth)) {
			enabled = false;
			return;	
		}
	}
	
	function Start () {
		CreateMaterials ();
	}
	
	function OnEnable() {
		camera.depthTextureMode |= DepthTextureMode.Depth;	
	}
	
	


function OnRenderImage (source : RenderTexture, destination : RenderTexture)
	{	
		CreateMaterials ();
		var oldSpeed=values;
var velocity=Camera.main.transform.InverseTransformDirection(Camera.main.velocity);

	values.x=Mathf.SmoothDamp(oldSpeed.x, velocity.x-oldSpeed.x,xVelocity,smoothing);
		values.y=Mathf.SmoothDamp(oldSpeed.y, velocity.y-oldSpeed.y,yVelocity,smoothing);
	values.z=Mathf.SmoothDamp(oldSpeed.z, velocity.z-oldSpeed.z,zVelocity,smoothing);

	_ApplyMaterial.SetVector("_Velocity", values);
	_ApplyMaterial.SetFloat("_texWidth", Screen.width);
	_ApplyMaterial.SetFloat("_texHeight", Screen.height);
	_ApplyMaterial.SetFloat("_Strength", Strength);
		_ApplyMaterial.SetFloat("_Static", isStatic);
		_ApplyMaterial.SetFloat("_distancefactor", DistanceMultiplier);

		Graphics.Blit(source,destination,_ApplyMaterial);	

	}	
}


