using UnityEditor;
using UnityEngine;

public class SnapperTool : EditorWindow
{
    private const string k_UndoStringSnap = "Snap Objects";

    [MenuItem("Tools/Snapper")]
    public static void OpenTheThing()
    {
        GetWindow<SnapperTool>("Snapper");
    }

    private void OnEnable()
    {
        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;

    }
    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        Handles.DrawLine(Vector3.zero, Vector3.up);
    }

    private void OnGUI()
    {
        using(new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Snap Selection"))
            {
                SnapSelection();
            }
        }
    }

    private void SnapSelection()
    {
        foreach (GameObject gameObject in Selection.gameObjects)
        {
            Undo.RecordObject(gameObject.transform, k_UndoStringSnap);
            gameObject.transform.position = gameObject.transform.position.Round();
        }
    }
}
