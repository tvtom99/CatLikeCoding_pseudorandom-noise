Shader "Hash/HashVisualisation"
{
    SubShader
    {
        CGPROGRAM
            #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
            #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
            #pragma editor_sync_compilation
            #pragma target 4.5

            #include "HashVisualisation.hlsl"

            struct Input
            {
                float worldPos;
            };

            void ConfigureSurface (Input input, inout SurfaceOutputStandard surface)
            {
                surface.Albedo = GetHashColour().rgb;
            }
        ENDCG
    }

    FallBack "Diffuse"
}
