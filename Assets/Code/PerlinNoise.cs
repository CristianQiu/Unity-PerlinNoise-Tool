using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class PerlinNoise
{
    #region Job Definitions

    [BurstCompile(FloatPrecision = FloatPrecision.Standard, FloatMode = FloatMode.Fast, CompileSynchronously = true)]
    private struct FillTexelsPerlin2D : IJobFor
    {
        [WriteOnly] public NativeArray<byte> texels;
        public int2 resolution;

        public float2 offset;
        public float2 frequency;

        public void Execute(int index)
        {
            int2 rowCol = new int2(index / resolution.x, index % resolution.x);

            float2 xy = (float2)rowCol / (float2)resolution;
            xy = xy * frequency + offset * frequency;

            float fPerlin = (noise.cnoise(xy) + 1.0f) * 0.5f;
            byte bPerlin = (byte)math.round(fPerlin * 255.0f);

            texels[index] = bPerlin;
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
            offset = (float2)noiseSettings.offset,
            frequency = new float2(noiseSettings.frequency, noiseSettings.frequency),
        }
        .ScheduleParallel(texels.Length, 64, default(JobHandle))
        .Complete();

        tex.Apply();
    }

    #endregion
}