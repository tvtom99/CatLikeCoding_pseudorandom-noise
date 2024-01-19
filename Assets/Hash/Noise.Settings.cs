using System;
using Unity.Burst;
using UnityEngine;

public static partial class Noise
{
    [Serializable]
    public struct Settings
    {
        public int seed;

        [Min(1)]
        public int frequency;

        [Range(1, 6)]
        public int octaves;

        public static Settings Default => new Settings 
        { 
            frequency = 4,
            octaves = 1
        };
    }
}