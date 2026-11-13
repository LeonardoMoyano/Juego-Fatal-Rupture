Shader "Universal Render Pipeline/2D/VisionConeShader"
{
    Properties
    {
        _VisionColor ("Color de Vision (Centro)", Color) = (1, 1, 0.95, 0.9)
        _AmbientColor ("Color Penumbra / Borde", Color) = (0.7, 0.7, 0.7, 0.3)
        _Smoothness ("Suavizado en Bordes", Range(0.5, 4.0)) = 1.6
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent+50" 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _VisionColor;
                float4 _AmbientColor;
                float _Smoothness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // input.color.a y input.uv.y contienen el factor de atenuación calculado en el script
                float fade = saturate(input.color.a);
                fade = pow(fade, _Smoothness);

                half4 finalColor = lerp(_AmbientColor, _VisionColor, fade);
                finalColor.a = fade * _VisionColor.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Sprites/Default"
}
