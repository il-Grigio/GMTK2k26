Shader "Custom/ToonShader4Level"
{
    Properties
    {
        [Header(Base)]
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Tint", Color) = (1,1,1,1)

        [Header(4 Livelli di Luce dal piu chiaro al piu scuro)]
        _Color1 ("Livello 1 - Luce piena", Color) = (1.0, 1.0, 1.0, 1)
        _Color2 ("Livello 2 - Luce media", Color) = (0.75, 0.75, 0.8, 1)
        _Color3 ("Livello 3 - Ombra", Color) = (0.45, 0.45, 0.55, 1)
        _Color4 ("Livello 4 - Ombra profonda", Color) = (0.2, 0.2, 0.3, 1)

        [Header(Soglie bande da 0 a 1)]
        _Threshold1 ("Soglia 1-2", Range(0,1)) = 0.75
        _Threshold2 ("Soglia 2-3", Range(0,1)) = 0.5
        _Threshold3 ("Soglia 3-4", Range(0,1)) = 0.25   
        _BandSmooth ("Morbidezza bordo banda", Range(0.0, 0.2)) = 0.01

        [Header(Outline)]
        _OutlineColor ("Colore Outline", Color) = (0,0,0,1)
        _OutlineWidth ("Spessore Outline", Range(0.0, 0.1)) = 0.02

        [Header(Specular opzionale)]
        _SpecularColor ("Colore Specular", Color) = (1,1,1,1)
        _SpecularSize ("Dimensione Specular", Range(0.0, 1.0)) = 0.85
        _SpecularSmooth ("Morbidezza Specular", Range(0.001, 0.5)) = 0.05
        [Toggle] _UseSpecular ("Attiva Specular", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry" }

        // ------------------------------------------------------------
        // PASS 1: OUTLINE
        // Disegna il retro del modello leggermente espanso lungo le
        // normali, con culling delle facce frontali, cosi il bordo
        // espanso rimane visibile dietro al modello normale.
        // ------------------------------------------------------------
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings OutlineVert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs vni = GetVertexNormalInputs(IN.normalOS);

                // Espande il vertice lungo la normale in world space
                float3 expandedPosWS = vpi.positionWS + vni.normalWS * _OutlineWidth;
                OUT.positionHCS = TransformWorldToHClip(expandedPosWS);
                return OUT;
            }

            half4 OutlineFrag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // ------------------------------------------------------------
        // PASS 2: FORWARD LIT (toon a 4 bande)
        // ------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex LitVert
            #pragma fragment LitFrag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _Color1;
                float4 _Color2;
                float4 _Color3;
                float4 _Color4;
                float _Threshold1;
                float _Threshold2;
                float _Threshold3;
                float _BandSmooth;
                float4 _SpecularColor;
                float _SpecularSize;
                float _SpecularSmooth;
                float _UseSpecular;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };

            Varyings LitVert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs vni = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = vpi.positionCS;
                OUT.positionWS = vpi.positionWS;
                OUT.normalWS = vni.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.shadowCoord = GetShadowCoord(vpi);
                return OUT;
            }

            // Restituisce il colore della banda in base a un valore di
            // luce compreso tra 0 e 1, con transizioni nette (o
            // leggermente smussate tramite _BandSmooth per evitare aliasing).
            half4 GetBandColor(float lightValue)
            {
                half4 col;
                col = lerp(_Color4, _Color3, smoothstep(_Threshold3 - _BandSmooth, _Threshold3 + _BandSmooth, lightValue));
                col = lerp(col, _Color2, smoothstep(_Threshold2 - _BandSmooth, _Threshold2 + _BandSmooth, lightValue));
                col = lerp(col, _Color1, smoothstep(_Threshold1 - _BandSmooth, _Threshold1 + _BandSmooth, lightValue));
                return col;
            }

            half4 LitFrag(Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(IN.positionWS));

                Light mainLight = GetMainLight(IN.shadowCoord);
                float NdotL = saturate(dot(normalWS, mainLight.direction));

                // L'attenuazione delle ombre (shadow map) viene inclusa
                // nel valore di luce cosi anche le aree in ombra proiettata
                // vengono classificate correttamente nelle 4 bande.
                float lightValue = NdotL * mainLight.shadowAttenuation;

                half4 bandColor = GetBandColor(lightValue);

                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 albedo = baseTex * _BaseColor;

                half3 color = albedo.rgb * bandColor.rgb * mainLight.color;

                // Specular a taglio netto (opzionale)
                if (_UseSpecular > 0.5)
                {
                    float3 halfDir = normalize(mainLight.direction + viewDirWS);
                    float NdotH = saturate(dot(normalWS, halfDir));
                    float spec = smoothstep(_SpecularSize - _SpecularSmooth, _SpecularSize + _SpecularSmooth, NdotH);
                    spec *= step(0.01, NdotL); // niente specular sul lato in ombra
                    color += spec * _SpecularColor.rgb;
                }

                return half4(color, albedo.a);
            }
            ENDHLSL
        }

        // Pass per proiettare/ricevere ombre correttamente
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_shadowcaster
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            // Reimplementazione manuale del bias (equivalente a
            // ApplyShadowBias di URP), per non dipendere da un percorso
            // di include specifico di versione.
            float3 ApplyBiasManual(float3 positionWS, float3 normalWS)
            {
                float invNdotL = 1.0 - saturate(dot(_LightDirection, normalWS));
                float scale = invNdotL * _ShadowBias.y;

                positionWS = _LightDirection * _ShadowBias.xxx + positionWS;
                positionWS = normalWS * scale.xxx + positionWS;
                return positionWS;
            }

            Varyings ShadowVert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                float4 positionCS = TransformWorldToHClip(ApplyBiasManual(positionWS, normalWS));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                OUT.positionHCS = positionCS;
                return OUT;
            }

            half4 ShadowFrag(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
