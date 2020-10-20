using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointSystem))]
public class WaypointSystemEditor : Editor
{
    [Header("Current Point")]
    public Vector3 m_CurrentPoint = Vector3.zero;
    [Header("Current Points")]
    public List<Vector3> m_Points = new List<Vector3>();


    private SerializedObject m_SerializedObject;
    private SerializedProperty m_PropertyCurrentPoint;
    private SerializedProperty m_PropertyPoints;

    private const string k_CurrentPoint = "m_CurrentPoint";
    private const string k_Points = "m_Points";
    private const string k_AddPoint = "AddPoint";
    private const string k_ClearPoints = "ClearPoints";

    public override void OnInspectorGUI()
    {
        m_SerializedObject.Update();
        EditorGUILayout.PropertyField(m_PropertyCurrentPoint);    

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
        WaypointSystem waypointSystem = target as WaypointSystem;
        if (waypointSystem == null || waypointSystem.Graph == null)
        {
            return;
        }
        EditorGUILayout.LabelField($"Nodes: {waypointSystem.Graph.Nodes.Count}");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Edges: {waypointSystem.Graph.Edges.Count}");
        if (GUILayout.Button("Clear Edges"))
        {
            ClearEdges(waypointSystem);
        }
        EditorGUILayout.EndHorizontal();
        foreach (Node<Vector3> node in waypointSystem.Graph.Nodes)
        {
            EditorGUILayout.BeginHorizontal();
            node.Value = EditorGUILayout.Vector3Field("", node.Value);
            node.NodeColor = EditorGUILayout.ColorField(node.NodeColor);
            if (GUILayout.Button("Add Edge"))
            {
                AddEdge(waypointSystem, node);
            }
            EditorGUILayout.EndHorizontal();
            foreach (Edge<float, Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.From == node))
            {
                EditorGUILayout.BeginHorizontal();
                edge.Value = EditorGUILayout.FloatField(edge.Value);
                EditorGUILayout.LabelField("--------------------------->");
                EditorGUILayout.ColorField(edge.To.NodeColor);
                EditorGUILayout.EndHorizontal();
            }
        }
        if(GUILayout.Button("Add Node"))
        {
            waypointSystem.Graph.Nodes.Add(new Node<Vector3> { Value = Vector3.zero });
        }
    }

    private void ClearEdges(WaypointSystem waypointSystem)
    {
        waypointSystem.Graph.Edges.Clear();
    }

    private void AddEdge(WaypointSystem waypointSystem, Node<Vector3> node)
    {
        GenericMenu menu = new GenericMenu();
        foreach (Node<Vector3> targetNode in waypointSystem.Graph.Nodes.Where(n => n != node))
        {
            menu.AddItem(new GUIContent($"{targetNode.Value}"), false, () => 
            { 
                waypointSystem.Graph.Edges.Add(new Edge<float, Vector3>() 
                { 
                    Value = 1.0f, EdgeColor = Color.white, From = node, To = targetNode 
                }); 
            });
        }
        menu.ShowAsContext();
    }

    private void OnSceneGUI()
    {
        WaypointSystem waypointSystem = target as WaypointSystem;
        if (waypointSystem == null || waypointSystem.Graph == null)
        {
            return;
        }
        foreach (Node<Vector3> node in waypointSystem.Graph.Nodes)
        {
            m_PropertyCurrentPoint.vector3Value = Handles.PositionHandle(m_PropertyCurrentPoint.vector3Value, Quaternion.identity);
            node.Value = m_PropertyCurrentPoint.vector3Value;
            Handles.color = node.NodeColor;
            if (Event.current.type == EventType.Repaint)
            {
                Handles.SphereHandleCap(0, node.Value, Quaternion.identity, 0.125f, EventType.Repaint);
            }
        }
        foreach (Edge<float, Vector3> edge in waypointSystem.Graph.Edges)
        {
            Handles.color = edge.EdgeColor; 
            Handles.DrawLine(edge.To.Value, edge.From.Value);
            if (Event.current.type == EventType.Repaint)
            {
                Vector3 distance = edge.To.Value - edge.From.Value;
                Handles.ArrowHandleCap(0, edge.To.Value - distance.normalized, Quaternion.LookRotation(distance), 0.5f, EventType.Repaint);
            }
        }
        EditorUtility.SetDirty(waypointSystem);
        if (m_SerializedObject.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
            Repaint();
        }
    }

    private void OnEnable()
    {
        m_SerializedObject = new SerializedObject(this);
        m_PropertyCurrentPoint = m_SerializedObject.FindProperty(k_CurrentPoint);
        m_PropertyPoints = m_SerializedObject.FindProperty(k_Points);
        Selection.selectionChanged += Repaint;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
    }
}
