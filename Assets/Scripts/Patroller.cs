using System.Collections.Generic;
using UnityEngine;

public interface IStartMoving
{
    void StartMoving(IEnumerable<Vector3> path);
}

[RequireComponent(typeof(Rigidbody))]
[SelectionBase]
public class Patroller : MonoBehaviour, IStartMoving
{
    [SerializeField, Range(1f, 100f)] private float m_Speed;
    [SerializeField] private Rigidbody m_Rigidbody;
    [SerializeField] public List<GameObject> m_Waypoints;

    private bool m_Move;
    private bool m_Stop;
    private float m_OldDistanceSquared = float.MaxValue;
    private Vector3 m_TargetPosition;
    private Vector3 m_Direction;
    private LinkedList<Vector3> m_Path;
    private LinkedList<Vector3> m_InitialPath;

    private const float k_MinDistanceToGoal = 0.01f;

    private void Start()
    {
        if (m_Rigidbody == null)
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }
        m_InitialPath = new LinkedList<Vector3>();
        m_InitialPath.AddLast(new Vector3(10f, 0f));
        m_InitialPath.AddLast(new Vector3(10f, 0f, -10f));
        m_InitialPath.AddLast(new Vector3(0f, 0f, -10f));
        m_InitialPath.AddLast(new Vector3(-10f, 0f, -10f));
        m_InitialPath.AddLast(new Vector3(-10f, 0f, 0f));
        m_InitialPath.AddLast(Vector3.zero);
        StartMoving(m_InitialPath);
    }

    private void FixedUpdate()
    {
        if (m_Move)
        {
            float distanceSquared = (transform.position - m_TargetPosition).sqrMagnitude;
            if (distanceSquared < (k_MinDistanceToGoal * k_MinDistanceToGoal) || (Mathf.Abs(distanceSquared) > Mathf.Abs(m_OldDistanceSquared)))
            {
                m_OldDistanceSquared = float.MaxValue;
                m_Rigidbody.velocity = Vector3.zero;
                transform.position = m_TargetPosition;
                m_Move = false;
                m_Stop = true;
            }
            else
            {
                m_OldDistanceSquared = distanceSquared;
            }
        }
        if (m_Stop)
        {
            m_Stop = false;
            MoveToNext();
        }
    }

    /// <summary> Start enemy movement along path.</summary>
    /// <param name="path"> Path to walk along.</param>
    public void StartMoving(IEnumerable<Vector3> path)
    {
        m_Path = new LinkedList<Vector3>(path);
        if (m_Path.Count > 0)
        {
            MoveTo(m_Path.First.Value);
            m_Path.RemoveFirst();
        }
    }

    private void MoveTo(Vector3 targetPosition)
    {
        m_TargetPosition = transform.position;
        m_Direction.x = GetDirection(m_TargetPosition.x, targetPosition.x);
        m_Direction.z = GetDirection(m_TargetPosition.z, targetPosition.z);
        Debug.Log(m_Direction);
        transform.rotation = Quaternion.LookRotation(m_Direction);
        m_TargetPosition = targetPosition;
        m_Rigidbody.velocity = m_Direction.normalized * m_Speed;
        m_Move = true;
    }

    //Calculate direction.
    private float GetDirection(float currentPosition, float targetPosition)
    {
        if (currentPosition > targetPosition)
        {
            return -Mathf.Sqrt(Mathf.Abs(targetPosition * targetPosition - currentPosition * currentPosition));
        }
        else if (Mathf.Abs(currentPosition) > Mathf.Abs(targetPosition))
        {
            return Mathf.Sqrt(Mathf.Abs(targetPosition * targetPosition - currentPosition * currentPosition));
        }
        else if (currentPosition == targetPosition)
        {
            return 0f;
        }
        else
        {
            return Mathf.Sqrt(targetPosition * targetPosition - currentPosition * currentPosition);
        }
    }

    private void MoveToNext()
    {
        if (m_Path.Count > 0)
        {
            MoveTo(m_Path.First.Value);
            m_Path.RemoveFirst();
        }
        else
        {
            StartMoving(m_InitialPath);
        }
    }
}
