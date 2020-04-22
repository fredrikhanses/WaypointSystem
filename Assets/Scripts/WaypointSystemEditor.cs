using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class WaypointSystemEditor : EditorWindow
{
    public Vector3 point;
    SerializedObject serializedObject;
    SerializedProperty propertyPoint;

    Vector3[] points;

    [MenuItem("Tools/WaypointSystem")]
    public static void OpenWaypointSystem()
    {
        GetWindow<WaypointSystemEditor>("WaypointSystem");
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        propertyPoint = serializedObject.FindProperty("point");
        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void OnGUI()
    {
        serializedObject.Update();

        if (serializedObject.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
        }
    }

    private void DuringSceneGUI(SceneView sceneView)
    {

    }
}
