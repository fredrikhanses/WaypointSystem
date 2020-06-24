using TMPro;
using UnityEditor;
using UnityEngine;

public class PatrollerWaypoint : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TextMeshProUGUI;

    private string m_IndexString;
    private int m_IndexInt;

    public void SetIndex(int index)
    {
        m_IndexInt = index;
        m_IndexString = index.ToString();
        m_TextMeshProUGUI.text = m_IndexString;
    }

    public int GetIndex()
    {
        return m_IndexInt;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Handles.Label(transform.position, m_IndexString);
    }
}
