using UnityEngine;
using System.Collections;

[AddComponentMenu("Material/Draw Vector Field")]



public class ObjectBlur : MonoBehaviour {
	
	public float BlurStrength = 1.0f;
	// Material settings
	Material	m_stretchMaterial,
				m_regularMaterial;
	
	//Position record
	Matrix4x4 m_prevModelMatrix;
	
	protected void Start() {
		// Set up materials
		m_stretchMaterial = MotionVectorMaterialFactory.NewMaterial();
		m_regularMaterial = renderer.material;
		
		// Copy current matrix/position (Avoids blurring the first frame)
		m_prevModelMatrix = transform.localToWorldMatrix;
		
	}
	
	// Avoid processing when not enabled
	protected void OnEnable() {
		MotionBlurEffect.RegisterObject(this);
	}
	
	protected void OnDisable() {
		MotionBlurEffect.DeregisterObject(this);
	}
	
	// LateUpdate should happen after most movement
	protected void LateUpdate() {
		Vector4 currentPosition = transform.position;
		currentPosition.w = 1f;
		// Calculate ModelView matrices
		Matrix4x4 _mv = CameraInfo.ViewMatrix*transform.localToWorldMatrix;
		Matrix4x4 _mvPrev = CameraInfo.PrevViewMatrix*m_prevModelMatrix;
		// Give material the matrices it needs
		m_stretchMaterial.SetMatrix("_mv", _mv);
			m_stretchMaterial.SetFloat("_Strength", BlurStrength);
						m_stretchMaterial.SetFloat("_CamDistance", Vector3.Distance(Camera.main.transform.position,transform.position));
		m_stretchMaterial.SetMatrix("_mvPrev", _mvPrev);
		m_stretchMaterial.SetMatrix("_mvInvTrans", _mv.transpose.inverse);
		m_stretchMaterial.SetMatrix("_mvpPrev", CameraInfo.PrevViewProjMatrix*m_prevModelMatrix);
		// Record our previous transform
		m_prevModelMatrix = transform.localToWorldMatrix;
	}
	
	// Set up for motion vector rendering
	public void PreMotionRender() {
		m_regularMaterial = renderer.material;
		renderer.material = m_stretchMaterial;
	}
	
	// Go back to normal rendering
	public void PostMotionRender() {
		renderer.material = m_regularMaterial;
	}
}