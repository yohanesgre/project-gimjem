using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectPlacer))]
public class ObjectPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ObjectPlacer objectPlacer = (ObjectPlacer)target;

        if (GUILayout.Button("Populate Grid"))
        {
            objectPlacer.PopulateGrid();
        }

        if (GUILayout.Button("Spawn Objects"))
        {
            objectPlacer.SpawnObjects();
        }
    }
}