Shader "Custom/TrailGlow"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color (HDR)", Color) = (1,1,1,1)
        _EmissionPower ("Emission Power", Range(0, 10)) = 3
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _EmissionColor;
            float _EmissionPower;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.rgb += _EmissionColor.rgb * _EmissionPower;
                return col;
            }

            ENDCG
        }
    }
}
