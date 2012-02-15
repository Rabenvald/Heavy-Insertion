using UnityEngine;
using System.Collections;

// This is a helper class to avoid repeated Shader.Find() calls
public class MotionVectorMaterialFactory {
	static Shader m_motionVectorShader;
	
	public static Shader MotionVectorShader {
		get {
			if (!m_motionVectorShader)
				m_motionVectorShader = Shader.Find("Hidden/Motion Vectors");
			return m_motionVectorShader;
		}
	}
	
	public static Material NewMaterial() {
		return new Material(MotionVectorShader);
	}
}