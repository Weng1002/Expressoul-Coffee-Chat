Shader "Unlit/FrostedGlass"
{
    Properties { _Radius("Radius", Range(1, 255)) = 4 }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque" }
        GrabPass { Tags { "LightMode"="Always" } }
        Pass
        {
            Tags { "LightMode"="Always" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata_t { float4 vertex : POSITION; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float4 uvgrab : TEXCOORD0; };
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                #if UNITY_UV_STARTS_AT_TOP
                    float scale = -1;
                #else
                    float scale = 1;
                #endif
                o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                o.uvgrab.zw = o.vertex.zw;
                return o;
            }
            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;
            int _Radius;
            half4 frag(v2f i) : SV_Target
            {
                half4 sum = half4(0,0,0,0);
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
                int count = 1;
                for (float r = 1; r <= _Radius; r++)
                {
                    sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab + float4(_GrabTexture_TexelSize.xy * r,0,0)));
                    sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab - float4(_GrabTexture_TexelSize.xy * r,0,0)));
                    sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab + float4(0,_GrabTexture_TexelSize.y * r,0,0)));
                    sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab - float4(0,_GrabTexture_TexelSize.y * r,0,0)));
                    count += 4;
                }
                return sum / count;
            }
            ENDCG
        }
    }
}
