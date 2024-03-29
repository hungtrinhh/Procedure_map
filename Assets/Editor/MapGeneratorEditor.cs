using Procedural_Map;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MapGenerator mapGenerator = (MapGenerator)target;
            ;
            if (DrawDefaultInspector())
            {
                if (mapGenerator.autoUpdate)
                {
                    mapGenerator.DrawMapInEditor();
                }
            }

            if (GUILayout.Button("Generate"))
            {
                mapGenerator.DrawMapInEditor();
            }
        }
    }
}