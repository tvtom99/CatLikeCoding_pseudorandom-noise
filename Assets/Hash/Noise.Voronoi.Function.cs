using Unity.Mathematics;

public static partial class Noise
{
    public interface IVoronoiFunction
    {
        float4 Evaluate(float4x2 minima);
    }

    public struct F1 : IVoronoiFunction //Function that shows the distance to the first nearest point
    {
        public float4 Evaluate(float4x2 distances) => distances.c0;
    }

    public struct F2 : IVoronoiFunction //Function that shows the distance to the second nearest point
    {
        public float4 Evaluate(float4x2 distances) => distances.c1;
    }

    public struct F2MinusF1 : IVoronoiFunction
    {
        public float4 Evaluate(float4x2 distaces) => distaces.c1 - distaces.c0;
    }
}