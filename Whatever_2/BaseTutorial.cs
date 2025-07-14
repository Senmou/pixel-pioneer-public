using UnityEngine;

public class BaseTutorial : MonoBehaviour
{
    [SerializeField] private TutorialSO _tutorialSO;
    [SerializeField] private CanvasGroup _canvasGroup;

    public CanvasGroup CanvasGroup => _canvasGroup;
    public TutorialSO TutorialSO => _tutorialSO;

    private void Awake()
    {
        _canvasGroup.alpha = 0f;
    }

    public virtual void OnFinish()
    {

    }

    public virtual bool CancelCondition()
    {
        return false;
    }
}
