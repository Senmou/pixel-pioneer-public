using UnityEngine;

public class BuildingMaterialMenuController : MonoBehaviour
{
    public static BuildingMaterialMenuController Instance;

    [SerializeField] private BuildingMaterialMenu _prefab;

    private bool _isActive;
    private BuildingMaterialMenu _spawnedMenu;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(BaseBuilding baseBuilding, Vector3 position)
    {
        if (_isActive) return;

        _isActive = true;
        _spawnedMenu = Instantiate(_prefab, position, Quaternion.identity);
        _spawnedMenu.Show(baseBuilding);
    }

    public void Hide()
    {
        if (!_isActive) return;

        _isActive = false;
        _spawnedMenu.Hide();
    }
}
