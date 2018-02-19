Shader "Custom/BasicVoxelShader" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _Texture1("Texture 1 (RGB)", 2D) = "white" {}
        _Texture2("Texture 2 (RGB)", 2D) = "white" {}
        _Texture3("Texture 3 (RGB)", 2D) = "white" {}

        _Texture1Height("Texture 1 Height (R)", 2D) = "black" {}
        _Texture2Height("Texture 2 Height (R)", 2D) = "black" {}
        _Texture3Height("Texture 3 Height (R)", 2D) = "black" {}

        _Texture1Normal("Texture 1 Normal (RGB)", 2D) = "black" {}
        _Texture2Normal("Texture 2 Normal (RGB)", 2D) = "black" {}
        _Texture3Normal("Texture 3 Normal (RGB)", 2D) = "black" {}

        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Scale("Scale", Float) = 4
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            sampler2D _Texture1;
            sampler2D _Texture2;
            sampler2D _Texture3;

            sampler2D _Texture1Height;
            sampler2D _Texture2Height;
            sampler2D _Texture3Height;

            sampler2D _Texture1Normal;
            sampler2D _Texture2Normal;
            sampler2D _Texture3Normal;

            struct Input {
                float2 uv_Texture1;
                float3 color : COLOR;
            };

            half _Glossiness;
            half _Metallic;
            fixed4 _Color;
            half _Scale;

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling

            UNITY_INSTANCING_CBUFFER_START(Props)
                // put more per-instance properties here
                UNITY_INSTANCING_CBUFFER_END


            void surf(Input IN, inout SurfaceOutputStandard o) {

                fixed height1 = tex2D(_Texture1Height, IN.uv_Texture1).r;
                fixed height2 = tex2D(_Texture2Height, IN.uv_Texture1).r;
                fixed height3 = tex2D(_Texture3Height, IN.uv_Texture1).r;

                float3 color = float3(IN.color.r + height1, IN.color.g + height2, IN.color.b + height3);

                // Albedo comes from a texture tinted by color
                fixed4 tex1 = tex2D(_Texture1, IN.uv_Texture1) * _Color;
                fixed4 tex2 = tex2D(_Texture2, IN.uv_Texture1) * _Color;
                fixed4 tex3 = tex2D(_Texture3, IN.uv_Texture1) * _Color;

                fixed3 normal1 = UnpackNormal(tex2D(_Texture1Normal, IN.uv_Texture1));
                fixed3 normal2 = UnpackNormal(tex2D(_Texture2Normal, IN.uv_Texture1));
                fixed3 normal3 = UnpackNormal(tex2D(_Texture3Normal, IN.uv_Texture1));

                float t1 = (1 / (1 * pow(2, color.r * _Scale)) + 1) / 2;
                float t2 = (1 / (1 * pow(2, color.g * _Scale)) + 1) / 2;
                float t3 = (1 / (1 * pow(2, color.b * _Scale)) + 1) / 2;

                float tSum = t1 + t2 + t3;
                t1 /= tSum;
                t2 /= tSum;
                t3 /= tSum;
                tex1 *= t1;
                tex2 *= t2;
                tex3 *= t3;

                o.Albedo = (tex1 + tex2 + tex3);

                // Metallic and smoothness come from slider variables
                o.Normal = (normal1 * t1 + normal2 * t2 + normal3 * t3);
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Occlusion = 1;
                o.Alpha = 1;
            }
            ENDCG
        }
            FallBack "Diffuse"
}
