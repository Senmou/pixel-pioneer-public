using UnityEngine;
using TMPro;

public class TutorialBuildMenu : BaseTutorial
{
    [SerializeField] private TextMeshProUGUI _text;

    private bool _pressedQ;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnPressedQ();
        }
    }

    public override bool CancelCondition()
    {
        return _pressedQ;
    }

    private void OnPressedQ()
    {
        _pressedQ = true;
        _text.color = Color.green;
    }
}
