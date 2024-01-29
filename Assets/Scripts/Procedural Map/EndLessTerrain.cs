using System;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural_Map
{
    public class EndLessTerrain : MonoBehaviour
    {
        public const float maxViewDis = 300;
        public LODInfo[] detailLevel;


        public Transform viewer;
        public static Vector2 viewerPosition;
        int chunkSize;
        int chunkVisibleInViewDistances;

        public Material mapMaterial;

        public Transform parentChunk;

        static MapGenerator mapGenerator;


        Dictionary<Vector2, TerrainChunk> terrainChunksDictionary = new();
        List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

        void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunkVisibleInViewDistances = Mathf.RoundToInt(maxViewDis / chunkSize);
        }

        void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            UpdateVisibleChunk();
        }

        void UpdateVisibleChunk()
        {
            for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
            {
                terrainChunksVisibleLastUpdate[i].SetVisible(false);
            }

            terrainChunksVisibleLastUpdate.Clear();


            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

            for (int yOffSet = -chunkVisibleInViewDistances;
                 yOffSet <= chunkVisibleInViewDistances;
                 yOffSet++)
            {
                for (int xOffSet = -chunkVisibleInViewDistances; xOffSet <= chunkVisibleInViewDistances; xOffSet++)
                {
                    Vector2 viewChunkCoord = new Vector2(currentChunkCoordX + xOffSet, currentChunkCoordY + yOffSet);
                    if (terrainChunksDictionary.ContainsKey(viewChunkCoord))
                    {
                        terrainChunksDictionary[viewChunkCoord].UpdateTerrainChunk();
                        if (terrainChunksDictionary[viewChunkCoord].IsVisible())
                        {
                            terrainChunksVisibleLastUpdate.Add(terrainChunksDictionary[viewChunkCoord]);
                        }
                    }
                    else
                    {
                        terrainChunksDictionary.Add(viewChunkCoord,
                            new TerrainChunk(viewChunkCoord, chunkSize, parentChunk, mapMaterial));
                    }
                }
            }
        }

        public class TerrainChunk
        {
            Vector2 position;
            GameObject meshObject;
            Bounds bounds;
            MeshRenderer meshRenderer;
            MeshFilter meshFilter;

            public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
            {
                position = coord * size;
                Vector3 positionV3 = new Vector3(position.x, 0, position.y);
                bounds = new Bounds(position, Vector2.one * size);

                meshObject = new("Terrain Chunk");

                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshRenderer.material = material;

                meshObject.transform.position = positionV3;
                meshObject.transform.parent = parent;
                SetVisible(false);
                
                mapGenerator.RequestMapData(OnMapDataReceived);
            }

            void OnMapDataReceived(MapGenerator.MapData mapData)
            {
                // mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            }

            void OnMeshDataReceived(MeshData meshData)
            {
                meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk()
            {
                float viewDtsFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewDtsFromNearestEdge <= maxViewDis;
                SetVisible(visible);
            }

            public void SetVisible(bool visible)
            {
                meshObject.SetActive(visible);
            }

            public bool IsVisible()
            {
                return meshObject.activeSelf;
            }
        }

        class LODMesh
        {
            public Mesh mesh;
            public bool hasRequested;
            public bool hasMesh;
            int lod;

            public LODMesh(int lod)
            {
                this.lod = lod;
            }

            void OnMeshDataReceive(MeshData meshData)
            {
                mesh = meshData.CreateMesh();
                hasMesh = true;
            }

            public void RequestMesh(MapGenerator.MapData mapData)
            {
                hasRequested = true;
                mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceive);
            }
        }

        [Serializable]
        public struct LODInfo
        {
            public int lod;
            public float visibleDtsThreshold;
        }
    }
}