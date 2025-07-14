using UnityEngine;
using TMPro;

public class TutorialMove : BaseTutorial
{
    [SerializeField] private TextMeshProUGUI _textA;
    [SerializeField] private TextMeshProUGUI _textD;
    [SerializeField] private TextMeshProUGUI _textSpace;

    private int _pressedA;
    private int _pressedD;
    private int _pressedSpace;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            OnPressedA();

        if (Input.GetKeyDown(KeyCode.D))
            OnPressedD();

        if (Input.GetKeyDown(KeyCode.Space))
            OnPressedSpace();
    }

    public override bool CancelCondition()
    {
        var allPressed = _pressedA >= 1 && _pressedD >= 1 && _pressedSpace >= 1;
        int pressedKeyCount = _pressedA + _pressedD + _pressedSpace;

        return allPressed || pressedKeyCount >= 10;
    }

    private void OnPressedA()
    {
        _pressedA++;
        _textA.color = Color.green;

        if (_pressedA <= 1)
            TutorialController.Instance.PlayStepCompleteFeedback();
    }

    private void OnPressedD()
    {
        _pressedD++;
        _textD.color = Color.green;

        if (_pressedD <= 1)
            TutorialController.Instance.PlayStepCompleteFeedback();
    }

    private void OnPressedSpace()
    {
        _pressedSpace++;
        _textSpace.color = Color.green;

        if (_pressedSpace <= 1)
            TutorialController.Instance.PlayStepCompleteFeedback();
    }
}
