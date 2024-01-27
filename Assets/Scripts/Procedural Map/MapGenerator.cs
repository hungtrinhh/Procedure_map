using System;
using UnityEngine;

namespace Procedural_Map
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode
        {
            NoiseMap,
            ColorMap,
            Mesh
        }
        
        public const int mapChunkSize = 241;


        [Range(0, 6)] public int levelOfDetail;
        [Space] public DrawMode drawMode = DrawMode.NoiseMap;


        public float noiseScale;
        public bool autoUpdate;
        public int octaves;
        public float persistance;
        public float lacunarity;
        public float heightMultiplier;

        public AnimationCurve meshHeightCurve;

        public int seed;
        public Vector2 offset;
        public TerrainType[] regions;

        private void OnValidate()
        {
            if (octaves < 0)
            {
                octaves = 0;
            }
        }


        public void GenerateMap()
        {
            var noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance,
                lacunarity, offset);
            Color[] colorsMap = new Color[mapChunkSize * mapChunkSize];
            MapDisplay display = FindObjectOfType<MapDisplay>();
            Debug.Log("Draw on " + display.gameObject.name);
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            colorsMap[y * mapChunkSize + x] = regions[i].color;
                            break;
                        }
                    }
                }
            }

            switch (drawMode)
            {
                case DrawMode.ColorMap:
                    display.DrawTexture(TextureGenerator.TextureFromColorMap(colorsMap, mapChunkSize, mapChunkSize));

                    break;
                case DrawMode.NoiseMap:
                    display.DrawTexture(TextureGenerator.TextTureFromHeightMap(noiseMap));
                    break;
                case DrawMode.Mesh:
                    display.DrawMesh(
                        MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, meshHeightCurve, levelOfDetail),
                        TextureGenerator.TextureFromColorMap(colorsMap, mapChunkSize, mapChunkSize));
                    break;
            }
        }

        [Serializable]
        public struct TerrainType
        {
            public string name;
            public float height;
            public Color color;
        }
    }
}