using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaypointSystemEditor : EditorWindow
{
    [SerializeField] public Patroller patroller;

    [Header("Current Point")]
    public Vector3 m_PointCurrent = Vector3.zero;
    [Header("Temporary Points")]
    public List<Vector3> m_PointsTemporary = new List<Vector3>();
    [Header("Confirmed Points")]
    public List<Vector3> m_PointsConfirmed = new List<Vector3>();
    [Header("Waypoints")]
    public List<GameObject> m_Waypoints = new List<GameObject>();

    public List<GameObject> Waypoints { get => m_Waypoints; }

    private SerializedObject m_SerializedObject;
    private SerializedProperty m_PropertyPointCurrent;
    private SerializedProperty m_PropertyPointsTemporary; 
    private SerializedProperty m_PropertyPointsConfirmed;
    private SerializedProperty m_PropertyWaypoints;
    private int m_ToolbarInt = 0;
    private string[] m_ToolbarStrings = new string[] { "Toolbar1", "Toolbar2", "Toolbar3" };
    private bool m_ShowPoints;
    private bool m_Value;
    private float m_SliderValue = 5f;
    private float m_SliderValueLeft = 0f;
    private float m_SliderValueRight = 10f;

    private const string k_Point = "m_PointCurrent";
    private const string k_Points = "m_PointsTemporary";
    private const string k_PointsConfirmed = "m_PointsConfirmed";
    private const string k_Waypoints = "m_Waypoints";
    private const string k_WaypointSystem = "WaypointSystem";
    private const string k_Toggle = "Toggle";
    private const string k_AddPoint = "AddPoint";
    private const string k_ConfirmPoint = "ConfirmPoint";
    private const string k_ResetPoints = "ResetPoints";

    [MenuItem("Tools/WaypointSystem")]
    public static void OpenWaypointSystem()
    {
        GetWindow<WaypointSystemEditor>(k_WaypointSystem);
    }

    private void OnEnable()
    {
        m_SerializedObject = new SerializedObject(this);
        m_PropertyPointCurrent = m_SerializedObject.FindProperty(k_Point);
        m_PropertyPointsTemporary = m_SerializedObject.FindProperty(k_Points);
        m_PropertyPointsConfirmed = m_SerializedObject.FindProperty(k_PointsConfirmed);
        m_PropertyWaypoints = m_SerializedObject.FindProperty(k_Waypoints);
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
        m_SerializedObject.Update();
        EditorGUILayout.PropertyField(m_PropertyPointCurrent);
        EditorGUILayout.PropertyField(m_PropertyPointsTemporary);
        EditorGUILayout.PropertyField(m_PropertyPointsConfirmed);
        EditorGUILayout.PropertyField(m_PropertyWaypoints);

        if (m_SerializedObject.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
        }

        // if you clicked left mouse button, in the editor window
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl(null);
            Repaint(); // repaint on the editor window UI
        }
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        if (m_ShowPoints)
        {
            foreach (SerializedProperty propertyPoint in m_PropertyPointsTemporary)
            {
                propertyPoint.vector3Value = Handles.PositionHandle(propertyPoint.vector3Value, Quaternion.identity);
            }
        }
        Handles.BeginGUI();
        Rect rectangle = new Rect(8f, 8f, 256f, 64f);
        m_ToolbarInt = GUI.Toolbar(rectangle, m_ToolbarInt, m_ToolbarStrings);
        rectangle.position -= Vector2.down * 75;
        rectangle.width *= 0.5f;
        if(GUI.Button(rectangle, k_AddPoint))
        {
            m_PointsTemporary.Add(m_PropertyPointCurrent.vector3Value);
            m_ShowPoints = true;
        }
        rectangle.position -= Vector2.down * 75;
        if (GUI.Button(rectangle, k_ConfirmPoint))
        { 
            foreach (Vector3 point in m_PointsTemporary)
            {
                m_PointsConfirmed.Add(point);
                bool found = false;
                foreach (GameObject waypoint in m_Waypoints)
                {
                    if (!waypoint.activeSelf)
                    {
                        found = true;
                        waypoint.transform.position = point;
                        waypoint.SetActive(true);
                        break;
                    }
                }
                if (found == false)
                {
                    GameObject waypoint = Resources.Load<GameObject>("Waypoint");
                    GameObject currentWaypoint = Instantiate(waypoint, point, Quaternion.identity);
                    m_Waypoints.Add(currentWaypoint);
                    currentWaypoint.SetActive(true);
                }
            }
            m_PointsTemporary.Clear();
            m_ShowPoints = false;
        }
        rectangle.position -= Vector2.down * 75;
        if (GUI.Button(rectangle, k_ResetPoints))
        {
            m_PointsConfirmed.Clear();
            foreach (GameObject waypoint in m_Waypoints)
            {
                waypoint.SetActive(false);
            }
        }
        rectangle.position -= Vector2.down * 75;
        if (GUI.Button(rectangle, "IncreaseWaypoints"))
        {
            GameObject waypoint = Resources.Load<GameObject>("Waypoint");
            for (int i = 0; i < m_SliderValue; i++)
            {
                GameObject currentWaypoint = Instantiate(waypoint, Vector3.zero, Quaternion.identity);
                m_Waypoints.Add(currentWaypoint);
                currentWaypoint.SetActive(false);
            }
        }
        rectangle.position -= Vector2.down * 75;
        m_SliderValue = GUI.HorizontalSlider(rectangle, m_SliderValue, m_SliderValueLeft, m_SliderValueRight);
        rectangle.position -= Vector2.down * 75;
        m_Value = GUI.Toggle(rectangle, m_Value, k_Toggle);
        Handles.EndGUI();
        m_SerializedObject.ApplyModifiedProperties();
    }
}
