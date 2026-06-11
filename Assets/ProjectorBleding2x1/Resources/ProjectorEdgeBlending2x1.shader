Shader "Hidden/Custom/ProjectorEdgeBlending2x1"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

    float _OutputWidthPixels;
    float _ProjectorWidthPixels;
    float _BlendWidthPixels;
    float _BlendGamma;
    float _DebugBlend;
    float _DebugUV;

    float _Intensity;

    float Ease(float t)
    {
        t = saturate(t);
        return t * t * (3.0 - 2.0 * t);
    }

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float2 outputUV = i.texcoord;

        float outputWidth    = max(_OutputWidthPixels, 1.0);
        float projectorWidth = max(_ProjectorWidthPixels, 1.0);
        float blendWidth     = clamp(_BlendWidthPixels, 1.0, projectorWidth - 1.0);
        float halfBlend      = blendWidth * 0.5;

        float outputX = outputUV.x * outputWidth;

        bool isLeftProjector = outputX < projectorWidth;

        // --------------------------------------------------
        // 1. 左右を均等にずらしてsourceXを作る
        // --------------------------------------------------

        float sourceX;

        if (isLeftProjector)
        {
            // 左画面は halfBlend 分だけ右側の映像を読む
            sourceX = outputX + halfBlend;
        }
        else
        {
            // 右画面は halfBlend 分だけ左側の映像を読む
            sourceX = outputX - halfBlend;
        }

        sourceX = clamp(sourceX, 0.0, outputWidth - 1.0);

        // --------------------------------------------------
        // 2. ブレンドマスクを sourceX 基準で作る
        // --------------------------------------------------
        //
        // BlendWidth = 240 の場合:
        //
        // overlapStart = 1920 - 120 = 1800
        // overlapEnd   = 1920 + 120 = 2040
        //
        // sourceX 1800:
        //   左 100%
        //   右 0%
        //
        // sourceX 1920:
        //   左 50%
        //   右 50%
        //
        // sourceX 2040:
        //   左 0%
        //   右 100%

        float overlapStart = projectorWidth - halfBlend;
        float overlapEnd = projectorWidth + halfBlend;

        float mask = 1.0;

        if (sourceX >= overlapStart && sourceX <= overlapEnd)
        {
            float t = (sourceX - overlapStart) / blendWidth;
            t = Ease(t);

            if (isLeftProjector)
            {
                mask = 1.0 - t;
            }
            else
            {
                mask = t;
            }
        }
        else
        {
            // 重なり範囲外
            mask = 1.0;
        }

        mask = pow(saturate(mask), _BlendGamma);

        float sourceU = sourceX / outputWidth;
        float2 sourceUV = float2(sourceU, outputUV.y);

        half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sourceUV);

        if (_DebugBlend > 0.5)
        {
            return half4(mask, mask, mask, 1.0);
        }

        if (_DebugUV > 0.5)
        {
            return half4(sourceUV.x, sourceUV.y, 0.0, 1.0);
        }

        col.rgb *= mask;
        col.a = 1.0;

        return col;
    }

    ENDHLSL

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment Frag

            ENDHLSL
        }
    }
}