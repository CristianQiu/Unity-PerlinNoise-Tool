using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class PerlinNoise
{
    #region Private Attributes

    private const float FrequencyEaseOfUseFactor = 100.0f;

    #endregion

    #region Job Definitions

    [BurstCompile(FloatPrecision = FloatPrecision.Standard, FloatMode = FloatMode.Fast, CompileSynchronously = true)]
    private struct FillTexelsPerlin2D : IJobFor
    {
        [WriteOnly] public NativeArray<byte> texels;
        public int2 resolution;

        public float2 offset;
        public float frequency;
        public int octaves;
        public float persistence;
        public float lacunarity;

        public void Execute(int index)
        {
            int2 rowCol = new int2(index / resolution.x, index % resolution.x);
            float2 xy = (float2)rowCol;

            float noiseAccum = 0.0f;
            float maxAmp = 0.0f;
            float amp = 1.0f;
            float freq = frequency;

            for (int i = 0; i < octaves; ++i)
            {
                float2 coords = xy * freq + offset * freq * FrequencyEaseOfUseFactor;
                noiseAccum += (noise.cnoise(coords) + 1.0f) * 0.5f * amp;

                maxAmp += amp;
                amp *= persistence;
                freq *= lacunarity;
            }

            float fnoise = noiseAccum / maxAmp;
            byte bnoise = (byte)math.round(fnoise * 255.0f);

            texels[index] = bnoise;
        }
    }

    #endregion

    #region 2D Methods

    public static void FillTextureWithPerlin2D(Texture2D tex, PerlinNoiseTextureSettings noiseSettings)
    {
        NativeArray<byte> texels = tex.GetRawTextureData<byte>();

        new FillTexelsPerlin2D()
        {
            texels = texels,
            resolution = new int2(noiseSettings.resolution.x, noiseSettings.resolution.y),
            offset = new float2(noiseSettings.offset.y, noiseSettings.offset.x),
            frequency = noiseSettings.frequency / FrequencyEaseOfUseFactor,
            octaves = noiseSettings.octaves,
            persistence = noiseSettings.persistence,
            lacunarity = noiseSettings.lacunarity,
        }
        .ScheduleParallel(texels.Length, 64, default(JobHandle))
        .Complete();

        tex.Apply();
    }

    #endregion
}