Shader "ZShaderSet/FullScreen/Darkness"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_DarknessValue("Darkness", Range(0,1)) = 0
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precission_hint_fastest
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			fixed _DarknessValue;

			fixed4 frag(v2f_img i) :COLOR
			{
				fixed4 renderTex = tex2D(_MainTex, i.uv);
				fixed4 finalColor = lerp(renderTex, float4(0,0,0,1), _DarknessValue);
				return finalColor;
			}
			ENDCG
		}
	}
}

