using UnityEngine;

[CreateAssetMenu]
public class BarrelType : ScriptableObject
{
    [Range(1f, 8f)]
    public float m_Radius;
    public float m_Damage;
    public Color m_Color;
}
