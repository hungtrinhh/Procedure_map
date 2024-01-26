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


        public DrawMode drawMode = DrawMode.NoiseMap;
        public int mapWidth, mapHeight;
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
            if (mapWidth < 1)
            {
                mapWidth = 1;
            }

            if (mapHeight < 1)
            {
                mapHeight = 1;
            }

            if (octaves < 0)
            {
                octaves = 0;
            }
        }


        public void GenerateMap()
        {
            var noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance,
                lacunarity, offset);
            Color[] colorsMap = new Color[mapWidth * mapHeight];
            MapDisplay display = FindObjectOfType<MapDisplay>();
            Debug.Log("Draw on " + display.gameObject.name);
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            colorsMap[y * mapWidth + x] = regions[i].color;
                            break;
                        }
                    }
                }
            }

            switch (drawMode)
            {
                case DrawMode.ColorMap:
                    display.DrawTexture(TextureGenerator.TextureFromColorMap(colorsMap, mapWidth, mapHeight));

                    break;
                case DrawMode.NoiseMap:
                    display.DrawTexture(TextureGenerator.TextTureFromHeightMap(noiseMap));
                    break;
                case DrawMode.Mesh:
                    display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier),
                        TextureGenerator.TextureFromColorMap(colorsMap, mapWidth, mapHeight));
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