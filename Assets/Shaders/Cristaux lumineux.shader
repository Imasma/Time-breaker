Shader "Custom/CrystalProximityGlow"
{
    Properties
    {
        // === Textures ===
        _MainTex        ("Albedo (RGB)",        2D)     = "white" {}
        _NoiseTex       ("Noise Texture",       2D)     = "white" {}
        _NormalMap      ("Normal Map",          2D)     = "bump"  {}

        // === Couleurs ===
        _BaseColor      ("Base Color",          Color)  = (0.6, 0.85, 1.0, 1.0)
        _GlowColor      ("Glow Color",          Color)  = (0.3, 0.9,  1.0, 1.0)
        _FresnelColor   ("Fresnel Color",       Color)  = (0.8, 0.95, 1.0, 1.0)

        // === Proximité ===
        _GlowRadius     ("Glow Radius",         Float)  = 5.0
        _GlowSharpness  ("Glow Sharpness",      Float)  = 2.0
        _EmissionMax    ("Emission Max",        Float)  = 3.0

        // === Pulsation ===
        _PulseSpeed     ("Pulse Speed",         Float)  = 3.0
        _PulseIntensity ("Pulse Intensity",     Range(0,1)) = 0.3

        // === Fresnel ===
        _FresnelPower   ("Fresnel Power",       Float)  = 3.0
        _FresnelStrength("Fresnel Strength",    Float)  = 1.5

        // === Distorsion interne ===
        _NoiseScrollX   ("Noise Scroll X",      Float)  = 0.05
        _NoiseScrollY   ("Noise Scroll Y",      Float)  = 0.03
        _NoiseStrength  ("Noise Strength",      Range(0, 0.3)) = 0.08
        _NoiseScale     ("Noise Scale",         Float)  = 1.0

        // === Transparence ===
        _Alpha          ("Base Alpha",          Range(0,1)) = 0.85
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue"      = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "CrystalGlowPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.0

            // ─── Includes URP ────────────────────────────────────────────
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ─── Variables globales (envoyées par C#) ────────────────────
            float4 _PlayerPos;          // position world du joueur
            float  _PlayerProximity;    // 0..1 calculé côté CPU (optionnel)

            // ─── Propriétés du matériau ──────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _GlowColor;
                float4 _FresnelColor;
                float  _GlowRadius;
                float  _GlowSharpness;
                float  _EmissionMax;
                float  _PulseSpeed;
                float  _PulseIntensity;
                float  _FresnelPower;
                float  _FresnelStrength;
                float  _NoiseScrollX;
                float  _NoiseScrollY;
                float  _NoiseStrength;
                float  _NoiseScale;
                float  _Alpha;
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
            CBUFFER_END

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);   SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_NormalMap);  SAMPLER(sampler_NormalMap);

            // ─── Structures ───────────────────────────────────────────────
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 worldPos     : TEXCOORD1;
                float3 worldNormal  : TEXCOORD2;
                float3 viewDir      : TEXCOORD3;
                float  fogFactor    : TEXCOORD4;
            };

            // ─── Vertex Shader ────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs    = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.worldPos    = posInputs.positionWS;
                OUT.worldNormal = normalInputs.normalWS;
                OUT.viewDir     = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.fogFactor   = ComputeFogFactor(posInputs.positionCS.z);

                return OUT;
            }

            // ─── Fragment Shader ──────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                // ── 1. PROXIMITÉ ─────────────────────────────────────────
                float dist      = distance(IN.worldPos, _PlayerPos.xyz);
                float proximity = 1.0 - saturate(dist / _GlowRadius);
                proximity       = pow(proximity, _GlowSharpness); // falloff exponentielle

                // ── 2. PULSATION (breathing glow) ────────────────────────
                // sin oscillant entre 0 et 1, modulé uniquement quand proche
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float animatedProximity = proximity * (1.0 - _PulseIntensity + pulse * _PulseIntensity);

                // ── 3. DISTORSION INTERNE (noise scroll) ─────────────────
                float2 noiseUV   = IN.uv * _NoiseScale
                                 + float2(_NoiseScrollX, _NoiseScrollY) * _Time.y;
                float  noiseSample = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

                // Perturber les UVs de la texture principale
                float2 distortedUV = IN.uv + (noiseSample - 0.5) * _NoiseStrength * animatedProximity;

                // ── 4. ALBEDO ─────────────────────────────────────────────
                half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);
                half4 col    = lerp(_BaseColor, _GlowColor, animatedProximity) * texCol;

                // ── 5. FRESNEL (brillance sur les bords) ─────────────────
                float3 normalWS  = normalize(IN.worldNormal);
                float3 viewDirWS = normalize(IN.viewDir);
                float  NdotV     = saturate(dot(normalWS, viewDirWS));
                float  fresnel   = pow(1.0 - NdotV, _FresnelPower);

                // Le fresnel est amplifié par la proximité
                col.rgb += _FresnelColor.rgb * fresnel * _FresnelStrength * (0.2 + animatedProximity * 0.8);

                // ── 6. EMISSION HDR (nécessite Bloom en post-process) ─────
                // Emission de base faible + forte quand proche
                float baseEmission  = 0.1;
                float glowEmission  = animatedProximity * _EmissionMax;
                col.rgb += _GlowColor.rgb * (baseEmission + glowEmission);

                // Boost supplémentaire sur le contour (fresnel + emission)
                col.rgb += _FresnelColor.rgb * fresnel * animatedProximity * (_EmissionMax * 0.5);

                // ── 7. ALPHA ──────────────────────────────────────────────
                // Plus brillant = légèrement plus opaque
                col.a = texCol.a * _Alpha + animatedProximity * (1.0 - _Alpha) * 0.5;

                // ── 8. FOG ────────────────────────────────────────────────
                col.rgb = MixFog(col.rgb, IN.fogFactor);

                return col;
            }

            ENDHLSL
        }

        // Shadow caster pass (nécessaire pour que l'objet projette des ombres)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex   ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    // Fallback pour Built-in RP (Unity legacy)
    Fallback "Transparent/Diffuse"
}
