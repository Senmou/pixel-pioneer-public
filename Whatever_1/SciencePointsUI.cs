using UnityEngine;
using System;
using TMPro;

public class SciencePointsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _pointsText;

    private void Start()
    {
        ScienceController.Instance.OnSciencePointsChanged += ScienceController_OnSciencePointsChanged;
    }

    private void ScienceController_OnSciencePointsChanged(object sender, EventArgs e)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        _pointsText.text = $"Science Points: {ScienceController.Instance.SciencePoints}";
    }
}
