Shader "Custom/FakeLight"
{
	Properties
	{
		_Range("Range", Range(0,30)) = 10
		_Color("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags 
		{ 
			"DisableBatching" = "True"
			"RenderType"="Transparent" 
		}
	
		Pass
		{
			Blend One One
			Cull Off
			ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			
			struct appdata
			{
				float4 vertex : POSITION;
				
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;

				float3 lVertex : TEXCOORD0;
				float4 color : COLOR;
				UNITY_FOG_COORDS(1)			
			};

			sampler2D _MainTex;
			
			float4 _MainTex_ST;
			float _Range;
			half4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.lVertex = v.vertex * _Range * 2;
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float atten = saturate((_Range - length(i.lVertex)) / _Range);
				fixed4 col = atten * atten * _Color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
