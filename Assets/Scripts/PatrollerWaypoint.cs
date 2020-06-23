using UnityEditor;
using UnityEngine;

public class PatrollerWaypoint : MonoBehaviour
{
    private string m_Index;

    public void SetIndex(int index)
    {
        m_Index = index.ToString();
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Handles.Label(transform.position, m_Index);
    }
}
