Shader "Custom/VoxelTerrainShader" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _Layer1Tex("Albedo (R)", 2D) = "red" {}
        _Layer2Tex("Albedo (G)", 2D) = "green" {}
        _Layer3Tex("Albedo (B)", 2D) = "blue" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            sampler2D _Layer1Tex;
            sampler2D _Layer2Tex;
            sampler2D _Layer3Tex;

            struct Input {
                float2 uv_Layer1Tex;
                float4 color : COLOR;
            };

            half _Glossiness;
            half _Metallic;

            void surf(Input IN, inout SurfaceOutputStandard o) {
                // Albedo comes from a texture tinted by color
                fixed4 l1 = tex2D(_Layer1Tex, IN.uv_Layer1Tex);
                fixed4 l2 = tex2D(_Layer2Tex, IN.uv_Layer1Tex);
                fixed4 l3 = tex2D(_Layer3Tex, IN.uv_Layer1Tex);

                fixed3 smoothingValue = normalize(IN.color.rgb);
                fixed3 c = (l1 * smoothingValue.r) + (l2 * smoothingValue.g) + (l3 * smoothingValue.b);

                o.Albedo = c;
                // Metallic and smoothness come from slider variables
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = 1;
            }
            ENDCG
        }
            FallBack "Diffuse"
}