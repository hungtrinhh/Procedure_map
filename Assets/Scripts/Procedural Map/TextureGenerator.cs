using UnityEngine;

namespace Procedural_Map
{
    public static class TextureGenerator
    {
        public static Texture2D TextureFromColorMap(Color[] colorsMap, int width, int height)
        {
            Texture2D texture2D = new Texture2D(width, height);
            texture2D.SetPixels(colorsMap);
            texture2D.filterMode = FilterMode.Point;
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.Apply();
            return texture2D;
        }

        public static Texture2D TextTureFromHeightMap(float[,] heightMap)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            Color[] colorsMap = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colorsMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                }
            }
            
            return TextureFromColorMap(colorsMap, width, height);
        }
    }
}