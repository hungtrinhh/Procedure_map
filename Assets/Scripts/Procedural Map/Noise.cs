using UnityEngine;
using Random = System.Random;

namespace Procedural_Map{
    public static class Noise{
        public enum NormalizeMode{
            Local,
            Global,
        }

        public static float[,] GenerateNoiseMap(int width,
            int height,
            int seed,
            float scale,
            int octaves,
            float persistance,
            float lacunarity, Vector2 offset,
            NormalizeMode normalizeMode){
            Random random = new Random(seed);
            Vector2[] octavesOffSet = new Vector2[octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;
            float frequency = 1;
            for (int i = 0; i < octaves; i++) {
                float offsetX = random.Next(-100000, 100000) + offset.x;
                float offsetY = random.Next(-100000, 100000) + offset.y;
                octavesOffSet[i] = new Vector2(offsetX, offsetY);
                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }

            float halfWidth = width >> 1;
            float halfHeight = height >> 1;

            if (scale <= 0) {
                scale = 1 / 1000f;
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;

            float[,] noiseMap = new float[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    float noiseHeight = 0;
                    amplitude = 1;
                    frequency = 1;

                    for (int i = 0; i < octaves; i++) {
                        float widthSample = (x - halfWidth + octavesOffSet[i].x) / scale * frequency;
                        float heightSample = (y - halfHeight + octavesOffSet[i].y) / scale * frequency;
                        float perlinValue = Mathf.PerlinNoise(widthSample, heightSample) * 2 - 1;

                        noiseHeight += perlinValue * amplitude;
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight) {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minLocalNoiseHeight) {
                        minLocalNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    switch (normalizeMode) {
                        case NormalizeMode.Global:
                            float normalizeHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight/1.5f);
                            noiseMap[x, y] = Mathf.Clamp(normalizeHeight,0 , int.MaxValue);
                            break;
                        case NormalizeMode.Local:
                            noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);

                            break;
                    }
                }
            }

            return noiseMap;
        }
    }
}