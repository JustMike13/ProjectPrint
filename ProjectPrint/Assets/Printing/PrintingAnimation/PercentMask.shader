Shader "Custom/PartialStencilMask"
{
    Properties
    {
        [IntRange] _StencilID ("Stencil ID", Range(0, 255)) = 0
        _Color ("Color", Color) = (1,1,1,1)
        _CutoffPercent ("Cutoff Percent", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Name "StencilMask"
            Stencil
            {
                Ref [_StencilID]
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float _CutoffPercent;
            float4 _BoundsMin;
            float4 _BoundsMax;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float height = _BoundsMax.y - _BoundsMin.y;
                float cutoffY = _BoundsMin.y + height * _CutoffPercent;
                clip(i.worldPos.y < cutoffY ? 1 : -1); // Only write stencil below cutoff
                return fixed4(0,0,0,0); // invisible
            }
            ENDCG
        }

        // Pass
        // {
        //     Name "RenderMasked"
        //     Stencil
        //     {
        //         Ref 1
        //         Comp Equal
        //         Pass Keep
        //     }

        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag
        //     #include "UnityCG.cginc"

        //     struct appdata
        //     {
        //         float4 vertex : POSITION;
        //     };

        //     struct v2f
        //     {
        //         float4 pos : SV_POSITION;
        //     };

        //     fixed4 _Color;

        //     v2f vert (appdata v)
        //     {
        //         v2f o;
        //         o.pos = UnityObjectToClipPos(v.vertex);
        //         return o;
        //     }

        //     fixed4 frag (v2f i) : SV_Target
        //     {
        //         return _Color;
        //     }
        //     ENDCG
        // }
    }
}
