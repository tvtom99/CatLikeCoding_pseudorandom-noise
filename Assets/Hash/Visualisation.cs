using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public abstract class Visualisation : MonoBehaviour
{
    public readonly struct SmallXXHash
    {
        const uint primeA = 0b10011110001101110111100110110001;
        const uint primeB = 0b10000101111010111100101001110111;
        const uint primeC = 0b11000010101100101010111000111101;
        const uint primeD = 0b00100111110101001110101100101111;
        const uint primeE = 0b00010110010101100110011110110001;

        readonly uint accumulator;

        public SmallXXHash(uint accumulator)
        {
            this.accumulator = accumulator;
        }

        public static SmallXXHash Seed(int seed) => (uint)seed + primeE;

        public static implicit operator SmallXXHash(uint accumulator) => new SmallXXHash(accumulator);

        public static implicit operator SmallXXHash4(SmallXXHash hash) => new SmallXXHash4(hash.accumulator);

        public static implicit operator uint(SmallXXHash hash)
        {
            uint avalanche = hash.accumulator;
            avalanche ^= avalanche >> 15;
            avalanche *= primeB;
            avalanche ^= avalanche >> 13;
            avalanche *= primeC;
            avalanche ^= avalanche >> 16;
            return avalanche;
        }

        static uint RotateLeft(uint data, int steps) => (data << steps) | (data >> 32 - steps);

        public SmallXXHash Eat(int data) => RotateLeft(accumulator + (uint)data * primeC, 17) * primeD;

        public SmallXXHash Eat(byte data) => RotateLeft(accumulator + data * primeE, 11) * primeA;
    }

    public readonly struct SmallXXHash4
    {
        const uint primeB = 0b10000101111010111100101001110111;
        const uint primeC = 0b11000010101100101010111000111101;
        const uint primeD = 0b00100111110101001110101100101111;
        const uint primeE = 0b00010110010101100110011110110001;

        readonly uint4 accumulator;

        public SmallXXHash4(uint4 accumulator)
        {
            this.accumulator = accumulator;
        }

        public static SmallXXHash4 Seed(int4 seed) => (uint4)seed + primeE;

        public static implicit operator SmallXXHash4(uint4 accumulator) => new SmallXXHash4(accumulator);

        public static implicit operator uint4(SmallXXHash4 hash)
        {
            uint4 avalanche = hash.accumulator;
            avalanche ^= avalanche >> 15;
            avalanche *= primeB;
            avalanche ^= avalanche >> 13;
            avalanche *= primeC;
            avalanche ^= avalanche >> 16;
            return avalanche;
        }

        static uint4 RotateLeft(uint4 data, int steps) => (data << steps) | (data >> 32 - steps);

        public SmallXXHash4 Eat(int4 data) => RotateLeft(accumulator + (uint4)data * primeC, 17) * primeD;

        public uint4 BytesA => (uint4)this & 255;

        public float4 Floats01A => (float4)BytesA * (1f / 255f);
        public uint4 BytesB => ((uint4)this >> 8) & 255;

        public uint4 BytesC => ((uint4)this >> 16) & 255;

        public uint4 BytesD => (uint4)this >> 24;

        public float4 Floats01B => (float4)BytesB * (1f / 255f);

        public float4 Floats01C => (float4)BytesC * (1f / 255f);

        public float4 Floats01D => (float4)BytesD * (1f / 255f);

        public static SmallXXHash4 operator + (SmallXXHash4 h, int v) => h.accumulator + (uint)v;

        public uint4 GetBits(int count, int shift) => ((uint4)this >> shift) & (uint)((1 << count) - 1);

        public float4 GetBitsAsFloats01(int count, int shift) => (float4)GetBits(count, shift) * (1f / ((1 << count) - 1));

        public static SmallXXHash4 Select(SmallXXHash4 a, SmallXXHash4 b, bool4 c) => math.select(a.accumulator, b.accumulator, c);
    }



    static int
        positionsId = Shader.PropertyToID("_Positions"),
        normalsId = Shader.PropertyToID("_Normals"),
        configId = Shader.PropertyToID("_Config");

    [SerializeField]
    Mesh instanceMesh;

    [SerializeField]
    Material material;

    [SerializeField, Range(1, 512)]
    int resolution = 16;

    [SerializeField, Range(-0.5f, 0.5f)]
    float displacement = 0.1f;

    [SerializeField]
    Shape shape;

    [SerializeField, Range(0.1f, 10f)]
    float instanceScale = 2f;

    NativeArray<float3x4> positions, normals;

    ComputeBuffer positionsBuffer, normalsBuffer;

    MaterialPropertyBlock propertyBlock;

    Bounds bounds;

    bool isDirty;

    public enum Shape { Plane, Sphere, Torus }

    static Shapes.ScheduleDelegate[] shapeJobs = {
        Shapes.Job<Shapes.Plane>.ScheduleParallel,
        Shapes.Job<Shapes.Sphere>.ScheduleParallel,
        Shapes.Job<Shapes.Torus>.ScheduleParallel
    };

    private void OnEnable()
    {
        isDirty = true;

        int length = resolution * resolution;
        length = length / 4 + (length & 1) ;
        positions = new NativeArray<float3x4>(length, Allocator.Persistent);
        normals = new NativeArray<float3x4>(length, Allocator.Persistent);
        positionsBuffer = new ComputeBuffer(length * 4, 3 * 4);
        normalsBuffer = new ComputeBuffer(length * 4, 3 * 4);

        propertyBlock ??= new MaterialPropertyBlock();
        EnableVisualization(length, propertyBlock);
        propertyBlock.SetBuffer(positionsId, positionsBuffer);
        propertyBlock.SetBuffer(normalsId, normalsBuffer);
        propertyBlock.SetVector(configId, new Vector4(resolution, instanceScale / resolution, displacement));
    }

    private void OnDisable()
    {
        positions.Dispose();
        normals.Dispose();
        positionsBuffer.Release();
        normalsBuffer.Release();
        positionsBuffer = null;
        normalsBuffer = null;

        DisableVisualization();
    }

    private void OnValidate()
    {
        if (positionsBuffer != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        if (isDirty || transform.hasChanged)
        {
            isDirty = false;
            transform.hasChanged = false;

            bounds = new Bounds(transform.position, float3(2f * cmax(abs(transform.lossyScale)) + displacement));

            UpdateVisualization(positions, resolution, shapeJobs[(int)shape](
                positions, normals, resolution, transform.localToWorldMatrix, default)
                );

            positionsBuffer.SetData(positions.Reinterpret<float3>(3 * 4 * 4));
            normalsBuffer.SetData(normals.Reinterpret<float3>(3 * 4 * 4));
        }

        Graphics.DrawMeshInstancedProcedural(
            instanceMesh, 0, material, bounds, resolution * resolution, propertyBlock
        );
    }

    protected abstract void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock);
    protected abstract void DisableVisualization();

    protected abstract void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle);
}