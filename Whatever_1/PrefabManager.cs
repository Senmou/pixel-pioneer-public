using MoreMountains.Tools;
using UnityEngine;

public class PrefabManager : MMPersistentSingleton<PrefabManager>
{
    [SerializeField] private PrefabSO _prefabSO;

    public Tooltip Tooltip => _prefabSO.Tooltip;

    public PrefabSO Prefabs => _prefabSO;

    public BuildingRecipeSO GetBuildingRecipeById(string id)
    {
        return _prefabSO.GetBuildingRecipeSOById(id);
    }
}
