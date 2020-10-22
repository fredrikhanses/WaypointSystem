using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointSystem)), CanEditMultipleObjects]
public class WaypointSystemEditor : Editor
{
    public Vector3 m_CurrentNode = Vector3.zero;
    public List<Vector3> m_Nodes = new List<Vector3>();

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
    private const string k_Length = "Length";

    public override void OnInspectorGUI()
    {
        m_SerializedObject.Update();
        WaypointSystem waypointSystem = target as WaypointSystem;
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
                EditorGUILayout.IntField(edge.From.Index);
                EditorGUILayout.LabelField(k_Empty);
                EditorGUILayout.IntField(edge.To.Index);
                if (GUILayout.Button(k_RemoveEdge))
                {
                    RemoveEdge(waypointSystem, edge);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.PropertyField(m_PropertyCurrentNode);
        EditorGUILayout.PropertyField(m_PropertyNodes, true);
        if (m_SerializedObject.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
        }
        if (waypointSystem != null)
        {
            EditorUtility.SetDirty(waypointSystem);
        }
    }

    private void OnSceneGUI()
    {
        m_SerializedObject.Update();
        WaypointSystem waypointSystem = target as WaypointSystem;
        if (waypointSystem == null || waypointSystem.Graph == null)
        {
            return;
        }
        foreach (Edge<float, Vector3> edge in waypointSystem.Graph.Edges)
        {
            Handles.color = edge.EdgeColor;
            Handles.DrawAAPolyLine(edge.To.Value, edge.From.Value);
        }
        foreach (Node<Vector3> node in waypointSystem.Graph.Nodes)
        {
            m_PropertyCurrentNode.vector3Value = node.Value;
            m_PropertyCurrentNode.vector3Value = Handles.PositionHandle(m_PropertyCurrentNode.vector3Value, Quaternion.identity);
            node.Value = m_PropertyCurrentNode.vector3Value;
            m_PropertyNodes.GetArrayElementAtIndex(node.Index).vector3Value = node.Value;
            if (Event.current.type == EventType.Repaint)
            {
                Handles.color = node.NodeColor;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.SphereHandleCap(0, node.Value, Quaternion.identity, 1.0f, EventType.Repaint);
            }
        }
        if (m_SerializedObject.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
        }
        if (waypointSystem != null)
        {
            EditorUtility.SetDirty(waypointSystem);
        }
    }

    private void OnEnable()
    {
        Tools.hidden = true;
        m_SerializedObject = new SerializedObject(this);
        m_PropertyCurrentNode = m_SerializedObject.FindProperty(k_CurrentNode);
        m_PropertyNodes = m_SerializedObject.FindProperty(k_Nodes);
        Selection.selectionChanged += Repaint;
        WaypointSystem waypointSystem = target as WaypointSystem;
        if (waypointSystem == null || waypointSystem.Graph == null)
        {
            return;
        }
        for (int i = 0; i < PlayerPrefs.GetInt(k_Length); i++)
        {
            Vector3 position = new Vector3(PlayerPrefs.GetFloat($"{i}x", 0.0f), PlayerPrefs.GetFloat($"{i}y", 0.0f), PlayerPrefs.GetFloat($"{i}z", 0.0f));
            AddNode(waypointSystem, position);
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt(k_Length, m_Nodes.Count);
        foreach (Vector3 node in m_Nodes)
        {
            PlayerPrefs.SetFloat($"{m_Nodes.IndexOf(node)}x", node.x);
            PlayerPrefs.SetFloat($"{m_Nodes.IndexOf(node)}y", node.y);
            PlayerPrefs.SetFloat($"{m_Nodes.IndexOf(node)}z", node.z);
        }
        Selection.selectionChanged -= Repaint;
        Tools.hidden = false;
        WaypointSystem waypointSystem = target as WaypointSystem;
        ClearNodes(waypointSystem);
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
        m_Nodes.Remove(node.Value);
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

    private void AddNode(WaypointSystem waypointSystem, Vector3 position = default)
    {
        Node<Vector3> lastNode = new Node<Vector3>();
        int nodeCount = waypointSystem.Graph.Nodes.Count;
        if (nodeCount > 0)
        {
            lastNode = waypointSystem.Graph.Nodes.Last();
            foreach (Edge<float, Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.From == lastNode))
            {
                RemoveEdge(waypointSystem, edge);
                break;
            }
        }
        if (position != default)
        {
            waypointSystem.Graph.Nodes.Add(new Node<Vector3> { Value = position, NodeColor = Color.red, Index = nodeCount });
        }
        else
        {
            waypointSystem.Graph.Nodes.Add(new Node<Vector3> { Value = new Vector3(lastNode.Value.x + 1.0f, lastNode.Value.y, lastNode.Value.y), NodeColor = Color.red, Index = nodeCount });
        }
        m_Nodes.Add(waypointSystem.Graph.Nodes.Last().Value);
        if (nodeCount + 1 > 1)
        {
            waypointSystem.Graph.Edges.Add(new Edge<float, Vector3>()
            {
                Value = 1.0f,
                EdgeColor = Color.white,
                From = lastNode,
                To = waypointSystem.Graph.Nodes.Last()
            });
        }
        if (nodeCount + 1 > 2)
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
