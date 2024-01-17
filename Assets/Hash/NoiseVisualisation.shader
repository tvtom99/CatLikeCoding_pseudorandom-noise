Shader "Hash/NoiseVisualisation"
{
    SubShader
    {
        CGPROGRAM
            #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
            #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
            #pragma editor_sync_compilation
            #pragma target 4.5

            #include "NoiseGPU.hlsl"

            struct Input
            {
                float worldPos;
            };

            void ConfigureSurface (Input input, inout SurfaceOutputStandard surface)
            {
                surface.Albedo = GetNoiseColour().rgb;
            }
        ENDCG
    }

    FallBack "Diffuse"
}
