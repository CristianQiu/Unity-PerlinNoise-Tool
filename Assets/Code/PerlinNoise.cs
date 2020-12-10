using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class PerlinNoise
{
    #region Job Definitions

    [BurstCompile]
    private struct Perlin2DColorsJob : IJobParallelFor
    {
        [ReadOnly] public PerlinNoiseTextureSettings noiseSettings;
        [WriteOnly] public NativeArray<Color32> pixels;

        public void Execute(int index)
        {
            int width = noiseSettings.resolution.x;
            int height = noiseSettings.resolution.y;

            int row = index / width;
            int col = index % width;

            float x = (float)col / (float)width;
            float y = (float)row / (float)height;

            x *= noiseSettings.frequency;
            y *= noiseSettings.frequency;

            // for complete correctness must be multiplied by frequency
            x += (noiseSettings.offset.x * noiseSettings.frequency);
            y += (noiseSettings.offset.y * noiseSettings.frequency);

            float perlinFloat = Perlin2D(x, y);

            // perlin noise by Unity says it may return values slightly below 0 or beyond 1
            byte perlinByte = (byte)(math.clamp(perlinFloat, 0.0f, 1.0f) * 255.0f);
            pixels[index] = new Color32(perlinByte, perlinByte, perlinByte, 255);
        }

        public void Dispose()
        {
            pixels.Dispose();
        }
    }

    #endregion

    #region 2D Methods

    public static float Perlin2D(float x, float y)
    {
        return Mathf.PerlinNoise(x, y);
    }

    public static float Perlin2D(Vector2 xy)
    {
        return Perlin2D(xy.x, xy.y);
    }

    public static Color32[] Perlin2DColors(PerlinNoiseTextureSettings noiseSettings)
    {
        int numPixels = noiseSettings.GetNumPixels();

        Perlin2DColorsJob fillPixelsJob = new Perlin2DColorsJob()
        {
            noiseSettings = noiseSettings,
            pixels = new NativeArray<Color32>(numPixels, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
        };

        JobHandle fillPixelsHandle = fillPixelsJob.Schedule(numPixels, 16);
        fillPixelsHandle.Complete();

        Color32[] pixelsArray = fillPixelsJob.pixels.ToArray();

        fillPixelsJob.Dispose();

        return pixelsArray;
    }

    #endregion
}