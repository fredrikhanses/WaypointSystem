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
        DrawGrid(2);
    }

    private void DrawGrid(int size)
    {
        int initSize = size;
        Vector3 right = Vector3.zero;
        Vector3 left = Vector3.zero;
        Vector3 forward = Vector3.zero;
        Vector3 back = Vector3.zero;
        while (size > 0)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Handles.color = Color.cyan;
                    Handles.DrawLine(right, Vector3.left + right);
                    Handles.DrawLine(right, Vector3.forward + right);
                    Handles.DrawLine(right, Vector3.back + right);
                    right += Vector3.right;
                    Handles.color = Color.black;
                    Handles.DrawLine(left, Vector3.right + left);
                    Handles.DrawLine(left, Vector3.forward + left);
                    Handles.DrawLine(left, Vector3.back + left);
                    left += Vector3.left;
                    Handles.color = Color.blue;
                    Handles.DrawLine(forward, Vector3.right + forward);
                    Handles.DrawLine(forward, Vector3.left + forward);
                    Handles.DrawLine(forward, Vector3.back + forward);
                    forward += Vector3.forward;
                    Handles.color = Color.white;
                    Handles.DrawLine(back, Vector3.right + back);
                    Handles.DrawLine(back, Vector3.left + back);
                    Handles.DrawLine(back, Vector3.forward + back);
                    back += Vector3.back;
                }
            }
            size--;
        }

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
