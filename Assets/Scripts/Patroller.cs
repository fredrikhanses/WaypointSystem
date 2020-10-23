using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[SelectionBase]
public class Patroller : MonoBehaviour
{
    [SerializeField, Range(1f, 20f)] private float m_Speed = 5f;
    [SerializeField, Range(0.01f, 1f)] private float m_DistancePrecision = 0.01f;
    [SerializeField, Range(0.001f, 0.01f)] private float m_RotationSpeed = 0.001f;
    [SerializeField, Range(1f, 10f)] private float m_RotationPrecision = 1f;
    [SerializeField] private bool m_ShowWaypoints = true;
    [SerializeField] private Rigidbody m_Rigidbody;
    [SerializeField] private WaypointSystem m_WaypointSystem;

    private bool m_Move;
    private bool m_Stop;
    private bool m_Rotate;
    private bool m_Next;
    private float m_OldDistanceSquared = float.MaxValue;
    private float m_Alpha;
    private float m_OldSpeed;
    private Vector3 m_TargetPosition;
    private Vector3 m_Direction;
    private Quaternion m_DirectionRotation;
    private LinkedList<Vector3> m_Path;
    private LinkedList<Vector3> m_InitialPath;
    private GameObject m_Waypoint;

    private const string k_Waypoint = "Waypoint";
    private const string k_Length = "Length";

    private void Awake()
    {
        m_OldSpeed = m_Speed;
        m_InitialPath = new LinkedList<Vector3>();
        if (m_Rigidbody == null)
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }
        if (m_ShowWaypoints)
        {
            m_Waypoint = Resources.Load<GameObject>(k_Waypoint);
        }
    }

    private void Start()
    {
        for (int i = 0; i < PlayerPrefs.GetInt(k_Length); i++)
        {
            Vector3 position = new Vector3(PlayerPrefs.GetFloat($"{i}x", 0.0f), PlayerPrefs.GetFloat($"{i}y", 0.0f), PlayerPrefs.GetFloat($"{i}z", 0.0f));
            if (m_ShowWaypoints)
            {
                Color color = new Color(PlayerPrefs.GetFloat($"{i}r", 1.0f), PlayerPrefs.GetFloat($"{i}g", 1.0f), PlayerPrefs.GetFloat($"{i}b", 1.0f), PlayerPrefs.GetFloat($"{i}a", 1.0f));
                PatrollerWaypoint patrollerWaypoint = Instantiate(m_Waypoint, position, Quaternion.identity).GetComponent<PatrollerWaypoint>();
                patrollerWaypoint.SetIndex(i); 
                patrollerWaypoint.SetColor(color);
            }
            m_InitialPath.AddLast(position);
        }
        StartMoving(m_InitialPath);
    }

    private void FixedUpdate()
    {
        if (m_InitialPath.Count == 0)
        {
            Start();
        }
        if (m_Move)
        {
            if (m_OldSpeed != m_Speed)
            {
                m_Rigidbody.velocity *= m_Speed / m_OldSpeed;
                m_OldSpeed = m_Speed;
            }
            float distanceSquared = (transform.position - m_TargetPosition).sqrMagnitude;
            if (distanceSquared < (m_DistancePrecision * m_DistancePrecision) || (Mathf.Abs(distanceSquared) > Mathf.Abs(m_OldDistanceSquared)))
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
        if(m_Stop)
        {
            m_Stop = false;
            CalculateDirection();
            m_Rotate = true;
        }
        if (m_Rotate)
        {
            m_Alpha += m_RotationSpeed;
            if ((transform.rotation.eulerAngles - m_DirectionRotation.eulerAngles).sqrMagnitude < m_RotationPrecision)
            {
                transform.rotation = m_DirectionRotation;
                m_Alpha = 0f;
                m_Rotate = false;
                m_Next = true;
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, m_DirectionRotation, m_Alpha);
            }
        }
        if (m_Next)
        {
            m_Next = false;
            MoveToNext();
        }
    }

    private void StartMoving(IEnumerable<Vector3> path)
    {
        m_Path = new LinkedList<Vector3>(path);
        if (m_Path.Count > 0)
        {
            m_Stop = true;
        }
    }

    private Vector3 CalculateDirection()
    {
        if (m_Path.Count > 0)
        {
            m_Direction = m_Path.First.Value - transform.position;
            m_DirectionRotation = Quaternion.LookRotation(m_Direction);
            return m_Direction;
        }
        else
        {
            m_Direction = m_InitialPath.First.Value - transform.position;
            m_DirectionRotation = Quaternion.LookRotation(m_Direction);
            return m_Direction;
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

    private void MoveTo(Vector3 targetPosition)
    {
        m_TargetPosition = targetPosition;
        m_Rigidbody.velocity = CalculateDirection().normalized * m_Speed;
        m_Move = true;
    }
}
