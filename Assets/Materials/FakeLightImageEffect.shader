Shader "FakeLightImageEffect"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _FakeLightRenderTexture("Base (RGB)", 2D) = "_FakeLightRenderTexture" {}
        _Color("Glow Color", Color) = (1, 1, 1, 1)
        _Intensity("Intensity", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
    }

    SubShader
    {
        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct Attributes
        {
            float4 positionOS   : POSITION;
            float2 uv           : TEXCOORD0;
        };


        struct Varyings
        {
            float2 uv        : TEXCOORD0;
            float4 vertex : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        TEXTURE2D_X(_MainTex);
        SAMPLER(sampler_MainTex);

        TEXTURE2D_X(_FakeLightRenderTexture);
        SAMPLER(sampler_FakeLightRenderTexture);

        half4 _Color;
        half _Intensity;

        Varyings Vertex(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
            output.vertex = vertexInput.positionCS;
            output.uv = input.uv;

            return output;
        }

        half4 Fragment(Varyings input) : SV_Target
        {
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
            half4 mainColor = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv);
            half4 lightColor = SAMPLE_TEXTURE2D_X(_FakeLightRenderTexture, sampler_FakeLightRenderTexture,uv);
            half4 color = mainColor * lightColor * 2;

            color.a = 1;
            return color;
        }

        ENDHLSL

        Pass
        {
            Blend[_SrcBlend][_DstBlend]
            ZTest Always    
            ZWrite Off      
            Cull Off        

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment        

            ENDHLSL
        }
    }
}