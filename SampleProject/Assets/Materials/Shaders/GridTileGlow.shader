Shader "Custom/GridTileGlow"
{
    Properties
    {
        _IdleColor ("Idle Edge Color", Color) = (0.93, 0.88, 0.78, 0.22)
        _StateColor ("State Color (lo pone el script)", Color) = (0.55, 0.72, 0.45, 1)
        _State ("State 0-1 (lo pone el script)", Range(0, 1)) = 0
        _Flash ("Flash 0-1 (lo pone el script)", Range(0, 1)) = 0

        _EdgeWidth ("Edge Width", Range(0.005, 0.15)) = 0.05
        _EdgeSoftness ("Edge Softness", Range(0.1, 1)) = 0.8
        _Glow ("Edge Brightness", Range(0, 1.5)) = 0.35
        _FillAlpha ("Fill Alpha", Range(0, 0.5)) = 0.10
        _Halo ("Top Halo Size", Range(0.02, 0.5)) = 0.22
        _FlashTint ("Flash Tint", Color) = (1, 0.95, 0.82, 1)
        _Sides ("Sides +x -x +z -z (lo pone el script)", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            // Alpha normal (no aditivo): colores mates, sin look neón
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _IdleColor;
                half4 _StateColor;
                half4 _FlashTint;
                float4 _Sides;
                float _State;
                float _Flash;
                float _EdgeWidth;
                float _EdgeSoftness;
                float _Glow;
                float _FillAlpha;
                float _Halo;
            CBUFFER_END

            struct Attributes { float4 posOS : POSITION; };
            struct Varyings
            {
                float4 posCS : SV_POSITION;
                float3 posOS : TEXCOORD0;
                float3 scale : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.posCS = TransformObjectToHClip(v.posOS.xyz);
                o.posOS = v.posOS.xyz;

                float4x4 m = GetObjectToWorldMatrix();
                o.scale = float3(length(float3(m._m00, m._m10, m._m20)),
                                 length(float3(m._m01, m._m11, m._m21)),
                                 length(float3(m._m02, m._m12, m._m22)));
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 p = i.posOS;
                float3 h = 0.5 * i.scale;
                float3 d = h - abs(p * i.scale);

                // Borde suave en las aristas
                float inner = _EdgeWidth * (1.0 - _EdgeSoftness);
                float3 a = 1.0 - smoothstep(inner, _EdgeWidth, d);

                // Lados unidos a otra losa del mismo grupo: sin borde ni cara
                float connX = p.x > 0 ? _Sides.x : _Sides.y;
                float connZ = p.z > 0 ? _Sides.z : _Sides.w;
                if ((d.x < 0.002 && connX > 0.5) || (d.z < 0.002 && connZ > 0.5))
                    return half4(0, 0, 0, 0);
                a.x *= 1.0 - connX;
                a.z *= 1.0 - connZ;

                float edge = max(a.x * a.y, max(a.y * a.z, a.x * a.z));

                // Halo muy suave en la cara superior
                float isTop = step(0.49, p.y);
                float dTop  = min(lerp(d.x, 10.0, connX), lerp(d.z, 10.0, connZ));
                float halo  = isTop * pow(saturate(1.0 - dTop / _Halo), 2.0);

                half3 col = lerp(_IdleColor.rgb, _StateColor.rgb, _State);

                // Borde un poco más claro que el relleno (sin pasarse de 1)
                col = saturate(col * (1.0 + edge * _Glow * (0.3 + _State)));

                float fill = _FillAlpha * (0.6 + 0.4 * isTop) * (1.0 + _State * 2.0);
                float alpha = fill
                            + edge * (_IdleColor.a + _State * 0.6)
                            + halo * _State * 0.35;

                // Destello cálido al colocar
                col = lerp(col, _FlashTint.rgb, _Flash * 0.5);
                alpha += _Flash * (edge * 0.5 + 0.15 * isTop);

                return half4(col, saturate(alpha));
            }
            ENDHLSL
        }
    }
}