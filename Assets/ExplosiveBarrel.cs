using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class ExplosiveBarrel : MonoBehaviour
{
    public BarrelType m_BarrelType;

    private static readonly int m_ShaderPropertyColor = Shader.PropertyToID("_Color");
    
    private MaterialPropertyBlock m_MaterialPropertyBlock;
    public MaterialPropertyBlock MaterialPropertyBlock
    {
        get
        {
            if(m_MaterialPropertyBlock == null)
            {
                m_MaterialPropertyBlock = new MaterialPropertyBlock(); 
            }
            return m_MaterialPropertyBlock;
        }
    }
    
    public void TryApplyColor()
    {
        if (m_BarrelType == null)
        {
            return;
        }
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        MaterialPropertyBlock.SetColor(m_ShaderPropertyColor, m_BarrelType.m_Color);
        renderer.SetPropertyBlock(MaterialPropertyBlock);
    }

    private void OnValidate()
    {
        TryApplyColor();
    }

    private void OnEnable()
    {
        TryApplyColor();
        ExplosiveBarrelManager.m_Barrels.Add(this);
    }

    private void OnDisable()
    {
        ExplosiveBarrelManager.m_Barrels.Remove(this);
    }

    [ContextMenu("Do Something")]
    public void DoSomething()
    {

    }

    private void OnDrawGizmos()
    {
        if(m_BarrelType == null)
        {
            return;
        }
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Handles.color = m_BarrelType.m_Color;
        Handles.DrawWireDisc(transform.position, transform.up, m_BarrelType.m_Radius);
        Handles.color = Color.white;
    }
}
