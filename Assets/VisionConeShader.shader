Shader "Custom/VisionConeShader"
{
    Properties
    {
        _Color ("Vision Color (Additive/Soft Tint)", Color) = (1, 1, 0.9, 0.35)
        _CenterColor ("Center Brightness", Color) = (1, 1, 1, 0.6)
        _FalloffPower ("Falloff Power", Range(0.5, 4.0)) = 1.8
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent+10" 
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float dist : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _CenterColor;
            float _FalloffPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                // v.vertex local mide la distancia desde el jugador (origen 0,0,0)
                o.dist = length(v.vertex.xy);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Atenuación suave desde el jugador hacia el borde del cono
                // Las coordenadas UV están normalizadas entre 0 y 1 con 0.5 en el centro
                float2 fromCenter = (i.uv - 0.5) * 2.0;
                float radial = clamp(length(fromCenter), 0.0, 1.0);
                float falloff = pow(1.0 - radial, _FalloffPower);

                fixed4 finalColor = lerp(_Color, _CenterColor, falloff * 0.5);
                finalColor.a *= falloff;

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
