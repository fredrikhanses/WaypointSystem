using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SnapperTool : EditorWindow
{
    public enum GridType
    {
        Cartesian,
        Polar
    }

    private const string k_UndoStringSnap = "Snap Objects";

    [Range(0f, 6f)]
    public float gridSize = 1f;
    [Range(4, 100)]
    public int angularDivisions = 24;
    public GridType gridType = GridType.Cartesian;
    //public Vector3 point;

    SerializedObject serializedObject;
    SerializedProperty propertyGridSize;
    SerializedProperty propertyGridType;
    SerializedProperty propertyAngularDivisions;
    //SerializedProperty propertyPoint;

    [MenuItem("Tools/Snapper")]
    public static void OpenTheThing()
    {
        GetWindow<SnapperTool>("Snapper");
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        propertyGridSize = serializedObject.FindProperty("gridSize");
        propertyGridType = serializedObject.FindProperty("gridType");
        propertyAngularDivisions = serializedObject.FindProperty("angularDivisions");
        //propertyPoint = serializedObject.FindProperty("point");

        gridSize = EditorPrefs.GetFloat("SNAPPER_TOOL_gridSize", 1f);
        gridType = (GridType)EditorPrefs.GetInt("SNAPPER_TOOL_gridType", 0);
        angularDivisions = EditorPrefs.GetInt("SNAPPER_TOOL_angularDivisions", 0);

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;

    }
    private void OnDisable()
    {
        EditorPrefs.SetFloat("SNAPPER_TOOL_gridSize", gridSize);
        EditorPrefs.SetInt("SNAPPER_TOOL_gridType", (int)gridType);
        EditorPrefs.SetInt("SNAPPER_TOOL_angularDivisions", angularDivisions);

        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
    //    serializedObject.Update();
    //    propertyPoint.vector3Value = Handles.PositionHandle(propertyPoint.vector3Value, Quaternion.identity);
    //    serializedObject.ApplyModifiedProperties();

        if(Event.current.type == EventType.Repaint)
        {
            const float gridExtent = 16f;
            Handles.zTest = CompareFunction.LessEqual;
            if (gridType == GridType.Cartesian)
            {
                DrawGridCartesian(gridExtent);
            }
            else if(gridType == GridType.Polar)
            {
                DrawGridPolar(gridExtent);
            }
        }
    }

    private void DrawGridPolar(float gridExtent)
    {
        int ringCount = Mathf.RoundToInt(gridExtent / gridSize);

        float radiusOuter = (ringCount - 1) * gridSize;

        for (int i = 1; i < ringCount; i++)
        {
            Handles.DrawWireDisc(Vector3.zero, Vector3.up, i * gridSize);
        }
        for (int i = 0; i < angularDivisions; i++)
        {
            float turn = i / (float)angularDivisions;
            float angularRadius = 2 * turn * Mathf.PI;
            float x = Mathf.Cos(angularRadius);
            float y = Mathf.Sin(angularRadius);
            Vector3 direction = new Vector3(x, 0f, y);
            Handles.DrawAAPolyLine(Vector3.zero, direction * radiusOuter);
        }
    }

    private void DrawGridCartesian(float gridExtent)
    {
        int lineCount = Mathf.RoundToInt((gridExtent * 2) / gridSize);
        if (lineCount % 2 == 0)
        {
            lineCount++;
        }
        int halfLineCount = lineCount / 2;
        for (int i = 0; i < lineCount; i++)
        {
            int offset = i - halfLineCount;
            float xCoord = offset * gridSize;
            float zCoord0 = halfLineCount * gridSize;
            float zCoord1 = -halfLineCount * gridSize;
            Vector3 point0 = new Vector3(xCoord, 0f, zCoord0);
            Vector3 point1 = new Vector3(xCoord, 0f, zCoord1);
            Handles.DrawAAPolyLine(point0, point1);
            point0 = new Vector3(zCoord0, 0f, xCoord);
            point1 = new Vector3(zCoord1, 0f, xCoord);
            Handles.DrawAAPolyLine(point0, point1);
        }
    }

    private void OnGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(propertyGridType);
        EditorGUILayout.PropertyField(propertyGridSize);
        if(gridType == GridType.Polar)
        {
            EditorGUILayout.PropertyField(propertyAngularDivisions);
            propertyAngularDivisions.intValue = Mathf.Max(4, propertyAngularDivisions.intValue);
        }
        serializedObject.ApplyModifiedProperties();

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
            gameObject.transform.position = GetSnappedPosition(gameObject.transform.position);
        }
    }

    Vector3 GetSnappedPosition(Vector3 originalPosition)
    {
        if (gridType == GridType.Cartesian)
        {
            return originalPosition.Round(gridSize);
        }
        else if (gridType == GridType.Polar)
        {
            Vector2 vector = new Vector2(originalPosition.x, originalPosition.z);
            float distance = vector.magnitude;
            float distanceSnapped = distance.Round(gridSize);
            float angularRadius = Mathf.Atan2(vector.y, vector.x);
            float angularTurns = angularRadius / Mathf.PI * 2;
            float angularTurnsSnapped = angularTurns.Round(1f / angularDivisions);
            float angularRadiusSnapped = angularTurnsSnapped * Mathf.PI * 2;
            Vector2 directionSnapped = new Vector2(Mathf.Cos(angularRadiusSnapped), Mathf.Sin(angularRadiusSnapped));
            Vector2 vectorSnapped = directionSnapped * distanceSnapped;
            return new Vector3(vectorSnapped.x, originalPosition.y, vectorSnapped.y);
        }
        return default;
    }
}
