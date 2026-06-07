Shader "Custom/TiltShift"
{
    Properties
    {
        _BlurSize      ("Blur Size",     Float) = 4.0
        _FocusCenter   ("Focus Center",  Float) = 0.5
        _FocusWidth    ("Focus Width",   Float) = 0.1
        _FocusFalloff  ("Focus Falloff", Float) = 0.15
        _Saturation    ("Saturation",    Float) = 1.3
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    // NOTE: Blit.hlsl already declares:
    //   TEXTURE2D_X(_BlitTexture);
    //   SAMPLER(sampler_LinearClamp);
    //   float4 _BlitTexture_TexelSize;
    //   float4 _BlitScaleBias;
    // Do NOT redeclare any of them here.

    float _BlurSize;
    float _FocusCenter;
    float _FocusWidth;
    float _FocusFalloff;
    float _Saturation;

    static const int   TAPS      = 9;
    static const float OFFSETS[9] = { -4.0, -3.0, -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0 };
    static const float WEIGHTS[9] = {
        0.0162, 0.0540, 0.1216, 0.1945, 0.2270,
        0.1945, 0.1216, 0.0540, 0.0162
    };

    float FocusMask(float uvY)
    {
        float dist = abs(uvY - _FocusCenter) - _FocusWidth;
        return saturate(dist / max(_FocusFalloff, 0.0001));
    }

    float3 AdjustSaturation(float3 col, float sat)
    {
        float lum = dot(col, float3(0.2126, 0.7152, 0.0722));
        return lerp(float3(lum, lum, lum), col, sat);
    }

    // Pass 0 — Horizontal Gaussian blur
    float4 FragHorizontal(Varyings input) : SV_Target
    {
        float2 uv   = input.texcoord;
        float  step = _BlurSize * _BlitTexture_TexelSize.x;

        float3 col = 0;
        UNITY_UNROLL
        for (int i = 0; i < TAPS; ++i)
            col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp,
                       uv + float2(OFFSETS[i] * step, 0)).rgb * WEIGHTS[i];

        return float4(col, 1);
    }

    // Pass 1 — Vertical blur + focus-band composite + saturation
    float4 FragVertical(Varyings input) : SV_Target
    {
        float2 uv   = input.texcoord;
        float  step = _BlurSize * _BlitTexture_TexelSize.y;

        float3 blurred = 0;
        UNITY_UNROLL
        for (int i = 0; i < TAPS; ++i)
            blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp,
                           uv + float2(0, OFFSETS[i] * step)).rgb * WEIGHTS[i];

        float3 sharp  = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
        float  mask   = FocusMask(uv.y);
        float3 result = lerp(sharp, blurred, mask);
        result = AdjustSaturation(result, _Saturation);

        return float4(result, 1);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "TiltShift_H"
            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment FragHorizontal
            ENDHLSL
        }

        Pass
        {
            Name "TiltShift_V"
            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment FragVertical
            ENDHLSL
        }
    }
}
