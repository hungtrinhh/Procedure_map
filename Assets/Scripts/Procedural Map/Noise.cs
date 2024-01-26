using UnityEngine;
using Random = System.Random;

namespace Procedural_Map
{
    public static class Noise
    {
        public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves,
            float persistance,
            float lacunarity, Vector2 offset)
        {
            Random random = new Random(seed);
            Vector2[] octavesOffSet = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = random.Next(-100000, 100000) + offset.x;
                float offsetY = random.Next(-100000, 100000) + offset.y;
                octavesOffSet[i] = new Vector2(offsetX, offsetY);
            }

            float halfWidth = width >> 1;
            float halfHeight = height >> 1;

            if (scale <= 0)
            {
                scale = 1 / 1000f;
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float[,] noiseMap = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;


                    for (int i = 0; i < octaves; i++)
                    {
                        float widthSample = (x - halfWidth) / scale * frequency + octavesOffSet[i].x;
                        float heightSample = (y - halfHeight) / scale * frequency + octavesOffSet[i].y;
                        float perlinValue = Mathf.PerlinNoise(widthSample, heightSample) * 2 - 1;

                        noiseHeight += perlinValue * amplitude;
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }
    }
}