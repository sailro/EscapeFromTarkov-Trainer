//This version of the shader does not support shadows, but it does support transparent outlines

Shader "Outlined/UltimateOutline"
{
	Properties
	{
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex("Texture", 2D) = "white" {}

		_FirstOutlineColor("Outline color", Color) = (1,0,0,0.5)
		_FirstOutlineWidth("Outlines width", Range(0.0, 2.0)) = 0.15

		_SecondOutlineColor("Outline color", Color) = (0,0,1,1)
		_SecondOutlineWidth("Outlines width", Range(0.0, 2.0)) = 0.025

		_Angle("Switch shader on angle", Range(0.0, 180.0)) = 89
		
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 8 //"Always"
	}

		CGINCLUDE
#include "UnityCG.cginc"

			struct appdata {
			float4 vertex : POSITION;
			float4 normal : NORMAL;
		};

		uniform float4 _FirstOutlineColor;
		uniform float _FirstOutlineWidth;

		uniform float4 _SecondOutlineColor;
		uniform float _SecondOutlineWidth;

		uniform sampler2D _MainTex;
		uniform float4 _Color;
		uniform float _Angle;

		ENDCG

			SubShader{
			//First outline
			Pass{
				Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off
				ZTest [_ZTest]
				Cull Back
				CGPROGRAM

				struct v2f {
					float4 pos : SV_POSITION;
				};

				#pragma vertex vert
				#pragma fragment frag

				v2f vert(appdata v) {
					appdata original = v;

					float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
					//This shader consists of 2 ways of generating outline that are dynamically switched based on demiliter angle
					//If vertex normal is pointed away from object origin then custom outline generation is used (based on scaling along the origin-vertex vector)
					//Otherwise the old-school normal vector scaling is used
					//This way prevents weird artifacts from being created when using either of the methods
					if (degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle) {
						v.vertex.xyz += normalize(v.normal.xyz) * _FirstOutlineWidth;
					}
	else {
	   v.vertex.xyz += scaleDir * _FirstOutlineWidth;
   }

   v2f o;
   o.pos = UnityObjectToClipPos(v.vertex);
   return o;
}

half4 frag(v2f i) : COLOR{
	return _FirstOutlineColor;
}

ENDCG
}


//Second outline
Pass{
	Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	ZWrite Off
	ZTest [_ZTest]
	Cull Back
	CGPROGRAM

	struct v2f {
		float4 pos : SV_POSITION;
	};

	#pragma vertex vert
	#pragma fragment frag

	v2f vert(appdata v) {
		appdata original = v;

		float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
		//This shader consists of 2 ways of generating outline that are dynamically switched based on demiliter angle
		//If vertex normal is pointed away from object origin then custom outline generation is used (based on scaling along the origin-vertex vector)
		//Otherwise the old-school normal vector scaling is used
		//This way prevents weird artifacts from being created when using either of the methods
		if (degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle) {
			v.vertex.xyz += normalize(v.normal.xyz) * _SecondOutlineWidth;
		}
	else {
		v.vertex.xyz += scaleDir * _SecondOutlineWidth;
	}

	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	return o;
	}

	half4 frag(v2f i) : COLOR{
		return _SecondOutlineColor;
	}

	ENDCG
}

//Surface shader
Tags{ "Queue" = "Transparent" }

CGPROGRAM
#pragma surface surf Lambert noshadow

struct Input {
	float2 uv_MainTex;
	float4 color : COLOR;
};

void surf(Input IN, inout SurfaceOutput  o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
		}
			Fallback "Diffuse"
} 