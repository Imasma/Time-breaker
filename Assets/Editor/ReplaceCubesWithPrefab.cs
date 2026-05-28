using UnityEditor;
using UnityEngine;

public class ReplaceCubesWithPrefab : EditorWindow
{
    public GameObject prefabToUse;

    [MenuItem("Tools/Replace Cubes With Prefab")]
    static void Open() => GetWindow<ReplaceCubesWithPrefab>("Replace Cubes");

    void OnGUI()
    {
        GUILayout.Label("Prefab de remplacement", EditorStyles.boldLabel);
        prefabToUse = (GameObject)EditorGUILayout.ObjectField(
            "Prefab", prefabToUse, typeof(GameObject), false);

        EditorGUILayout.HelpBox(
            "Sélectionne tes cubes dans la Hierarchy puis clique Replace.",
            MessageType.Info);

        if (GUILayout.Button("Replace Selection") && prefabToUse != null)
            ReplaceSelected();
    }

    void ReplaceSelected()
    {
        var selection = Selection.gameObjects;
        int count = 0;

        foreach (var go in selection)
        {
            // Copie position/rotation/scale
            Transform t = go.transform;
            GameObject newGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabToUse);
            Undo.RegisterCreatedObjectUndo(newGo, "Replace with Prefab");

            newGo.transform.SetParent(t.parent);
            newGo.transform.position = t.position;
            newGo.transform.rotation = t.rotation;
            newGo.transform.localScale = t.localScale;
            newGo.name = go.name;

            Undo.DestroyObjectImmediate(go);
            count++;
        }

        Debug.Log($"{count} cubes remplacés avec succès !");
    }
}