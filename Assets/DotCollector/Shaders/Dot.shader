// Dot.shader — URP Unlit, GPU instanced, one draw call
// Renders each dot as a smooth circle billboard.
// Draws 2 triangles (6 vertices) per instance using SV_VertexID 0-5.

Shader "Custom/DotInstanced"
{
    Properties { }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct DotState
            {
                float2 position;
                float  scale;
                float  maxScale;
                float  state;   // 0=alive  1=collecting (being pulled)  2=dead
            };

            StructuredBuffer<DotState> _DotBuffer;

            struct Varyings
            {
                float4 posCS    : SV_POSITION;
                float2 uv       : TEXCOORD0;   // [0,1] across quad
                float  state    : TEXCOORD1;
                float  sizeFrac : TEXCOORD2;
            };

            // Two triangles as a quad, no index buffer needed.
            // vertID: 0  1  2  3  4  5
            //  tri0:  BL BR TL | tri1: BR TR TL
            // local XY in [-0.5, 0.5], UV in [0,1]
            void QuadVertex(uint vertID, out float2 pos, out float2 uv)
            {
                // bit tricks: unpack a 2-tri strip from vertID
                uint xBit = (vertID == 1 || vertID == 2 || vertID == 4 || vertID == 5) ? 1u : 0u;
                uint yBit = (vertID == 2 || vertID == 3 || vertID == 4 || vertID == 5) ? 1u : 0u;

                // Remap so tri winding is correct for both triangles
                //  vertID  xBit yBit
                //    0      0    0   BL
                //    1      1    0   BR
                //    2      0    1   TL
                //    3      1    0   BR  (degenerate — won't be reached with 2-tri)
                // Cleaner: just hard-code 6 entries
                const float2 POS[6] = {
                    float2(-0.5, -0.5),  // 0 BL
                    float2( 0.5, -0.5),  // 1 BR
                    float2(-0.5,  0.5),  // 2 TL
                    float2( 0.5, -0.5),  // 3 BR
                    float2( 0.5,  0.5),  // 4 TR
                    float2(-0.5,  0.5)   // 5 TL
                };
                const float2 UV[6] = {
                    float2(0, 0),
                    float2(1, 0),
                    float2(0, 1),
                    float2(1, 0),
                    float2(1, 1),
                    float2(0, 1)
                };
                pos = POS[vertID];
                uv  = UV[vertID];
            }

            Varyings Vert(uint vertID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                DotState d = _DotBuffer[instanceID];

                float2 localPos;
                float2 uv;
                QuadVertex(vertID, localPos, uv);

                float3 worldPos = float3(d.position + localPos * d.scale, 0.0);

                Varyings o;
                o.posCS    = TransformWorldToHClip(worldPos);
                o.uv       = uv;
                o.state    = d.state;
                o.sizeFrac = saturate(d.maxScale > 0.001 ? d.scale / d.maxScale : 1.0);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                // Map UV [0,1] → centred [-1,1] for SDF circle
                float2 c    = i.uv * 2.0 - 1.0;
                float  dist = length(c);

                // Discard outside circle
                clip(1.0 - dist - 0.005);

                // Smooth anti-aliased rim
                float alpha = smoothstep(1.0, 0.85, dist);

                // Fade in as dot grows from seed → full size
                alpha *= smoothstep(0.0, 0.3, i.sizeFrac);

                // --- Color ---
                // alive      → teal-green with slight inner highlight
                // collecting → bright cyan pulse (state == 1 set by VacuumCollect)
                // dead       → transparent
                half3 aliveCol  = lerp(half3(0.3, 0.85, 0.6), half3(0.5, 1.0, 0.75),
                                       smoothstep(0.6, 0.0, dist)); // subtle inner glow
                half3 pullCol   = half3(0.4, 0.95, 1.0);            // cyan when being pulled
                half3 col       = i.state < 0.5 ? aliveCol : pullCol;

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
