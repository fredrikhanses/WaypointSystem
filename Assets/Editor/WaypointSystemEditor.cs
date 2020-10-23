using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointSystem)), CanEditMultipleObjects]
public class WaypointSystemEditor : Editor
{
    public List<Vector3> m_Nodes = new List<Vector3>();

    private SerializedObject m_SerializedObject;
    private SerializedProperty m_PropertyNodes;

    private const string k_Nodes = "m_Nodes";
    private const string k_AddNode = "+ Node";
    private const string k_ClearNodes = "Clear Nodes";
    private const string k_RemoveNode = "- Node";
    private const string k_Empty = "";
    private const string k_Arrow = "--------------------------->";
    private const string k_Length = "Length";

    public override void OnInspectorGUI()
    {
        m_SerializedObject.Update();
        WaypointSystem waypointSystem = target as WaypointSystem;
        if (waypointSystem == null || waypointSystem.Graph == null)
        {
            return;
        }
        if (GUILayout.Button(k_ClearNodes))
        {
            ClearNodes(waypointSystem);
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Nodes: {waypointSystem.Graph.Nodes.Count}");
        EditorGUILayout.LabelField($"Edges: {waypointSystem.Graph.Edges.Count}");
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
            EditorGUILayout.EndHorizontal();
            foreach (Edge<Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.From == node))
            {
                edge.EdgeColor = node.NodeColor;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.IntField(edge.From.Index);
                EditorGUILayout.LabelField(k_Arrow);
                EditorGUILayout.IntField(edge.To.Index);
                EditorGUILayout.EndHorizontal();
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

    private void OnSceneGUI()
    {
        m_SerializedObject.Update();
        WaypointSystem waypointSystem = target as WaypointSystem;
        if (waypointSystem == null || waypointSystem.Graph == null)
        {
            return;
        }
        foreach (Edge<Vector3> edge in waypointSystem.Graph.Edges)
        {
            Handles.color = edge.EdgeColor;
            Handles.DrawAAPolyLine(edge.To.Value, edge.From.Value);
        }
        for (int i = 0; i < m_PropertyNodes.arraySize; i++)
        {
            SerializedProperty property = m_PropertyNodes.GetArrayElementAtIndex(i);
            property.vector3Value = Handles.PositionHandle(property.vector3Value, Quaternion.identity);
            waypointSystem.Graph.Nodes.ElementAt(i).Value = property.vector3Value;
            if (Event.current.type == EventType.Repaint)
            {
                Handles.color = waypointSystem.Graph.Nodes.ElementAt(i).NodeColor;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.SphereHandleCap(0, waypointSystem.Graph.Nodes.ElementAt(i).Value, Quaternion.identity, 1.0f, EventType.Repaint);
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
            Color color = new Color(PlayerPrefs.GetFloat($"{i}r", 1.0f), PlayerPrefs.GetFloat($"{i}g", 1.0f), PlayerPrefs.GetFloat($"{i}b", 1.0f), PlayerPrefs.GetFloat($"{i}a", 1.0f));
            AddNode(waypointSystem, position, color, true);
        }
    }

    private void OnDisable()
    {
        WaypointSystem waypointSystem = target as WaypointSystem;
        if (waypointSystem == null || waypointSystem.Graph == null)
        {
            return;
        }
        PlayerPrefs.SetInt(k_Length, waypointSystem.Graph.Nodes.Count);
        foreach (Node<Vector3> node in waypointSystem.Graph.Nodes)
        {
            PlayerPrefs.SetFloat($"{node.Index}x", node.Value.x);
            PlayerPrefs.SetFloat($"{node.Index}y", node.Value.y);
            PlayerPrefs.SetFloat($"{node.Index}z", node.Value.z);
            PlayerPrefs.SetFloat($"{node.Index}r", node.NodeColor.r);
            PlayerPrefs.SetFloat($"{node.Index}g", node.NodeColor.g);
            PlayerPrefs.SetFloat($"{node.Index}b", node.NodeColor.b);
            PlayerPrefs.SetFloat($"{node.Index}a", node.NodeColor.a);
        }
        Selection.selectionChanged -= Repaint;
        Tools.hidden = false;
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

    private void RemoveEdge(WaypointSystem waypointSystem, Edge<Vector3> edge)
    {
        waypointSystem.Graph.Edges.Remove(edge);
    }

    private void RemoveNode(WaypointSystem waypointSystem, Node<Vector3> node)
    {   
        foreach (Edge<Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.From == node))
        {
            RemoveEdge(waypointSystem, edge);
            break;
        }
        foreach (Edge<Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.To == node))
        {
            RemoveEdge(waypointSystem, edge);
            break;
        }
        waypointSystem.Graph.Nodes.Remove(node);
        m_Nodes.Remove(node.Value);
        if (waypointSystem.Graph.Nodes.Count > 2)
        {
            waypointSystem.Graph.Edges.Add(new Edge<Vector3>()
            {
                EdgeColor = waypointSystem.Graph.Nodes.Last().NodeColor,
                From = waypointSystem.Graph.Nodes.Last(),
                To = waypointSystem.Graph.Nodes.First()
            });
        }
    }

    private void AddNode(WaypointSystem waypointSystem, Vector3 position = default, Color color = default, bool reappear = false)
    {
        Node<Vector3> lastNode = new Node<Vector3>();
        Node<Vector3> firstNode = new Node<Vector3>();
        int nodeCount = waypointSystem.Graph.Nodes.Count;
        if (nodeCount > 0)
        {
            lastNode = waypointSystem.Graph.Nodes.Last();
            firstNode = waypointSystem.Graph.Nodes.First();
            foreach (Edge<Vector3> edge in waypointSystem.Graph.Edges.Where(edge => edge.From == lastNode))
            {
                RemoveEdge(waypointSystem, edge);
                break;
            }
        }
        if (reappear)
        {
            waypointSystem.Graph.Nodes.Add(new Node<Vector3> { Value = position, NodeColor = color, Index = nodeCount });
        }
        else
        {
            waypointSystem.Graph.Nodes.Add(new Node<Vector3> { Value = Vector3.Lerp(firstNode.Value, lastNode.Value, 0.5f), NodeColor = Color.white, Index = nodeCount });
        }
        m_Nodes.Add(waypointSystem.Graph.Nodes.Last().Value);
        if (nodeCount + 1 > 1)
        {
            waypointSystem.Graph.Edges.Add(new Edge<Vector3>()
            {
                EdgeColor = lastNode.NodeColor,
                From = lastNode,
                To = waypointSystem.Graph.Nodes.Last()
            });
        }
        if (nodeCount + 1 > 2)
        {
            waypointSystem.Graph.Edges.Add(new Edge<Vector3>()
            {
                EdgeColor = waypointSystem.Graph.Nodes.Last().NodeColor,
                From = waypointSystem.Graph.Nodes.Last(),
                To = waypointSystem.Graph.Nodes.First()
            });
        }
    }
}
