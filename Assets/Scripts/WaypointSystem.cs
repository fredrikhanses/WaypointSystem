using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointSystem : MonoBehaviour
{
    [SerializeField] private Graph<Vector3, float> graph = new Graph<Vector3, float>();
    public Graph<Vector3, float> Graph => graph;
}
