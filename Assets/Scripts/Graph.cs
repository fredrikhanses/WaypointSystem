using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Graph<TNodeType, TEdgeType>
{
    public Graph()
    {
        Nodes = new List<Node<TNodeType>>();
        Edges = new List<Edge<TNodeType>>();
    }

    public List<Node<TNodeType>> Nodes { get; private set; }
    public List<Edge<TNodeType>> Edges { get; private set; }
}

[Serializable]
public class Node<TNodeType>
{
    public Node()
    {
        NodeColor = Color.white;
        Index = 0;
    }

    public Color NodeColor { get; set; }
    public TNodeType Value { get; set; }
    public int Index { get; set; }
}

[Serializable]
public class Edge<TNodeType>
{
    public Edge()
    {
        EdgeColor = Color.white;
    }

    public Color EdgeColor { get; set; }
    public Node<TNodeType> From { get; set; }
    public Node<TNodeType> To { get; set; }
}
