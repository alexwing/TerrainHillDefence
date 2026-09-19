Shader "Custom/StochasticTerrain"
{
    Properties
    {
        [HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        
        [HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
        [HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
        
        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "Queue" = "Geometry-100"
        }
        
        CGPROGRAM
        #pragma surface surf Standard vertex:vert fullforwardshadows
        #pragma target 3.0

        sampler2D _Control;
        sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
        sampler2D _Normal0, _Normal1, _Normal2, _Normal3;

        struct Input
        {
            float2 uv_Control;
            float2 uv_Splat0;
            float2 uv_Splat1;
            float2 uv_Splat2;
            float2 uv_Splat3;
        };

        void vert (inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.uv_Control = v.texcoord.xy;
            data.uv_Splat0 = v.texcoord.xy * 15.0; // Fallback, usually overridden by script
            data.uv_Splat1 = v.texcoord.xy * 15.0;
            data.uv_Splat2 = v.texcoord.xy * 15.0;
            data.uv_Splat3 = v.texcoord.xy * 15.0;
        }

        // --- STOCHASTIC SAMPLING ---
        // Generates pseudo-random offset
        float2 hash2D2D(float2 p)
        {
            return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
        }

        // Blends 3 samples on a triangular grid to eliminate tiling
        void tex2DStochastic(sampler2D tex, sampler2D normTex, float2 uv, out float4 outAlbedo, out float3 outNormal)
        {
            float2x2 m = float2x2(1.0, 0.0, 0.5, 0.8660254);
            
            float2 p = mul(m, uv * 0.5); // scale down grid to make patches larger
            float2 p1 = floor(p);
            float2 f = frac(p);
            
            float2 p2 = p1 + float2(1, 0);
            float2 p3 = p1 + float2(0, 1);
            float2 p4 = p1 + float2(1, 1);
            
            float2 i1 = p1;
            float2 i2 = p2;
            float2 i3 = p3;
            
            if (f.x + f.y >= 1.0) {
                i1 = p4; i2 = p3; i3 = p2;
            } else {
                i2 = p2; i3 = p3;
            }
            
            float2 d1 = f - (i1 - p1);
            float2 d2 = f - (i2 - p1);
            float2 d3 = f - (i3 - p1);
            
            float w1 = max(0.0, 0.5 - dot(d1, d1));
            float w2 = max(0.0, 0.5 - dot(d2, d2));
            float w3 = max(0.0, 0.5 - dot(d3, d3));
            
            float w31 = w1*w1*w1;
            float w32 = w2*w2*w2;
            float w33 = w3*w3*w3;
            
            float norm = 1.0 / (w31 + w32 + w33);
            w1 = w31 * norm;
            w2 = w32 * norm;
            w3 = w33 * norm;
            
            float2 off1 = hash2D2D(i1);
            float2 off2 = hash2D2D(i2);
            float2 off3 = hash2D2D(i3);
            
            // Apply a random rotation to each sample
            float a1 = off1.x * 6.2831853; float s1 = sin(a1); float c1 = cos(a1);
            float a2 = off2.x * 6.2831853; float s2 = sin(a2); float c2 = cos(a2);
            float a3 = off3.x * 6.2831853; float s3 = sin(a3); float c3 = cos(a3);
            
            float2x2 rot1 = float2x2(c1, -s1, s1, c1);
            float2x2 rot2 = float2x2(c2, -s2, s2, c2);
            float2x2 rot3 = float2x2(c3, -s3, s3, c3);
            
            // Albedo
            float4 c_1 = tex2D(tex, mul(rot1, uv) + off1);
            float4 c_2 = tex2D(tex, mul(rot2, uv) + off2);
            float4 c_3 = tex2D(tex, mul(rot3, uv) + off3);
            
            outAlbedo = c_1 * w1 + c_2 * w2 + c_3 * w3;
            
            // Normals
            float4 n_1 = tex2D(normTex, mul(rot1, uv) + off1);
            float4 n_2 = tex2D(normTex, mul(rot2, uv) + off2);
            float4 n_3 = tex2D(normTex, mul(rot3, uv) + off3);
            
            // Unpack and rotate normals back
            float3 norm1 = UnpackNormal(n_1);
            float3 norm2 = UnpackNormal(n_2);
            float3 norm3 = UnpackNormal(n_3);
            
            norm1.xy = mul(float2x2(c1, s1, -s1, c1), norm1.xy);
            norm2.xy = mul(float2x2(c2, s2, -s2, c2), norm2.xy);
            norm3.xy = mul(float2x2(c3, s3, -s3, c3), norm3.xy);
            
            outNormal = normalize(norm1 * w1 + norm2 * w2 + norm3 * w3);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half4 splat_control = tex2D(_Control, IN.uv_Control);
            
            float4 col0, col1, col2, col3;
            float3 norm0, norm1, norm2, norm3;
            
            tex2DStochastic(_Splat0, _Normal0, IN.uv_Splat0, col0, norm0);
            tex2DStochastic(_Splat1, _Normal1, IN.uv_Splat1, col1, norm1);
            tex2DStochastic(_Splat2, _Normal2, IN.uv_Splat2, col2, norm2);
            tex2DStochastic(_Splat3, _Normal3, IN.uv_Splat3, col3, norm3);

            half4 finalColor = 
                splat_control.r * col0 +
                splat_control.g * col1 +
                splat_control.b * col2 +
                splat_control.a * col3;
                
            float3 finalNormal = 
                splat_control.r * norm0 +
                splat_control.g * norm1 +
                splat_control.b * norm2 +
                splat_control.a * norm3;
                
            o.Albedo = finalColor.rgb;
            o.Normal = normalize(finalNormal);
            o.Alpha = 1.0;
            o.Smoothness = 0.1;
            o.Metallic = 0.0;
        }
        ENDCG
    }
    Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Standard-AddPass"
    Dependency "BaseMapShader" = "Hidden/TerrainEngine/Splatmap/Standard-BaseMap"
    Fallback "Nature/Terrain/Standard"
}
