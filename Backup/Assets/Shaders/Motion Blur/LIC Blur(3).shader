Shader "Hidden/LIC Blur" {
	Properties {
		_MainTex ("Source (RGBA)", 2D) = "white" {}
		_Velocity ("Velocity Field (RG)", 2D) = "gray" {}
		_AspectRatio ("Camera aspect ratio", float) = 1.0
	}

	SubShader {
		ZTest Always
		Cull Off
		ZWrite Off
		Fog { Mode off }
		Pass {
			Blend One Zero
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest 
				#include "UnityCG.cginc"
				
				uniform sampler2D _MainTex;
				uniform sampler2D _Velocity;
				// Modify blur steps here
				#define STEPS 10
				
				//This is for checking for anti-aliasing in D3D
				#if SHADER_API_D3D9
				uniform float2 _MainTex_TexelSize;
				#endif
				uniform float _globalstrength;
				float4 frag (v2f_img i) : COLOR
				{
					float4 output = float4(0,0,0,0);
					
					float2 velocityUV = i.uv;
					//When anti-aliasing is on, we have to invert our Y coordinate for D3D
					#if SHADER_API_D3D9
					if (_MainTex_TexelSize.y < 0)
						velocityUV.y = 1 - velocityUV.y;
					#endif
					
					float4 v2 = tex2D(_Velocity, velocityUV);
					float2 v = float2(DecodeFloatRG(v2.rg), DecodeFloatRG(v2.ba));
					
					v = (v - 0.5)*1;
					#if SHADER_API_D3D9
					// When anti-aliasing is off, we have to negate our Y velocity for D3D
					if (_MainTex_TexelSize.y >= 0)
						v.y = -v.y;
					#endif

							
					for (int offset = 0; offset <= STEPS; offset ++) {
					
					

						float	stepIntesity = 1.0/(STEPS + 1),
								stepLength = (0.5/STEPS);
						
						float4 stepSample = tex2D(_MainTex, i.uv +((v)* ((5.0-offset)*stepLength)));
						
						output += stepSample*stepIntesity;
					}
									
					return output;
				}
			ENDCG
		}
	}

	Fallback off

}