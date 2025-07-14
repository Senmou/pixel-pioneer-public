using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu()]
public class BuildableRecipeSO : BaseRecipeSO
{
    [PropertyOrder(-1)][ShowInInspector][ReadOnly] public new string Id { get; private set; }
    public BaseBuildable prefab;
    public string buildableName;
}