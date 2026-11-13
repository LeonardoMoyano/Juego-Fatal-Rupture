Shader "Universal Render Pipeline/2D/SoftEdgeDarkness"
{
    Properties
    {
        _DarknessColor ("Color de Oscuridad", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent+100" 
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
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _DarknessColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // El canal Alpha de los vértices define la opacidad:
                // 0 = 100% transparente (dentro de la visión)
                // 1 = 100% negro (oscuridad total exterior)
                // Valores intermedios = degradado / difuminado suave estilo Project Zomboid
                half4 col = _DarknessColor;
                col.a *= input.color.a;
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Sprites/Default"
}
