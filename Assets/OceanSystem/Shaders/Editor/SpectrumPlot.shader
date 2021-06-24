Shader "Hidden/SpectrumPlot"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../Resources/ComputeShaders/Oceanography.hlsl"

            float width;
            float height;
            float windowAspectRatio;
            float2 leftBottom;
            float3 backgroundColor;
            bool showCascades;
            bool drawSpectrum;
            bool drawRamp;

            // local spectrum params
            float local_scale;
            int local_energySpectrum;
            float local_windSpeed;
            float local_fetch;
            float local_peaking;
            float local_shortWaves;

            // swell spectrum params
            float swell_scale;
            int swell_energySpectrum;
            float swell_windSpeed;
            float swell_fetch;
            float swell_peaking;
            float swell_shortWaves;

            // cascades params
            float4 cutoffsLow;
            float4 cutoffsHigh;
            float3 cascade0Color;
            float3 cascade1Color;
            float3 cascade2Color;
            float3 cascade3Color;

            // ramp graph params
            int equalizerChannel;
            TEXTURE2D(equalizerRamp);
            SAMPLER(sampler_equalizerRamp);
            float3 rampFill;
            float3 rampLine;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float2 Spectrum(float x, int energySpectrum, float windSpeed, float fetch, float peaking, float shortWaves)
            {
                float k = 2 * PI / pow(10, x);
                float omega = Frequency(k, Depth);
                float peakOmega;
                float peak;

                float chi = abs(g * fetch * 1000 / windSpeed / windSpeed);
                chi = min(1e4, chi);

                float2 res = 0;

                if (energySpectrum == 2)
                {
                    peakOmega = JonswapPeakOmega(chi, windSpeed);
                    peak = JONSWAP(peakOmega, peakOmega, chi, peaking) * TMACorrection(peakOmega, Depth);
                    res = float2(JONSWAP(omega, peakOmega, chi, peaking) * TMACorrection(omega, Depth), peak);
                }

                if (energySpectrum == 1)
                {
                    peakOmega = JonswapPeakOmega(chi, windSpeed);
                    peak = JONSWAP(peakOmega, peakOmega, chi, peaking);
                    res = float2(JONSWAP(omega, peakOmega, chi, peaking), peak);
                }

                if (energySpectrum == 0)
                {
                    peakOmega = PiersonMoskowitzPeakOmega(windSpeed);
                    peak = PiersonMoskowitz(peakOmega, peakOmega);
                    res = float2(PiersonMoskowitz(omega, peakOmega), peak);
                }

                return res * ShortWavesFade(k, shortWaves);
            }

            float SpectrumGraph(float x)
            {
                float2 local = Spectrum(x, local_energySpectrum,
                    local_windSpeed, local_fetch, local_peaking, local_shortWaves);
                float2 swell = Spectrum(x, swell_energySpectrum,
                    swell_windSpeed, swell_fetch, swell_peaking, swell_shortWaves);
                float v = max(local.x / local.y, swell_scale * swell.x / swell.y);
                return sqrt(sqrt(v)) * 0.95;
            }

            float2 GridValue(float2 coords)
            {
                float gridWidth = 0.007;
                float aspectRatio = width / height / windowAspectRatio;
                float sdfGrid = min(abs(round(coords.x / 0.5) * 0.5 - coords.x) - gridWidth * aspectRatio,
                    abs(round(coords.y / 0.25) * 0.25 - coords.y) - gridWidth);
                sdfGrid /= gridWidth;

                float sdfBorder = 0;
                sdfBorder -= coords.x < (leftBottom.x + gridWidth * 2 * aspectRatio);
                sdfBorder -= coords.x > (leftBottom.x + width - gridWidth * 2 * aspectRatio);
                sdfBorder -= coords.y < (leftBottom.y + gridWidth * 2);
                sdfBorder -= coords.y > (leftBottom.y + height - gridWidth * 2);
                sdfBorder /= gridWidth;

                return float2(-sdfGrid, -sdfBorder);
            }

            float3 DrawSpectrum(float3 col, float3 spectrumColor, float2 coords)
            {
                float sdfSpectrum = SpectrumGraph(coords.x) - coords.y;
                sdfSpectrum /= fwidth(sdfSpectrum);
                float belowSpectrum = saturate(sdfSpectrum) * 0.7;
                return lerp(col, spectrumColor, belowSpectrum);
            }

            float3 CascadedSpectrumColor(float2 coords)
            {
                float4 cutoffValuesHigh = log10(2 * PI / cutoffsLow);
                float4 cutoffValuesLow = log10(2 * PI / cutoffsHigh);
                float cascade0 = (coords.x < cutoffValuesHigh.x) * (coords.x > cutoffValuesLow.x);
                float cascade1 = (coords.x < cutoffValuesHigh.y) * (coords.x > cutoffValuesLow.y);
                float cascade2 = (coords.x < cutoffValuesHigh.z) * (coords.x > cutoffValuesLow.z);
                float cascade3 = (coords.x < cutoffValuesHigh.w) * (coords.x > cutoffValuesLow.w);

                float3 functionColor = cascade0 * cascade0Color;
                functionColor += cascade1 * cascade1Color;
                functionColor += cascade2 * cascade2Color;
                functionColor += cascade3 * cascade3Color;

                return functionColor;
            }

            float2 RampSdf(float2 uv)
            {
                float eq = SAMPLE_TEXTURE2D(equalizerRamp, sampler_equalizerRamp, uv)[equalizerChannel] * 0.5;
                float sgn = sign(eq - 0.5);
                float sdfEqualizerFill;
                sdfEqualizerFill = (sgn * uv.y > sgn * 0.5) * sgn * (eq - uv.y);
                sdfEqualizerFill /= fwidth(sdfEqualizerFill);

                float eqDer = (SAMPLE_TEXTURE2D(equalizerRamp, sampler_equalizerRamp, uv + float2(0.005, 0))[equalizerChannel] * 0.5 - eq) / 0.005 / windowAspectRatio;
                float sdfEqualizerLine = -abs(eq - uv.y) + 0.012 * sqrt(1 + eqDer * eqDer);
                sdfEqualizerLine /= fwidth(sdfEqualizerLine);

                return float2(sdfEqualizerFill, sdfEqualizerLine);
            }

            float3 DrawRamp(float3 col, float2 uv)
            {
                float2 sdfRamp = RampSdf(uv);
                col = lerp(col, rampFill, saturate(sdfRamp.x) * 0.6);
                return lerp(col, rampLine, saturate(sdfRamp.y));
            }

            float4 Frag(Varyings IN) : SV_Target{
                float2 coords = IN.uv * float2(width, height) + leftBottom;
                float2 sdfGrid = GridValue(coords);
                float3 gridColor = 0.52;

                float3 spectrumColor;
                if (showCascades)
                    spectrumColor = CascadedSpectrumColor(coords);
                else if (drawRamp)
                    spectrumColor = 0.4;
                else
                 spectrumColor = float3(129, 180, 254) / 255;

                // Background
                float3 col = backgroundColor;
                // Draw grid
                col = lerp(col, gridColor, saturate(sdfGrid.x));

                if (drawSpectrum)
                {
                    col = DrawSpectrum(col, spectrumColor, coords);
                }

                if (drawRamp)
                    col = DrawRamp(col, IN.uv);

                // Draw border
                col = lerp(col, gridColor, saturate(sdfGrid.y));

                return float4(col, 1);
            }

        ENDHLSL

        Pass
        {
            Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
