using System;
using System.Collections.Generic;
using System.Threading;
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

        private readonly Queue<MapThreadingInfo<MapData>> mapDataThreadingInfoQueue = new();

        private void OnValidate()
        {
            if (octaves < 0)
            {
                octaves = 0;
            }
        }
        
        

        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData();
            MapDisplay display = FindObjectOfType<MapDisplay>();

            switch (drawMode)
            {
                case DrawMode.ColorMap:
                    display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));

                    break;
                case DrawMode.NoiseMap:
                    display.DrawTexture(TextureGenerator.TextTureFromHeightMap(mapData.heightMap));
                    break;
                case DrawMode.Mesh:
                    display.DrawMesh(
                        MeshGenerator.GenerateTerrainMesh(mapData.heightMap, heightMultiplier, meshHeightCurve,
                            levelOfDetail),
                        TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                    break;
            }
        }

        public void RequestMapData(Action<MapData> callBack)
        {
            ThreadStart threadStart = delegate { MapDataThread(callBack); };
            new Thread(threadStart).Start();
        }

        void MapDataThread(Action<MapData> callBack)
        {
            MapData mapData = GenerateMapData();
            lock (mapDataThreadingInfoQueue)
            {
                mapDataThreadingInfoQueue.Enqueue(new MapThreadingInfo<MapData>(callBack, mapData));
            }
        }

        void Update()
        {
            if (mapDataThreadingInfoQueue.Count > 0)
            {
                for (int i = 0; i < mapDataThreadingInfoQueue.Count; i++)
                {
                    MapThreadingInfo<MapData> threadingInfo = mapDataThreadingInfoQueue.Dequeue();
                    threadingInfo.callBack(threadingInfo.parameter);
                }
            }
        }


        MapData GenerateMapData()
        {
            var noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance,
                lacunarity, offset);
            Color[] colorsMap = new Color[mapChunkSize * mapChunkSize];
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

            return new MapData(noiseMap, colorsMap);
        }

        struct MapThreadingInfo<T>
        {
            public readonly Action<T> callBack;
            public readonly T parameter;

            public MapThreadingInfo(Action<T> callBack, T parameter)
            {
                this.callBack = callBack;
                this.parameter = parameter;
            }
        }


        [Serializable]
        public struct TerrainType
        {
            public string name;
            public float height;
            public Color color;
        }

        public struct MapData
        {
            public readonly float[,] heightMap;
            public readonly Color[] colorMap;


            public MapData(float[,] heightMap, Color[] colorMap)
            {
                this.heightMap = heightMap;
                this.colorMap = colorMap;
            }
        }
    }
}