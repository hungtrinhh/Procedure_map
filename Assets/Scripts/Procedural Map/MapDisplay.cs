using System;
using UnityEngine;

namespace Procedural_Map
{
    public class MapDisplay : MonoBehaviour
    {
        public Renderer textureRenderer;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        private void Awake()
        {
            textureRenderer = GetComponent<Renderer>();
        }


        public void DrawTexture(Texture2D texture2D)
        {
            textureRenderer.sharedMaterial.mainTexture = texture2D;
            textureRenderer.transform.localScale = new Vector3(texture2D.width, 1, texture2D.height);
        }

        public void DrawMesh(MeshData meshData, Texture2D texture2D)
        {
            meshFilter.sharedMesh = meshData.CreateMesh();
            meshRenderer.sharedMaterial.mainTexture = texture2D;
        }
    }
}