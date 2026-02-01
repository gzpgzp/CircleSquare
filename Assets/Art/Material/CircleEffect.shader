Shader "Custom/CircleEffect_SpriteCompatible"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Circle Settings)]
        _CircleCenter ("Circle Center (World)", Vector) = (0,0,0,0)
        _CircleRadius ("Circle Radius", Float) = 1
        _EdgeWidth ("Edge Width", Float) = 0.1

        [Header(Color Settings)]
        _EdgeColor ("Edge Color", Color) = (1,0,0,1)
        _InnerColor ("Inner Color", Color) = (0,0,0,1)
        _OutsideColor ("Outside Color", Color) = (1,1,1,0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // SpriteRenderer Tint
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float2 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;

            float2 _CircleCenter;
            float _CircleRadius;
            float _EdgeWidth;

            float4 _EdgeColor;
            float4 _InnerColor;
            float4 _OutsideColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sprite texture
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Distance to circle center (world space)
                float dist = distance(i.worldPos, _CircleCenter);

                float edgeStart = _CircleRadius - _EdgeWidth;

                float edgeBlend = smoothstep(edgeStart, _CircleRadius, dist);
                float outsideBlend = step(_CircleRadius, dist);

                fixed4 circleColor = lerp(_InnerColor, _EdgeColor, edgeBlend);
                circleColor = lerp(circleColor, _OutsideColor, outsideBlend);

                // Final color = SpriteDefault pipeline
                fixed4 finalColor = texColor * circleColor * i.color;

                return finalColor;
            }
            ENDCG
        }
    }
}