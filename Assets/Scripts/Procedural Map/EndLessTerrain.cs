using System;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural_Map{
    public class EndLessTerrain : MonoBehaviour{
        const float viewerMoveThresholdForChunkUpdate = 25f;

        const float sqrtViewerMoveThresholdForChunkUpdate =
            viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

        public static float maxViewDts;
        public LODInfo[] detailLevel;


        public Transform viewer;
        public static Vector2 viewerPosition;
        Vector2 viewerPositionOld;
        int chunkSize;
        int chunkVisibleInViewDistances;

        public Material mapMaterial;

        public Transform parentChunk;

        static MapGenerator mapGenerator;


        Dictionary<Vector2, TerrainChunk> terrainChunksDictionary = new();
        List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

        void Start(){
            mapGenerator = FindObjectOfType<MapGenerator>();
            maxViewDts = detailLevel[^1].visibleDtsThreshold;
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunkVisibleInViewDistances = Mathf.RoundToInt(maxViewDts / chunkSize);
            UpdateVisibleChunk();
        }

        void Update(){
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

            if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrtViewerMoveThresholdForChunkUpdate) {
                viewerPositionOld = viewerPosition;
                UpdateVisibleChunk();
            }
        }

        void UpdateVisibleChunk(){
            for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
                terrainChunksVisibleLastUpdate[i].SetVisible(false);
            }

            terrainChunksVisibleLastUpdate.Clear();
            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

            for (int yOffSet = -chunkVisibleInViewDistances;
                 yOffSet <= chunkVisibleInViewDistances;
                 yOffSet++) {
                for (int xOffSet = -chunkVisibleInViewDistances; xOffSet <= chunkVisibleInViewDistances; xOffSet++) {
                    Vector2 viewChunkCoord = new Vector2(currentChunkCoordX + xOffSet, currentChunkCoordY + yOffSet);
                    if (terrainChunksDictionary.ContainsKey(viewChunkCoord)) {
                        terrainChunksDictionary[viewChunkCoord].UpdateTerrainChunk();
                        if (terrainChunksDictionary[viewChunkCoord].IsVisible()) {
                            terrainChunksVisibleLastUpdate.Add(terrainChunksDictionary[viewChunkCoord]);
                        }
                    }
                    else {
                        terrainChunksDictionary.Add(viewChunkCoord,
                            new TerrainChunk(viewChunkCoord, chunkSize, detailLevel, parentChunk, mapMaterial));
                    }
                }
            }
        }

        public class TerrainChunk{
            Vector2 position;
            GameObject meshObject;
            Bounds bounds;
            MeshRenderer meshRenderer;
            MeshFilter meshFilter;
            LODInfo[] detailLevels;
            LODMesh[] LODMeshes;

            MapGenerator.MapData mapData;
            bool mapDataReceived;
            int previousLODIndex = -1;

            public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material){
                this.detailLevels = detailLevels;

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
                LODMeshes = new LODMesh[this.detailLevels.Length];

                for (int i = 0; i < detailLevels.Length; i++) {
                    LODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                }

                mapGenerator.RequestMapData(OnMapDataReceived);
            }

            void OnMapDataReceived(MapGenerator.MapData mapData){
                this.mapData = mapData;
                mapDataReceived = true;

                Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap,
                    MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
                meshRenderer.material.mainTexture = texture;
                UpdateTerrainChunk();
            }

            void OnMeshDataReceived(MeshData meshData){
                meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk(){
                if (!mapDataReceived) {
                    return;
                }

                float viewDtsFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewDtsFromNearestEdge <= maxViewDts;

                if (visible) {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length; i++) {
                        if (viewDtsFromNearestEdge > detailLevels[i].visibleDtsThreshold) {
                            lodIndex = i + 1;
                        }
                        else {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex) {
                        LODMesh lodMesh = LODMeshes[lodIndex];
                        if (lodMesh.hasMesh) {
                            meshFilter.mesh = lodMesh.mesh;
                            previousLODIndex = lodIndex;
                        }
                        else if (!lodMesh.hasRequestedMesh) {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }

                SetVisible(visible);
            }

            public void SetVisible(bool visible){
                meshObject.SetActive(visible);
            }

            public bool IsVisible(){
                return meshObject.activeSelf;
            }
        }

        class LODMesh{
            public Mesh mesh;
            public bool hasRequestedMesh;
            public bool hasMesh;
            [Range(0, 6)] int lod;
            Action updateCallback;

            public LODMesh(int lod, Action updateCallback){
                this.lod = lod;
                this.updateCallback = updateCallback;
            }

            void OnMeshDataReceive(MeshData meshData){
                mesh = meshData.CreateMesh();
                hasMesh = true;
                updateCallback();
            }

            public void RequestMesh(MapGenerator.MapData mapData){
                hasRequestedMesh = true;
                mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceive);
            }
        }

        [Serializable]
        public struct LODInfo{
            [Range(0, 6)] public int lod;
            public float visibleDtsThreshold;
        }
    }
}