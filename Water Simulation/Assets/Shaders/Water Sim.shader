Shader "Unlit/Water Sim"
{
     Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseWaterColor("Color of the water", color) = (1,1,1)
        _Ambient("Ambient Lighting", color) = (1,1,1)
        _Shininess("Gloss", float) = 1.0
        _Reflectance("Reflectance", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            static const float e = 2.718281828459;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float height : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _BaseWaterColor;
            float3 _Ambient;
            float _Shininess;
            float _Reflectance;

            struct waveData
            {
                float2 dir;
                float freq;
                float amp;
                float speed;
                float sharpness;
            };
            uniform StructuredBuffer<waveData> _Waves;
            int _NumberOfWaves;

            float GetHeight(waveData wave, float2 xz, float t)
            {
                float description = dot(xz, wave.dir)*wave.freq + t*wave.speed;
                float sinWave = sin(description); 
                return wave.amp*pow(e, sinWave-wave.sharpness);
            }

            float GetSurfaceHeight(float3 pos)
            {
                float finalHeight = 0;
                for(int iter = 0; iter<_NumberOfWaves; iter++)
                {
                    finalHeight += GetHeight(_Waves[iter], pos.xz, _Time.y);
                }
                return finalHeight;
            }

            float GetWaveBinormal(waveData wave, float2 xz, float t, float height)
            {
                float description = dot(xz, wave.dir)*wave.freq + t*wave.speed;
                float cosWave = cos(description);
                float yFactor = wave.amp * wave.dir.x * wave.freq * cosWave * height;
                return yFactor;
            }

            float GetWaveTangent(waveData wave, float2 xz, float t, float height)
            {
                float description = dot(xz, wave.dir)*wave.freq + t*wave.speed;
                float cosWave = cos(description);
                float yFactor = wave.amp * wave.dir.y * wave.freq * cosWave * height;
                return yFactor;
            }

            float3 GetSurfaceNormal(float3 pos, float height)
            {
                float finalBinormal = 0;
                float finalTangent = 0;

                for(int iter = 0; iter<_NumberOfWaves; iter++)
                {
                    finalBinormal += GetWaveBinormal(_Waves[iter], pos.xz, _Time.y, height);
                    finalTangent += GetWaveTangent(_Waves[iter], pos.xz, _Time.y, height);
                }
                return normalize(float3(finalBinormal, -1, finalTangent));
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos;

                float finalHeight = GetSurfaceHeight(worldPos);
                v.vertex.y += finalHeight;
                o.height = finalHeight;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {   
                fixed4 col = 1.0;
                float3 normal = GetSurfaceNormal(i.worldPos, i.height);
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float NdotL = saturate(dot(lightDir, -normal));
                float3 camPos = _WorldSpaceCameraPos;
                float3 camView = normalize(i.worldPos - camPos);
                float3 viewReflect = normalize(camView - lightDir);

                float specularFalloff = saturate(dot(viewReflect, normal));
                specularFalloff = pow(specularFalloff, _Shininess)*_Reflectance;

                col.rgb = NdotL*_LightColor0.rgb + _Ambient + _BaseWaterColor;
                col.rgb += NdotL*specularFalloff*_LightColor0.rgb;
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDHLSL
        }
    }
}
