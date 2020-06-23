using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaypointSystemEditor : EditorWindow
{
    [Header("Current Point")]
    public Vector3 m_PointCurrent = Vector3.zero;
    [Header("Temporary Points")]
    public List<Vector3> m_PointsTemporary = new List<Vector3>();
    [Header("Confirmed Points")]
    public List<Vector3> m_PointsConfirmed = new List<Vector3>();
    [Header("Waypoints")]
    public List<GameObject> m_Waypoints = new List<GameObject>();

    private SerializedObject m_SerializedObject;
    private SerializedProperty m_PropertyPointCurrent;
    private SerializedProperty m_PropertyPointsTemporary; 
    private SerializedProperty m_PropertyPointsConfirmed;
    private SerializedProperty m_PropertyWaypoints;
    private GameObject m_Waypoint;
    private string[] m_WaypointsAmount;
    private bool m_ShowPoints;
    private float m_SliderValue = 5f;
    private float m_SliderValueLeft = 1f;
    private float m_SliderValueRight = 10f;

    private const string k_Point = "m_PointCurrent";
    private const string k_Points = "m_PointsTemporary";
    private const string k_PointsConfirmed = "m_PointsConfirmed";
    private const string k_Waypoints = "m_Waypoints";
    private const string k_WaypointSystem = "WaypointSystem";
    private const string k_AddPoint = "AddPoint";
    private const string k_ConfirmPoint = "ConfirmPoint";
    private const string k_ClearPoints = "ClearPoints";
    private const string k_Waypoint = "Waypoint";
    private const string k_IncreaseWaypoints = "IncreaseWaypoints";
    private const string k_FillWaypoints = "Fill With Only\nActive Waypoints";
    private const string k_WaypointsAmount = "WaypointsAmount";
    private const string k_Underlines = "_______________";
    private const string k_NewLine = "\n";

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
        m_Waypoint = Resources.Load<GameObject>(k_Waypoint);
        m_WaypointsAmount = new string[] { k_WaypointsAmount + k_NewLine + m_SliderValueLeft.ToString() + k_Underlines + m_SliderValueRight.ToString() };
        m_Waypoints.Clear();
        foreach (PatrollerWaypoint waypoint in FindObjectsOfType<PatrollerWaypoint>())
        {
            m_Waypoints.Add(waypoint.gameObject);
            m_Waypoints.Reverse();
        } 
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
        Rect rectangle = new Rect(8f, 8f, 128f, 32f);
        if(GUI.Button(rectangle, k_AddPoint))
        {
            m_PointsTemporary.Add(m_PropertyPointCurrent.vector3Value);
            m_ShowPoints = true;
            SceneView.RepaintAll();
            Repaint();
        }
        rectangle.position -= Vector2.down * 40;
        if (GUI.Button(rectangle, k_ConfirmPoint))
        { 
            foreach (SerializedProperty propertyPoint in m_PropertyPointsTemporary)
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
                    currentWaypoint.GetComponent<PatrollerWaypoint>().SetIndex(i);
                    m_Waypoints.Add(currentWaypoint);
                    currentWaypoint.SetActive(true);
                }
            }
            m_PointsTemporary.Clear();
            m_ShowPoints = false;
            SceneView.RepaintAll();
            Repaint();
        }
        rectangle.position -= Vector2.down * 40;
        if (GUI.Button(rectangle, k_ClearPoints))
        {
            m_PointsConfirmed.Clear();
            m_PointsTemporary.Clear();
            foreach (GameObject waypoint in m_Waypoints)
            {
                waypoint.SetActive(false);
            }
            SceneView.RepaintAll();
            Repaint();
        }
        rectangle.position -= Vector2.down * 75;
        if (GUI.Button(rectangle, k_IncreaseWaypoints))
        {
            for (int i = 0; i < m_SliderValue; i++)
            {
                GameObject currentWaypoint = Instantiate(m_Waypoint, Vector3.zero, Quaternion.identity);
                m_Waypoints.Add(currentWaypoint);
                currentWaypoint.SetActive(false);
            }
            SceneView.RepaintAll();
            Repaint();
        }
        rectangle.position -= Vector2.down * 35;
        GUI.Toolbar(rectangle, 0, m_WaypointsAmount);
        rectangle.position -= Vector2.down * 30;
        m_SliderValue = GUI.HorizontalSlider(rectangle, m_SliderValue, m_SliderValueLeft, m_SliderValueRight);
        rectangle.position -= Vector2.down * 75;
        if (GUI.Button(rectangle, k_FillWaypoints))
        {
            m_Waypoints.Clear();
            foreach (PatrollerWaypoint waypoint in FindObjectsOfType<PatrollerWaypoint>())
            {
                m_Waypoints.Add(waypoint.gameObject);
                m_Waypoints.Reverse();
            }
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
