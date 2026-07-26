Shader "Custom/WallHoleStencilWriter"
{
    // Sfera invisibile centrata sul player. Scrive un valore nello Stencil
    // Buffer nell'area che occupa a schermo, e nient'altro (niente colore,
    // niente depth reale). Versione URP (compatibile col progetto che usa
    // ToonShader4Level).
    //
    // ORDINE DI RENDERING: deve disegnarsi PRIMA del muro (Queue piu' bassa),
    // altrimenti lo stencil non e' ancora scritto quando il muro fa il test.
    // Il muro (ToonShader4Level) e' su Queue "Geometry" (2000) di default,
    // quindi questa e' su "Geometry-1" (1999).

    Properties
    {
        _NoiseTex ("Noise (edge pattern)", 2D) = "white" {}
        _EdgeNoiseAmount ("Edge Noise Amount", Range(0,1)) = 0.4
        [IntRange] _StencilRef ("Stencil Ref Value (deve combaciare col muro)", Range(0,255)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry-1" }

        Pass
        {
            Name "StencilWrite"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ColorMask 0     // invisibile: non scrive colore
            ZWrite Off      // non altera la depth reale della scena
            ZTest Always    // marca l'area indipendentemente da chi sarebbe "davanti"
            Cull Off

            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST;
                float _EdgeNoiseAmount;
                float _StencilRef;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 localPos    : TEXCOORD1;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _NoiseTex);
                OUT.localPos = IN.positionOS.xyz; // per il bordo organico (sfera unit-radius)
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Bordo frastagliato/organico invece di un cerchio perfetto
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uv * 2.0).r;
                float distFromCenter = length(IN.localPos); // ~1 al bordo
                float edgeThreshold = 1.0 - _EdgeNoiseAmount * noise;
                clip(edgeThreshold - distFromCenter);
                return 0;
            }
            ENDHLSL
        }
    }
}
