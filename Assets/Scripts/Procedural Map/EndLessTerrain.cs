using System;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural_Map
{
    public class EndLessTerrain : MonoBehaviour
    {
        public const float maxViewDis = 300;
        public Transform viewer;
        public static Vector2 viewerPosition;
        int chunkSize;
        int chunkVisibleInViewDistances;

        Dictionary<Vector2, TerrainChunk> terrainChunksDictionary = new();

        void Start()
        {
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
                    }
                    else
                    {
                        terrainChunksDictionary.Add(viewChunkCoord, new TerrainChunk(viewChunkCoord, chunkSize));
                    }
                }
            }
        }

        public class TerrainChunk
        {
            Vector2 position;
            GameObject meshObject;
            Bounds bounds;

            public TerrainChunk(Vector2 coord, int size)
            {
                position = coord * size;
                Vector3 positionV3 = new Vector3(position.x,0, position.y);
                bounds = new Bounds(position, Vector2.one * size);
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);

                meshObject.transform.position = positionV3;
                meshObject.transform.localPosition = Vector3.one * size / 10f;
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
        }
    }
}