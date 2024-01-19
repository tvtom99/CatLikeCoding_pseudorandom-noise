using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Visualisation;

public static partial class Noise
{
    public struct Voronoi1D<L> : INoise where L : struct, ILattice
    {
        public float4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            LatticeSpan4 x = default(L).GetLatticeSpan4(positions.c0, frequency);

            return 0f;
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

            return 0f;
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

            return 0f;
        }
    }
}