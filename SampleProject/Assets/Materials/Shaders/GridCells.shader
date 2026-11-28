Shader "Custom/GridCells"
{
    Properties
    {
        _StateTex ("State Texture", 2D) = "black" {}
        _GridSize ("Grid Size (cols, rows)", Vector) = (8, 8, 0, 0)
        _LineWidth ("Line Width", Range(0.005, 0.2)) = 0.04

        _GridColor ("Grid Line Color", Color) = (1, 1, 1, 0.3)
        _OccupiedColor ("Occupied Color", Color) = (0.2, 0.6, 1, 1)
        _ValidColor ("Valid Color", Color) = (0, 1, 0, 1)
        _InvalidColor ("Invalid Color", Color) = (1, 0.1, 0.1, 1)
        _PulseSpeed ("Pulse Speed", Float) = 6

        _GlowIntensity ("Glow Intensity (HDR)", Range(1, 8)) = 3
        _HaloSize ("Halo Size", Range(0.05, 0.5)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_StateTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _GridSize;
                float _LineWidth;
                half4 _GridColor;
                half4 _OccupiedColor;
                half4 _ValidColor;
                half4 _InvalidColor;
                float _PulseSpeed;
                float _GlowIntensity;
                float _HaloSize;
            CBUFFER_END

            struct Attributes { float4 posOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.posCS = TransformObjectToHClip(v.posOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 gp   = i.uv * _GridSize.xy;
                int2   cell = min((int2)floor(gp), (int2)_GridSize.xy - 1);
                float2 f    = frac(gp);

                half4 s = LOAD_TEXTURE2D(_StateTex, cell);

                // Distancia al borde de la casilla
                float2 d    = min(f, 1.0 - f);
                float edge  = min(d.x, d.y);
                float border = 1.0 - smoothstep(_LineWidth * 0.5, _LineWidth, edge);

                // Halo: intenso junto al borde, se apaga hacia el centro
                float halo = saturate(1.0 - edge / _HaloSize);
                halo *= halo;

                // Pulsos
                float pulseFast = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed);
                float pulseSlow = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed * 0.35 + (cell.x + cell.y) * 0.6);

                half3 stateRgb = 0;
                float pulse = 1.0;
                if (s.g > 0.5)      { stateRgb = _ValidColor.rgb;    pulse = pulseFast; }
                else if (s.b > 0.5) { stateRgb = _InvalidColor.rgb;  pulse = pulseFast; }
                else if (s.r > 0.5) { stateRgb = _OccupiedColor.rgb; pulse = pulseSlow; }

                float hasState = (s.r + s.g + s.b) > 0.5 ? 1.0 : 0.0;

                // Relleno + halo + borde
                float fill   = lerp(0.10, 0.28, pulse);
                float stateA = hasState * saturate(fill + halo * 0.55 + border * 0.9);

                // Glow HDR: más brillo en borde y halo (el Bloom lo hace brillar)
                float glow = lerp(1.0, _GlowIntensity, saturate(border + halo * 0.5));
                stateRgb *= glow * lerp(0.8, 1.0, pulse);

                // Rejilla base con una onda de brillo que la recorre
                float wave  = 0.5 + 0.5 * sin((gp.x + gp.y) * 1.2 - _Time.y * 2.0);
                float gridA = _GridColor.a * border * lerp(0.5, 1.0, wave);

                half3 rgb = lerp(_GridColor.rgb, stateRgb, hasState);
                float a   = max(gridA, stateA);
                return half4(rgb, a);
            }
            ENDHLSL
        }
    }
}