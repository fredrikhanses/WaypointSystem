using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointSystem))]
public class WaypointSystemEditor : Editor
{
    [SerializeField] private Vector3 m_CurrentNode = Vector3.zero;
    private List<Vector3> m_Nodes = new List<Vector3>();
    private WaypointSystem waypointSystem;

    private SerializedObject m_SerializedObject;
    private SerializedProperty m_PropertyCurrentNode;
    private SerializedProperty m_PropertyNodes;

    private const string k_CurrentNode = "m_CurrentNode";
    private const string k_Nodes = "m_Nodes";
    private const string k_AddNode = "+ Node";
    private const string k_AddEdge = "+ Edge";
    private const string k_ClearNodes = "Clear Nodes";
    private const string k_ClearEdges = "Clear Edges";
    private const string k_RemoveNode = "- Node";
    private const string k_RemoveEdge = "- Edge";
    private const string k_Empty = "";

    public override void OnInspectorGUI()
    {
        WaypointSystem waypointSystem = target as WaypointSystem;
        m_SerializedObject.Update();
        EditorGUILayout.PropertyField(m_PropertyCurrentNode);
        if (m_SerializedObject.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
        }
        if (waypointSystem == null || waypointSystem.Graph == null)
        {
            return;
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Nodes: {waypointSystem.Graph.Nodes.Count}");
        if (GUILayout.Button(k_ClearNodes))
        {
            ClearNodes(waypointSystem);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Edges: {waypointSystem.Graph.Edges.Count}");
        if (GUILayout.Button(k_ClearEdges))
        {
            ClearEdges(waypointSystem);
        }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button(k_AddNode))
        {
            AddNode(waypointSystem);
        }
        if (GUILayout.Button(k_RemoveNode))
        {
            if (waypointSystem.Graph.Nodes.Count > 0)
            {
                RemoveNode(waypointSystem, waypointSystem.Graph.Nodes.Last());
            }
        }
        foreach (Node<Vector3> node in waypointSystem.Graph.Nodes)
        {
            EditorGUILayout.BeginHorizontal();
            node.Value = EditorGUILayout.Vector3Field(k_Empty, node.Value);
            node.NodeColor = EditorGUILayout.ColorField(node.NodeColor);
            if (GUILayout.Button(k_AddEdge))
            {
                AddEdge(waypointSystem, node);
            }
            EditorGUILayout.EndHorizontal();
            foreach (Edge<float, Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.From == node))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(k_Empty);
                //edge.Value = EditorGUILayout.FloatField(edge.Value);
                EditorGUILayout.ColorField(edge.To.NodeColor);
                if (GUILayout.Button(k_RemoveEdge))
                {
                    RemoveEdge(waypointSystem, edge);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            //EditorGUILayout.BeginHorizontal();
            //if (GUILayout.Button(k_RemoveNode))
            //{
            //    RemoveNode(waypointSystem, node);
            //    break;
            //}
            //EditorGUILayout.LabelField(k_Empty);
            //EditorGUILayout.EndHorizontal();
        }
    }

    private void OnSceneGUI()
    {
        waypointSystem = target as WaypointSystem;
        if (waypointSystem == null || waypointSystem.Graph == null)
        {
            return;
        }
        foreach (Edge<float, Vector3> edge in waypointSystem.Graph.Edges)
        {
            Handles.color = edge.EdgeColor;
            Handles.DrawAAPolyLine(edge.To.Value, edge.From.Value);
            //if (Event.current.type == EventType.Repaint)
            //{
            //    Vector3 distance = edge.To.Value - edge.From.Value;
            //    Handles.ArrowHandleCap(0, distance * 0.3f, Quaternion.LookRotation(distance), Mathf.Clamp(distance.magnitude, 0.1f, 1.0f), EventType.Repaint);
            //}
        }
        foreach (Node<Vector3> node in waypointSystem.Graph.Nodes)
        {
            m_PropertyCurrentNode.vector3Value = node.Value;
            m_PropertyCurrentNode.vector3Value = Handles.PositionHandle(m_PropertyCurrentNode.vector3Value, Quaternion.identity);
            node.Value = m_PropertyCurrentNode.vector3Value;
            if (Event.current.type == EventType.Repaint)
            {
                Handles.color = node.NodeColor;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.SphereHandleCap(0, node.Value, Quaternion.identity, 1.0f, EventType.Repaint);
            }
        }
    }

    private void OnValidate()
    {
        waypointSystem = target as WaypointSystem;
        if (waypointSystem != null)
        {
            EditorUtility.SetDirty(waypointSystem);
        }
        if (m_SerializedObject.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
            Repaint();
        }
    }

    private void OnEnable()
    {
        Tools.hidden = true;
        m_SerializedObject = new SerializedObject(this);
        m_PropertyCurrentNode = m_SerializedObject.FindProperty(k_CurrentNode);
        m_PropertyNodes = m_SerializedObject.FindProperty(k_Nodes);
        Selection.selectionChanged += Repaint;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        Tools.hidden = false;
    }

    private void ClearNodes(WaypointSystem waypointSystem)
    {
        waypointSystem.Graph.Nodes.Clear();
        ClearEdges(waypointSystem);
        m_Nodes.Clear();
    }

    private void ClearEdges(WaypointSystem waypointSystem)
    {
        waypointSystem.Graph.Edges.Clear();
    }

    private void RemoveEdge(WaypointSystem waypointSystem, Edge<float, Vector3> edge)
    {
        waypointSystem.Graph.Edges.Remove(edge);
    }

    private void RemoveNode(WaypointSystem waypointSystem, Node<Vector3> node)
    {   
        foreach (Edge<float, Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.From == node))
        {
            RemoveEdge(waypointSystem, edge);
            break;
        }
        foreach (Edge<float, Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.To == node))
        {
            RemoveEdge(waypointSystem, edge);
            break;
        }
        waypointSystem.Graph.Nodes.Remove(node);
        if (waypointSystem.Graph.Nodes.Count > 0)
        {
            waypointSystem.Graph.Edges.Add(new Edge<float, Vector3>()
            {
                Value = 1.0f,
                EdgeColor = Color.white,
                From = waypointSystem.Graph.Nodes.Last(),
                To = waypointSystem.Graph.Nodes.First()
            });
        }
    }

    private void AddNode(WaypointSystem waypointSystem)
    {
        Node<Vector3> lastNode = new Node<Vector3>();
        if (waypointSystem.Graph.Nodes.Count > 0)
        {
            lastNode = waypointSystem.Graph.Nodes.Last();
            foreach (Edge<float, Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.From == lastNode))
            {
                RemoveEdge(waypointSystem, edge);
                break;
            }
        }
        waypointSystem.Graph.Nodes.Add(new Node<Vector3> { Value = Vector3.zero, NodeColor = Color.red });
        if (waypointSystem.Graph.Nodes.Count > 1)
        {
            waypointSystem.Graph.Edges.Add(new Edge<float, Vector3>()
            {
                Value = 1.0f,
                EdgeColor = Color.white,
                From = lastNode,
                To = waypointSystem.Graph.Nodes.Last()
            });
        }
        if (waypointSystem.Graph.Nodes.Count > 2)
        {
            waypointSystem.Graph.Edges.Add(new Edge<float, Vector3>()
            {
                Value = 1.0f,
                EdgeColor = Color.white,
                From = waypointSystem.Graph.Nodes.Last(),
                To = waypointSystem.Graph.Nodes.First()
            });
        }
        m_Nodes.Add(Vector3.zero);
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
                    Value = 1.0f,
                    EdgeColor = Color.white,
                    From = node,
                    To = targetNode
                });
            });
        }
        menu.ShowAsContext();
    }
}
