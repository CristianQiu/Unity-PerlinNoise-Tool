using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class PerlinNoise
{
    #region Job Definitions

    [BurstCompile]
    private struct Perlin2DFillTexelsJob : IJobFor
    {
        [WriteOnly] public NativeArray<Color32> texels;

        [ReadOnly] public int2 resolution;
        [ReadOnly] public float2 offset;
        [ReadOnly] public float2 frequency;

        public void Execute(int index)
        {
            int2 rowCol = new int2(index / resolution.x, index % resolution.x);

            float2 xy = (float2)rowCol / (float2)resolution;
            xy = xy * frequency + offset * frequency;

            float fPerlin = Perlin2D(xy.x, xy.y);

            // perlin noise by Unity says it may return values slightly below 0 or beyond 1
            byte bPerlin = (byte)(math.clamp(fPerlin, 0.0f, 1.0f) * 255.0f);
            texels[index] = new Color32(bPerlin, bPerlin, bPerlin, 255);
        }
    }

    #endregion

    #region 2D Methods

    public static float Perlin2D(float x, float y)
    {
        // TODO: Implement perlin noise with Unity's math library. I believe perlin will take advantage from SIMD.
        return Mathf.PerlinNoise(x, y);
    }

    public static float Perlin2D(Vector2 xy)
    {
        return Perlin2D(xy.x, xy.y);
    }

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