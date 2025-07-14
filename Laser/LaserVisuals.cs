using UnityEngine;

public class LaserVisuals : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private GameObject _startVFX;
    [SerializeField] private GameObject _endVFX;
    [SerializeField] private Material _laserMaterial;

    [ColorUsage(true, true)][SerializeField] private Color _color0;
    [ColorUsage(true, true)][SerializeField] private Color _color1;
    [ColorUsage(true, true)][SerializeField] private Color _color2;

    private void Awake()
    {
        _lineRenderer.positionCount = 2;
    }

    public void UpdateMaterial(bool isUsingHyperLaser)
    {
        switch (LaserUpgradeController.Instance.HardnessLevel)
        {
            case 0:
                _laserMaterial.SetColor("_Color", _color0);
                _lineRenderer.startWidth = 0.2f;
                _lineRenderer.endWidth = 0.1f;
                break;
            case 1:
                _laserMaterial.SetColor("_Color", _color1);
                _lineRenderer.startWidth = 0.3f;
                _lineRenderer.endWidth = 0.1f;

                break;
            case 2:
                _laserMaterial.SetColor("_Color", _color2);
                _lineRenderer.startWidth = 0.5f;
                _lineRenderer.endWidth = 0.2f;
                break;
        }

        var miningSpeedBonus = LaserUpgradeController.Instance.MiningSpeedBonus;
        var lerp = Mathf.Lerp(0f, 1f, (miningSpeedBonus - 1f) / 3f);
        var glow = Mathf.Lerp(30f, 90f, lerp);
        _laserMaterial.SetFloat("_Glow", glow);

        var speed = Mathf.Lerp(-5f, -15f, lerp);
        _laserMaterial.SetVector("_LaserSpeed", new Vector2(speed, 0f));

        if (isUsingHyperLaser)
        {
            _lineRenderer.startWidth = 1.2f;
            _lineRenderer.endWidth = 1f;
            _laserMaterial.SetFloat("_Glow", 90f);
            _laserMaterial.SetVector("_LaserSpeed", new Vector2(-25, 0f));
        }
    }

    public void UpdatePosition(Vector3 startPos, Vector3 endPos)
    {
        _lineRenderer.SetPosition(index: 0, transform.position);
        _lineRenderer.SetPosition(index: 1, endPos);

        _endVFX.transform.position = endPos;
    }

    public void Show(bool isUsingHyperLaser)
    {
        UpdateMaterial(isUsingHyperLaser);

        _lineRenderer.enabled = true;
        _startVFX.SetActive(true);
        _endVFX.SetActive(true);
    }

    public void Hide()
    {
        _lineRenderer.enabled = false;
        _startVFX.SetActive(false);
        _endVFX.SetActive(false);
    }
}
