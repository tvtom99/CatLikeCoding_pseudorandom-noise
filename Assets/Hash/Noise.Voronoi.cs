using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Visualisation;

public static partial class Noise
{
    public struct Voronoi1D<L, F> : INoise where L : struct, ILattice where F : struct, IVoronoiFunction
    {
        public float4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            L l = default(L);
            LatticeSpan4 x = default(L).GetLatticeSpan4(positions.c0, frequency);

            float4x2 minima = 2f;
            for(int i = -1; i <= 1; i++)
            {
                SmallXXHash4 h = hash.Eat(l.ValidateSingleStep(x.p0 + i, frequency));
                minima = UpdateVoronoiMinima(minima, abs(h.Floats01A + i - x.g0));
            }

            return default(F).Evaluate(minima);
        }
    }

    public struct Voronoi2D<L, F> : INoise where L : struct, ILattice where F : struct, IVoronoiFunction
    {

        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            LatticeSpan4
                x = l.GetLatticeSpan4(positions.c0, frequency),
                z = l.GetLatticeSpan4(positions.c2, frequency);

            float4x2 minima = 2f;
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

            //Clamp the values
            minima.c0 = min(minima.c0, 1f);
            minima.c1 = min(minima.c1, 1f);

            return default(F).Evaluate(minima);
        }
    }

    public struct Voronoi3D<L, F> : INoise where L : struct, ILattice where F : struct, IVoronoiFunction
    {

        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            LatticeSpan4
                x = l.GetLatticeSpan4(positions.c0, frequency),
                y = l.GetLatticeSpan4(positions.c1, frequency),
                z = l.GetLatticeSpan4(positions.c2, frequency);

            float4x2 minima = 2f;
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

            //Clamp the values on minima
            minima.c0 = min(minima.c0, 1f);
            minima.c1 = min(minima.c1, 1f);

            return default(F).Evaluate(minima);
        }
    }

    static float4x2 UpdateVoronoiMinima(float4x2 minima, float4 distances)
    {
        bool4 newMinimum = distances < minima.c0;
        minima.c1 = select(
            select(minima.c1, distances, distances < minima.c1),
            minima.c0, 
            newMinimum
        );
        minima.c0 = select(minima.c0, distances, newMinimum);
        return minima;
    }

    static float4 GetDistance(float4 x, float4 y) => sqrt(x * x + y * y);

    static float4 GetDistance(float4 x, float4 y, float4 z) => sqrt(x * x + y * y + z * z);
}