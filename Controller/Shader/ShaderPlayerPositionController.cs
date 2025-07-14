using System.Collections;
using UnityEngine;

public class ShaderPlayerPositionController : MonoBehaviour
{
    [SerializeField] private MaterialListSO _materialListSO;

    private void Start()
    {
        UndergroundController.Instance.OnUndergroundChanged += UndergroundController_OnUndergroundChanged;
    }

    private void OnDestroy()
    {
        UndergroundController.Instance.OnUndergroundChanged -= UndergroundController_OnUndergroundChanged;
    }

    private void Update()
    {
        if (Player.Instance == null)
            return;

        foreach (var material in _materialListSO.materials)
        {
            material.SetVector("_PlayerPos", Player.Instance.transform.position);
        }
    }

    private Coroutine _groundShaderWeightCo;
    private void UndergroundController_OnUndergroundChanged(object sender, bool e)
    {
        if (_groundShaderWeightCo != null)
            StopCoroutine(_groundShaderWeightCo);
        _groundShaderWeightCo = StartCoroutine(GroundShaderWeightCo(e ? 1f : 0f));
    }

    private IEnumerator GroundShaderWeightCo(float targetWeight)
    {
        var startWeight = _materialListSO.materials[0].GetFloat("_Weight");
        float timer = 0f;
        float timerMax = 1f;
        while (timer < timerMax)
        {
            timer += Time.deltaTime;
            var currentWeight = Mathf.Lerp(startWeight, targetWeight, timer / timerMax);

            foreach (var material in _materialListSO.materials)
            {
                material.SetFloat("_Weight", currentWeight);
            }
            yield return null;
        }
    }
}
