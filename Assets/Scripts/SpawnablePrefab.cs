using UnityEditor;
using UnityEngine;

public class SpawnablePrefab : MonoBehaviour
{
    public float height = 1f;

    private void OnDrawGizmosSelected()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position + transform.up * height;
        Handles.DrawAAPolyLine(start, end);

        void DrawSphere(Vector3 point)
        {
            float handleSize = HandleUtility.GetHandleSize(point);
            Gizmos.DrawSphere(point, 0.1f);
        }

        DrawSphere(start);
        DrawSphere(end);
    }
}
