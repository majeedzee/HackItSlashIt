using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PaintTerrain))]
public class TerrainBuilderEditor : Editor {

	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PaintTerrain myScript = (PaintTerrain)target;
        if (GUILayout.Button("Generate Terrain"))
        {
            myScript.Start();
        }
    }
}
