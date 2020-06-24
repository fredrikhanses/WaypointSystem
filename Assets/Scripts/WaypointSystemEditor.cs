using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WaypointSystemEditor : EditorWindow
{
    [Header("Spawn Point")]
    public Vector3 m_PointSpawn = Vector3.zero;
    [Header("Current Points")]
    public List<Vector3> m_PointsCurrent = new List<Vector3>();
    [Header("Confirmed Points")]
    public List<Vector3> m_PointsConfirmed = new List<Vector3>();
    [Header("Waypoints")]
    public List<GameObject> m_Waypoints = new List<GameObject>();

    private SerializedObject m_SerializedObject;
    private SerializedProperty m_PropertyPointSpawn;
    private SerializedProperty m_PropertyPointsCurrent; 
    private SerializedProperty m_PropertyPointsConfirmed;
    private SerializedProperty m_PropertyWaypoints;
    private GameObject m_Waypoint;
    private Vector2 scrollPosition = Vector2.zero;
    private bool m_ShowPoints;
    private float m_PointsScrollScaler = 40f;
    private float m_WaypointsScrollScalar = 20f;

    private const string k_Point = "m_PointSpawn";
    private const string k_Points = "m_PointsCurrent";
    private const string k_PointsConfirmed = "m_PointsConfirmed";
    private const string k_Waypoints = "m_Waypoints";
    private const string k_WaypointSystem = "WaypointSystem";
    private const string k_AddPoint = "AddPoint";
    private const string k_ConfirmPoint = "ConfirmPoint";
    private const string k_ClearPoints = "ClearPoints";
    private const string k_Waypoint = "Waypoint";
    private const string k_CreatedWaypoint = "Created Waypoint";

    [MenuItem("Tools/WaypointSystem")]
    public static void OpenWaypointSystem()
    {
        GetWindow<WaypointSystemEditor>(k_WaypointSystem);
    }

    private void OnEnable()
    {
        m_SerializedObject = new SerializedObject(this);
        m_PropertyPointSpawn = m_SerializedObject.FindProperty(k_Point);
        m_PropertyPointsCurrent = m_SerializedObject.FindProperty(k_Points);
        m_PropertyPointsConfirmed = m_SerializedObject.FindProperty(k_PointsConfirmed);
        m_PropertyWaypoints = m_SerializedObject.FindProperty(k_Waypoints);
        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;
        Undo.undoRedoPerformed += OnCreatedObjectUndo;
        m_Waypoint = Resources.Load<GameObject>(k_Waypoint);
        m_Waypoints.Clear();
        m_PointsConfirmed.Clear();
        foreach (PatrollerWaypoint waypoint in FindObjectsOfType<PatrollerWaypoint>())
        {
            m_Waypoints.Add(waypoint.gameObject);
            m_PointsConfirmed.Add(waypoint.transform.position);
        }
        m_Waypoints.Reverse();
        m_PointsConfirmed.Reverse();
    }

    private void OnCreatedObjectUndo()
    {
        PatrollerWaypoint[] waypoints= FindObjectsOfType<PatrollerWaypoint>();
        int waypointsCount = waypoints.Count();
        if (waypointsCount > 0)
        {
            if (m_Waypoints.Count != waypointsCount)
            {
                m_Waypoints.Clear();
                foreach (PatrollerWaypoint waypoint in waypoints)
                {
                    m_Waypoints.Add(waypoint.gameObject);
                }
                m_Waypoints.Reverse();
            }
            if (m_PointsConfirmed.Count != waypointsCount)
            {
                m_PointsConfirmed.Clear();
                foreach (PatrollerWaypoint waypoint in waypoints)
                {
                    m_PointsConfirmed.Add(waypoint.transform.position);
                }
                m_PointsConfirmed.Reverse();
            }
        }
        else
        {
            m_Waypoints.Clear();
            m_PointsConfirmed.Clear();
        }
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void OnGUI()
    {
        float windowX = GetWindow<WaypointSystemEditor>(k_WaypointSystem).maxSize.x;
        float windowY = GetWindow<WaypointSystemEditor>(k_WaypointSystem).maxSize.y;
        Rect window = new Rect(0, 0, windowX, windowY);
        float pointsTemporaryHeight = m_PointsCurrent.Count * m_PointsScrollScaler;
        float pointsConfirmedHeight = m_PointsConfirmed.Count * m_PointsScrollScaler;
        float waypointsHeight = m_Waypoints.Count * m_WaypointsScrollScalar;
        float combinedHeight = pointsTemporaryHeight + pointsConfirmedHeight + waypointsHeight;
        Rect windowScroll = new Rect(0, 0, windowX, windowY + combinedHeight);
        scrollPosition = GUI.BeginScrollView(window, scrollPosition, windowScroll);

        m_SerializedObject.Update();
        EditorGUILayout.PropertyField(m_PropertyPointSpawn);
        EditorGUILayout.PropertyField(m_PropertyPointsCurrent);
        EditorGUILayout.PropertyField(m_PropertyPointsConfirmed);
        EditorGUILayout.PropertyField(m_PropertyWaypoints);

        GUI.EndScrollView();

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
            foreach (SerializedProperty propertyPoint in m_PropertyPointsCurrent)
            {
                propertyPoint.vector3Value = Handles.PositionHandle(propertyPoint.vector3Value, Quaternion.identity);
            }
        }
        Handles.BeginGUI();
        Rect rectangle = new Rect(8f, 8f, 128f, 32f);
        if(GUI.Button(rectangle, k_AddPoint))
        {
            m_PointsCurrent.Add(m_PropertyPointSpawn.vector3Value);
            m_ShowPoints = true;
            SceneView.RepaintAll();
            Repaint();
        }
        rectangle.position -= Vector2.down * 40;
        if (GUI.Button(rectangle, k_ConfirmPoint))
        { 
            foreach (SerializedProperty propertyPoint in m_PropertyPointsCurrent)
            {
                m_PointsConfirmed.Add(propertyPoint.vector3Value);
                bool found = false;
                int i = 0;
                foreach (GameObject waypoint in m_Waypoints)
                {
                    if (!waypoint.activeSelf)
                    {
                        found = true;
                        waypoint.transform.position = propertyPoint.vector3Value;
                        waypoint.GetComponent<PatrollerWaypoint>().SetIndex(i);
                        waypoint.SetActive(true);
                        break;
                    }
                    i++;
                }
                if (found == false)
                {
                    GameObject currentWaypoint = Instantiate(m_Waypoint, propertyPoint.vector3Value, Quaternion.identity);
                    Undo.RegisterCreatedObjectUndo(currentWaypoint, k_CreatedWaypoint);
                    currentWaypoint.GetComponent<PatrollerWaypoint>().SetIndex(i);
                    m_Waypoints.Add(currentWaypoint);
                    currentWaypoint.SetActive(true);
                }
            }
            m_PointsCurrent.Clear();
            m_ShowPoints = false;
            SceneView.RepaintAll();
            Repaint();
        }
        rectangle.position -= Vector2.down * 40;
        if (GUI.Button(rectangle, k_ClearPoints))
        {
            m_PointsConfirmed.Clear();
            m_PointsCurrent.Clear();
            foreach (GameObject waypoint in m_Waypoints)
            {
                Undo.DestroyObjectImmediate(waypoint);
            }
            m_Waypoints.Clear();
            SceneView.RepaintAll();
            Repaint();
        }
        Handles.EndGUI();
        if (m_SerializedObject.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
            Repaint();
        }
    }
}
