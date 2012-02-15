// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "CameraMBlur" {
Properties {
	_MainTex ("", RECT) = "white" {}
	_Strength ("Strength", Range (1, 30)) = 15.0
		_Bonus ("Bonus", Float) =1.0
		_Clamp ("Clamp", Float) =1.0
}

SubShader {
	Pass {
		ZTest Always Cull off ZWrite Off Fog { Mode off }

CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 
#include "UnityCG.cginc"



struct v2f { 
	float4 pos	: POSITION;
	float2 uv	: TEXCOORD1;

}; 
uniform float4 _MainTex_ST;
v2f vert (appdata_img v)
{

	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
o.uv=TRANSFORM_TEX(v.texcoord, _MainTex);
#if SHADER_API_D3D9
if (_MainTex_TexelSize.y < 0)
        o.uv.y = 1-o.uv.y;
#endif
	return o;
}

uniform float4x4 _Myview;
uniform float4x4 _Myviewprev;
uniform float _Strength;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _MainTex;

float4 _MainTex_TexelSize;
uniform float _texWidth;
uniform float _texHeight;
uniform float _Clamp;
uniform float _smoothTime;
uniform float _Bonus;
uniform float _RotBonus;

half4 frag (v2f_img i) : COLOR
{


float2 Texcoord =i.uv;
  // Get the depth buffer value at this pixel.   
  float z= tex2D(_CameraDepthTexture,Texcoord).r;
// H is the viewport position at this pixel in the range -1 to 1.   
float4 H = float4((Texcoord.x*2-1), (Texcoord.y*2-1),_RotBonus, 1);
//float4 H = float4((Texcoord.x*2-1), (Texcoord.y*2-1),(z)*_Bonus, 1);

	
// Transform by the view-projection inverse.   
   float4 D = mul(H,_Myview);   
    // float4 D = mul(H,_Myview);   
// Divide by w to get the world position.   
   float4 worldPos = D /D.w;  
     // Current viewport position   
   float4 currentPos = H;   
// Use the world position, and transform by the previous view-   
   // projection matrix.   
   float4 previousPos = mul(worldPos,_Myviewprev);   
      //float4 previousPos = mul(worldPos,_Myviewprev);  
// Convert to nonhomogeneous points [-1,1] by dividing by w.   
previousPos /= previousPos.w;   
// Use this frame's position and last frame's to compute the pixel   
   // velocity.   
   //previousPos=lerp(currentPos,previousPos,_smoothTime);
   float2 texsize= (_texWidth,_texHeight);
   float2 vel=(currentPos - previousPos)*saturate((z)*_Bonus);
    float2 velocity = clamp(vel,-_Clamp,_Clamp)/_Strength;  

  
   
    // Get the initial color at this pixel.   
   float4 color = tex2D(_MainTex, Texcoord);   
Texcoord += velocity;   
for(int i = 1; i < 10; ++i, Texcoord += velocity )
{   
  // Sample the color buffer along the velocity vector.   
   float4 currentColor = tex2D(_MainTex, Texcoord);   
  // Add the current color to our color sum.   
  color += currentColor;   
  

}   
// Average all of the samples to get the final blur color.   
   float4 finalColor = color / 10.0;  
    	
	return finalColor;
}
ENDCG
	}
}

Fallback off

}