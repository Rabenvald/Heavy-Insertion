// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "CameraZBlur" {
Properties {
	_MainTex ("", RECT) = "white" {}
		_Normal ("_Normal", 2D) = "white" {}
	_Strength ("Strength", Range (1, 30)) = 15.0
		_Velocity ("Velocity", Vector)=(0,0,0)
		
		
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
	float2 uv2	: TEXCOORD2;

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
uniform sampler2D _DepthTex;
	sampler2D _CameraDepthTexture;
uniform sampler2D _MainTex;
uniform sampler2D _Normal;

float4 _MainTex_TexelSize;
uniform float _texWidth;
uniform float _texHeight;
uniform float3 _Velocity;
uniform float _smoothTime;
uniform float _Static;
uniform float _distancefactor;



half4 frag (v2f_img i) : COLOR
{

   float2 Texcoord =i.uv;
     float z= tex2D(_CameraDepthTexture,Texcoord).r;
	 z=saturate(1-z*_distancefactor);
   

   
	float3 velocity=_Velocity;
	velocity *=0.1;
		velocity.xy *=-_Strength;
	float x=i.uv.x;
	float y=i.uv.y;
	
float xfactor=(1.0-clamp(velocity.x*sign(velocity.z),-1,1));
float yfactor=(1.0-clamp(velocity.y*sign(velocity.z),-1,1));
velocity.x=(x*2-1)*0.2+(x*2-xfactor)*velocity.z*z;
velocity.y=(y*2-1)*0.2+(y*2-yfactor)*velocity.z*z;
	//velocity.x=	(x*2-1)*0.05+(x*2-xfactor)*velocity.z*z;
	//velocity.y=	(y*2-1)*0.05+(y*2-yfactor)*velocity.z*z;
	_Static*=0.01;
	if (_Static > 0)
	
{
velocity.x=	(x*2-1)*_Static*z;
	velocity.y=	(y*2-1)*_Static*z;
}
	float2 velo=(velocity.x,velocity.y);
 

	
	
    // Get the initial color at this pixel.   
   float4 color = tex2D(_MainTex, Texcoord);    
	_Strength *=0.01;
	
	Texcoord += velocity.xy*_Strength;   
	for(int i = 1; i < 10; ++i, Texcoord -= velocity.xy*_Strength)
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