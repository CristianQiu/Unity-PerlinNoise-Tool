using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class PerlinNoise
{
    #region Job Definitions

    [BurstCompile(FloatPrecision = FloatPrecision.Standard, FloatMode = FloatMode.Fast, CompileSynchronously = true)]
    private struct Perlin2DFillTexelsJob : IJobFor
    {
        [WriteOnly] public NativeArray<Color32> texels;

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
            texels[index] = new Color32(bPerlin, bPerlin, bPerlin, 255);
        }
    }

    #endregion

    #region 2D Methods

    public static Color32[] Perlin2DColors(PerlinNoiseTextureSettings noiseSettings)
    {
        int numTexels = noiseSettings.GetNumTexels();
        NativeArray<Color32> texels = new NativeArray<Color32>(numTexels, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        new Perlin2DFillTexelsJob()
        {
            texels = texels,
            resolution = new int2(noiseSettings.resolution.x, noiseSettings.resolution.y),
            offset = (float2)noiseSettings.offset,
            frequency = new float2(noiseSettings.frequency, noiseSettings.frequency),
        }
        .ScheduleParallel(numTexels, 64, default(JobHandle))
        .Complete();

        Color32[] texelsArray = texels.ToArray();
        texels.Dispose();

        return texelsArray;
    }

    #endregion
}