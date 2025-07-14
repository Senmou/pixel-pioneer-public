using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class AssetLoader<T> : MonoBehaviour where T : Object
{
    private AsyncOperationHandle<IList<T>> _loadHandle;

    public async Task<List<T>> LoadAssets(string label)
    {
        try
        {
            var assetList = new List<T>();
            _loadHandle = Addressables.LoadAssetsAsync<T>(
                new List<string> { label },
                addressable =>
                {
                    assetList.Add(addressable);
                }, Addressables.MergeMode.Union,
                false);

            await _loadHandle.Task;
            return assetList;
        }
        catch (InvalidKeyException e)
        {
            Debug.LogError(e.Message);
            return null;
        }
    }

    private void OnDestroy()
    {
        if (_loadHandle.IsValid())
            Addressables.Release(_loadHandle);
    }
}