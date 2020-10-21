using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Graph<TNodeType, TEdgeType>
{
    public Graph()
    {
        Nodes = new List<Node<TNodeType>>();
        Edges = new List<Edge<TEdgeType, TNodeType>>();
    }

    public List<Node<TNodeType>> Nodes { get; private set; }
    public List<Edge<TEdgeType, TNodeType>> Edges { get; private set; }
}

[Serializable]
public class Node<TNodeType>
{
    public Node()
    {
        NodeColor = Color.white;
        Index = 0;
        IndexColor = Color.black;
    }

    public Color NodeColor { get; set; }
    public TNodeType Value { get; set; }
    public int Index { get; set; }
    public Color IndexColor { get; set; }
}

[Serializable]
public class Edge<TEdgeType, TNodeType>
{
    public Edge()
    {
        EdgeColor = Color.white;
    }

    public Color EdgeColor { get; set; }
    public TEdgeType Value { get; set; }
    public Node<TNodeType> From { get; set; }
    public Node<TNodeType> To { get; set; }
}
