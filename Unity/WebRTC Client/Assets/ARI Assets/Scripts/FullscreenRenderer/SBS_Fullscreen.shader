Shader "Hidden/SBS_Fullscreen"
{
    Properties
    {
        _BlitTexture ("Blit Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

           Varyings Vert(Attributes IN)
{
    UNITY_SETUP_INSTANCE_ID(IN);
    
    Varyings v;
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(v);
    
    v.positionCS = TransformObjectToHClip(IN.positionOS);
    v.uv = IN.uv;
    
    return v;
}


            half4 Frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                float2 uv = IN.uv;
                uv.x = uv.x * 0.5 + 0.5 * unity_StereoEyeIndex;
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
            }
            ENDHLSL
        }
    }
}
