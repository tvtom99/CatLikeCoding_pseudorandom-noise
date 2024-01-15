#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    StructuredBuffer<uint> _Hashes;
    StructuredBuffer<float3> _Positions;
#endif

float4 _Config;

void ConfigureProcedural()
{
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        unity_ObjectToWorld = 0.0;
        unity_ObjectToWorld._m03_m13_m23_m33 = float4(
            _Positions[unity_InstanceID],
            1.0
        );

        unity_ObjectToWorld._m13 += _Config.z * ((1.0 / 255.0) * (_Hashes[unity_InstanceID] >> 24 ) - 0.5);
    #endif
}

float3 GetHashColour()
{
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        uint hash = _Hashes[unity_InstanceID];
        return (1.0 / 255.0) * float3(
            hash & 255,
            (hash >> 8) & 255,
            (hash >>16) & 255
        );
    #else
        return 1.0;
    #endif
}