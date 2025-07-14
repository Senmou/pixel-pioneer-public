using UnityEngine.Localization;
using MoreMountains.Feedbacks;
using UnityEngine;
using TMPro;

public class ArtifactCounter : MonoBehaviour, ITooltip
{
    public static ArtifactCounter Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _amountUI;
    [SerializeField] private MMF_Player _feedback;
    [SerializeField] private LocalizedString _tooltipTitle;
    [SerializeField] private LocalizedString _tooltipDescription;

    #region ITooltip
    public string TooltipTitle => _tooltipTitle.GetLocalizedString();
    public string TooltipDescription => _tooltipDescription.GetLocalizedString();
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI(false);
    }

    public void UpdateUI(bool playFeedback = true)
    {
        if (playFeedback)
            _feedback.PlayFeedbacks();
    }
}
