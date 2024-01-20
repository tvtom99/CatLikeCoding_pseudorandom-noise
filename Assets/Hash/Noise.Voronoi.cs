using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Visualisation;

public static partial class Noise
{
    public struct Voronoi1D<L> : INoise where L : struct, ILattice
    {
        public float4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            L l = default(L);
            LatticeSpan4 x = default(L).GetLatticeSpan4(positions.c0, frequency);

            float4 minima = 2f;
            for(int i = -1; i <= 1; i++)
            {
                SmallXXHash4 h = hash.Eat(l.ValidateSingleStep(x.p0 + i, frequency));
                minima = UpdateVoronoiMinima(minima, abs(h.Floats01A + i - x.g0));
            }

            return minima;
        }
    }

    public struct Voronoi2D<L> : INoise where L : struct, ILattice
    {

        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            LatticeSpan4
                x = l.GetLatticeSpan4(positions.c0, frequency),
                z = l.GetLatticeSpan4(positions.c2, frequency);

            float4 minima = 2f;
            for (int i = -1; i <= 1; i++)
            {
                SmallXXHash4 hx = hash.Eat(l.ValidateSingleStep(x.p0 + i, frequency));
                float4 xOffset = i - x.g0;
                for (int j = -1; j <= 1; j++)
                {
                    SmallXXHash4 h = hx.Eat(l.ValidateSingleStep(z.p0 + j, frequency));
                    float4 zOffset = j - z.g0;
                    minima = UpdateVoronoiMinima(minima, GetDistance(
                        h.Floats01A + xOffset, h.Floats01B + zOffset
                    ));
                    minima = UpdateVoronoiMinima(minima, GetDistance(
                        h.Floats01C + xOffset, h.Floats01D + zOffset
                    ));
                }
            }

            return min(minima, 1f);
        }
    }

    public struct Voronoi3D<L> : INoise where L : struct, ILattice
    {

        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            LatticeSpan4
                x = l.GetLatticeSpan4(positions.c0, frequency),
                y = l.GetLatticeSpan4(positions.c1, frequency),
                z = l.GetLatticeSpan4(positions.c2, frequency);

            float4 minima = 2f;
            for (int u = -1; u <= 1; u++)
            {
                SmallXXHash4 hx = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
                float4 xOffset = u - x.g0;
                for (int v = -1; v <= 1; v++)
                {
                    SmallXXHash4 hy = hx.Eat(l.ValidateSingleStep(y.p0 + v, frequency));
                    float4 yOffset = v - y.g0;
                    for (int w = -1; w <= 1; w++)
                    {
                        SmallXXHash4 h =
                            hy.Eat(l.ValidateSingleStep(z.p0 + w, frequency));
                        float4 zOffset = w - z.g0;
                        minima = UpdateVoronoiMinima(minima, GetDistance(
                            h.GetBitsAsFloats01(5, 0) + xOffset,
                            h.GetBitsAsFloats01(5, 5) + yOffset,
                            h.GetBitsAsFloats01(5, 10) + zOffset
                        ));
                        minima = UpdateVoronoiMinima(minima, GetDistance(
                            h.GetBitsAsFloats01(5, 15) + xOffset,
                            h.GetBitsAsFloats01(5, 20) + yOffset,
                            h.GetBitsAsFloats01(5, 25) + zOffset
                        ));                                                             //Do this twice so that there's enough points to prevent flat regions due to the min clamp on the return statement
                    }
                }
            }
            return min(minima, 1f);
        }
    }

    static float4 UpdateVoronoiMinima(float4 minima, float4 distances)
    {
        return select(minima, distances, distances < minima);
    }

    static float4 GetDistance(float4 x, float4 y) => sqrt(x * x + y * y);

    static float4 GetDistance(float4 x, float4 y, float4 z) => sqrt(x * x + y * y + z * z);
}