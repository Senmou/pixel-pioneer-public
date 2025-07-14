using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

public abstract class AbstractLootDropItem<T>
{
    public T item;
    public float probabilityWeight;
    [ReadOnly] public float probabilityPercent;

    [HideInInspector] public float probabilityRangeFrom;
    [HideInInspector] public float probabilityRangeTo;
}

[Serializable]
public abstract class AbstractLootDropTable<T, K> where T : AbstractLootDropItem<K>
{
    public List<T> lootDropItems;

    private float _totalWeight;
    private bool _isValidated;

    public void ValidateTable()
    {
        _isValidated = true;

        if (lootDropItems != null && lootDropItems.Count > 0)
        {
            float accWeight = 0f;

            foreach (T lootDropItem in lootDropItems)
            {
                if (lootDropItem.probabilityWeight < 0f)
                {
                    Debug.LogWarning("You can't have negative weight on an item. Resetting item's weight to 0.");
                    lootDropItem.probabilityWeight = 0f;
                }
                else
                {
                    lootDropItem.probabilityRangeFrom = accWeight;
                    accWeight += lootDropItem.probabilityWeight;
                    lootDropItem.probabilityRangeTo = accWeight;
                }
            }

            _totalWeight = accWeight;

            foreach (T lootDropItem in lootDropItems)
            {
                lootDropItem.probabilityPercent = (lootDropItem.probabilityWeight / _totalWeight) * 100;
            }
        }
    }

    public T PickLootDropItem()
    {
        if (!_isValidated)
            ValidateTable();

        float pickedNumber = UnityEngine.Random.Range(0f, _totalWeight);

        foreach (T lootDropItem in lootDropItems)
        {
            if (pickedNumber > lootDropItem.probabilityRangeFrom && pickedNumber < lootDropItem.probabilityRangeTo)
            {
                return lootDropItem;
            }
        }

        Debug.LogError("Item couldn't be picked. Make sure that all active loot drop tables contain at least one item");
        return null;
    }
}
