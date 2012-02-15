using UnityEngine;

// Easy access and production of camera information from current/previous frame

[AddComponentMenu("Camera/CameraInfo")]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraInfo : MonoBehaviour {
	public static Matrix4x4 ViewMatrix {get; private set;}
	public static Matrix4x4 ProjectionMatrix {get; private set;}
	public static Matrix4x4 ViewProjectionMatrix {get; private set;}
	public static Matrix4x4 PrevViewMatrix {get; private set;}
	public static Matrix4x4 PrevProjectionMatrix {get; private set;}
	public static Matrix4x4 PrevViewProjMatrix {get; private set;}
	
	bool m_d3d;
	
	protected void Awake() {
		m_d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
		
		ViewMatrix = Matrix4x4.identity;
		ProjectionMatrix = Matrix4x4.identity;
		ViewProjectionMatrix = Matrix4x4.identity;
		PrevViewMatrix = Matrix4x4.identity;
		PrevProjectionMatrix = Matrix4x4.identity;
		PrevViewProjMatrix = Matrix4x4.identity;
		
		UpdateCurrentMatrices();
	}
	
	protected void Update() {
		
		// Copy old matrices
		PrevViewMatrix = ViewMatrix;
		PrevProjectionMatrix = ProjectionMatrix;
		PrevViewProjMatrix = ViewProjectionMatrix;
		
		
		UpdateCurrentMatrices();
	}
	
	// Generate V, P and VP matrices
	void UpdateCurrentMatrices() {
		ViewMatrix = camera.worldToCameraMatrix;
		Matrix4x4 p = camera.projectionMatrix;
		if (m_d3d) {
			// Invert Y for rendering to a render texture
			for (int i = 0; i < 4; i++) {
				p[1,i] = -p[1,i];
			}
			// Scale and bias from OpenGL -> D3D depth range
			for (int i = 0; i < 4; i++) {
				p[2,i] = p[2,i]*0.5f + p[3,i]*0.5f;
			}
		}
		ProjectionMatrix = p;
		ViewProjectionMatrix = ProjectionMatrix*ViewMatrix;
	}
}
