using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Visualisation;

public static partial class Noise
{
    public interface IGradient
    {
        float4 Evaluate(SmallXXHash4 hash, float4 x);

        float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y);

        float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z);
    }
    
    public struct Value : IGradient
    {
        public float4 Evaluate(SmallXXHash4 hash, float4 x) => hash.Floats01A * 2f - 1f;
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => hash.Floats01A * 2f - 1f;
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => hash.Floats01A * 2f - 1f;
    }

    public struct Perlin : IGradient
    {
        public float4 Evaluate(SmallXXHash4 hash, float4 x) => 
            (1f + hash.Floats01A) * select(-x, x, ((uint4)hash & 1 << 8) == 0);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => 0f;
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => 0f;
    }
}