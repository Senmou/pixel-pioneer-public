using UnityEngine;

[CreateAssetMenu(menuName = "Manager/Material Manager", fileName = "Material Manager")]
public class MaterialManagerSO : ScriptableObject
{
    [SerializeField] private Material _dirtMaterial;
    [SerializeField] private Material _stoneMaterial;

    public Material GetMaterial(BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.Dirt: return _dirtMaterial;
            case BlockType.Stone: return _stoneMaterial;
        }
        return _dirtMaterial;
    }
}
