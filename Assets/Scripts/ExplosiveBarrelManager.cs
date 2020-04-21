using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExplosiveBarrelManager : MonoBehaviour
{
    public static List<ExplosiveBarrel> m_Barrels = new List<ExplosiveBarrel>();
    public static void UpdateBarrelsColors()
    {
        foreach (ExplosiveBarrel barrel in m_Barrels)
        {
            barrel.TryApplyColor();
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Vector3 managerPosition = transform.position;
        foreach (ExplosiveBarrel barrel in m_Barrels)
        {
            if (barrel.m_BarrelType == null)
            {
                return;
            }
            Vector3 barrelPosition = barrel.transform.position;
            float halfHeight = (managerPosition.y - barrelPosition.y) * 0.5f;
            Vector3 offset = Vector3.up * halfHeight;
            Handles.DrawBezier(managerPosition, barrelPosition, managerPosition - offset, barrelPosition + offset, barrel.m_BarrelType.m_Color, EditorGUIUtility.whiteTexture, 1f);
        }
    }
    #endif
}
