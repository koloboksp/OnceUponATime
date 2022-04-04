// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FakeLightImageEffect"
{
	Properties
	{
		_MainTex("Source", 2D) = "white" {}
		_LightingTex("Lighting", 2D) = "white" {}
	}
	
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
	
		Pass
		{
			CGPROGRAM
			#pragma vertex vertexShader
			#pragma fragment fragmentShader
			
			#include "UnityCG.cginc"
			
			struct vertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			
			struct vertexOutput
			{
				float2 texcoord : TEXCOORD0;
				float4 position : SV_POSITION;
			};
			
			vertexOutput vertexShader(vertexInput i)
			{
				vertexOutput o;
				o.position = UnityObjectToClipPos(i.vertex);
				o.texcoord = i.texcoord;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _LightingTex;

			
			float4 fragmentShader(vertexOutput i) : COLOR
			{
				float4 srcColor = tex2D(_MainTex, i.texcoord);
				float4 lightingColor = tex2D(_LightingTex, i.texcoord);

				return srcColor * lightingColor;
			}
			ENDCG
		}
	}
	Fallback Off
}