Shader "Universal Render Pipeline/2D/MaskCutoutShader"
{
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent-20" 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "MaskPass"
            ZWrite Off
            Cull Off
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
