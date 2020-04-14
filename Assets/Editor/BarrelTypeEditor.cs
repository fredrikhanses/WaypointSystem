using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(BarrelType))]
public class BarrelTypeEditor : Editor
{
    SerializedObject m_SerializedObject;
    SerializedProperty m_PropRadius;
    SerializedProperty m_PropDamage;
    SerializedProperty m_PropColor;
    private void OnEnable()
    {
        m_SerializedObject = serializedObject;
        m_PropRadius = m_SerializedObject.FindProperty("m_Radius");
        m_PropDamage = m_SerializedObject.FindProperty("m_Damage");
        m_PropColor = m_SerializedObject.FindProperty("m_Color");
    }

    public override void OnInspectorGUI()
    {
        m_SerializedObject.Update();
        EditorGUILayout.PropertyField(m_PropRadius);
        EditorGUILayout.PropertyField(m_PropDamage);
        EditorGUILayout.PropertyField(m_PropColor);
        if(m_SerializedObject.ApplyModifiedProperties())
        {
            ExplosiveBarrelManager.UpdateBarrelsColors();
        }

    }
}
