using UnityEngine;

namespace Procedural_Map
{
    public static class MeshGenerator
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap,float heightMultiplier) 
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;
            MeshData meshData = new MeshData(width, height);

            int vextexIndex = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    meshData.vectices[vextexIndex] = new Vector3(topLeftX + x, heightMap[x, y]*heightMultiplier, topLeftZ - y);
                    meshData.uvs[vextexIndex] = new Vector2(x / (float)width, y: y / (float)height);

                    if (x < width - 1 && y < height - 1)
                    {
                        meshData.AddTriangle(vextexIndex, vextexIndex + width + 1, vextexIndex + width);
                        meshData.AddTriangle(vextexIndex + width + 1, vextexIndex, vextexIndex + 1);
                    }


                    vextexIndex++;
                }
            }

            return meshData;
        }
    }

    public class MeshData
    {
        public Vector3[] vectices;
        public int[] triangles;
        private int triangleIndex;
        public Vector2[] uvs;

        public MeshData(int meshWidth, int meshHeight)
        {
            vectices = new Vector3[meshWidth * meshHeight];
            triangles = new int[(meshHeight - 1) * (meshWidth - 1) * 6];
            uvs = new Vector2[meshWidth * meshHeight];
        }

        public void AddTriangle(int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vectices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}