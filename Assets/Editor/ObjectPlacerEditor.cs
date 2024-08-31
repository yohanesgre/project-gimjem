using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectPlacer))]
public class ObjectPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ObjectPlacer objectPlacer = (ObjectPlacer)target;

        if (GUILayout.Button("Place Objects"))
        {
            objectPlacer.PlaceObjects();
        }
    }
}