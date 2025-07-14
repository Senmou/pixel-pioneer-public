using System.Collections.Generic;
using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;
using QFSW.QC;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance { get; private set; }

    [SerializeField] private Canvas _canvas;

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player _showFeedback;
    [SerializeField] private MMF_Player _hideFeedback;
    [SerializeField] private MMF_Player _stepCompleteFeedback;

    [Header("Tutorial SO")]
    [SerializeField] private TutorialSO _tutorialMoveSO;
    [SerializeField] private TutorialSO _tutorialMiningSO;
    [SerializeField] private TutorialSO _tutorialBuildMenuSO;
    [SerializeField] private TutorialSO _tutorialThrowItemsSO;
    [SerializeField] private TutorialSO _tutorialCableSO;
    [SerializeField] private TutorialSO _tutorialArtifact;

    private Queue<BaseTutorial> _tutorialQueue = new Queue<BaseTutorial>();

    private void Awake()
    {
        Instance = this;
    }

    private void StartTutotrial()
    {
        //if (!PlayerPrefs.HasKey(_tutorialMoveSO.id))
        //{
        //    ShowTutorial_Move();
        //    ShowTutorial_Mining();
        //}
    }

    private IEnumerator StartTutorialsAfterIntro()
    {
        while (IntroController.Instance.IntroInAction)
        {
            yield return null;
        }

        StartTutotrial();
    }

    private void Update()
    {
        if (_tutorialQueue.Count == 0)
            return;

        if (_tutorialQueue.Peek().gameObject.activeSelf && _tutorialQueue.Peek().CancelCondition())
            FinishCurrentTutorial();
    }

    public void ShowTutorial_Move()
    {
        EnqueueTutorial(_tutorialMoveSO);
    }

    public void ShowTutorial_Mining()
    {
        EnqueueTutorial(_tutorialMiningSO);
    }

    public void ShowTutorial_BuildMenu()
    {
        EnqueueTutorial(_tutorialBuildMenuSO);
        BuildingController.Instance.OnBuildingPlaced += BuildingController_OnBuildingPlaced;
    }

    public void ShowTutorial_Cable()
    {
        EnqueueTutorial(_tutorialCableSO);
    }

    public void ShowTutorial_Artifact()
    {
        EnqueueTutorial(_tutorialArtifact);
    }

    private void BuildingController_OnBuildingPlaced(object sender, Placeable e)
    {
        BuildingController.Instance.OnBuildingPlaced -= BuildingController_OnBuildingPlaced;
        ShowTutorial_ThrowItems();
    }

    public void ShowTutorial_ThrowItems()
    {
        EnqueueTutorial(_tutorialThrowItemsSO);
    }

    private bool EnqueueTutorial(TutorialSO tutorialSO)
    {
        bool enqueued = false;
        if (PlayerPrefs.HasKey(tutorialSO.id))
            return enqueued;

        var tutorial = Instantiate(tutorialSO.prefab, _canvas.transform);
        tutorial.gameObject.SetActive(false);
        _tutorialQueue.Enqueue(tutorial);

        enqueued = true;

        if (_tutorialQueue.Count == 1)
            ShowTutorial(_tutorialQueue.Peek());

        return enqueued;
    }

    private void ShowTutorial(BaseTutorial tutorial)
    {
        PlayerPrefs.SetInt(tutorial.TutorialSO.id, 1);

        var feedback = _showFeedback.GetFeedbackOfType<MMF_CanvasGroup>();
        tutorial.gameObject.SetActive(true);
        feedback.TargetCanvasGroup = tutorial.CanvasGroup;

        var positionFeedback = _showFeedback.GetFeedbackOfType<MMF_Position>();
        positionFeedback.AnimatePositionTarget = tutorial.gameObject;

        _showFeedback.Initialization();
        _showFeedback.PlayFeedbacks();
    }

    private void FinishCurrentTutorial()
    {
        var currentTutorial = _tutorialQueue.Dequeue();

        currentTutorial.OnFinish();

        var canvasGroupFeedback = _hideFeedback.GetFeedbackOfType<MMF_CanvasGroup>();
        canvasGroupFeedback.TargetCanvasGroup = currentTutorial.CanvasGroup;

        var destroyFeedback = _hideFeedback.GetFeedbackOfType<MMF_Destroy>();
        destroyFeedback.TargetGameObject = currentTutorial.gameObject;

        var positionFeedback = _hideFeedback.GetFeedbackOfType<MMF_Position>();
        positionFeedback.AnimatePositionTarget = currentTutorial.gameObject;

        _hideFeedback.Initialization();
        _hideFeedback.PlayFeedbacks();
    }

    public void OnFeedbackHideFinished()
    {
        Invoke("ShowNextTutorial", 1f);
    }

    private void ShowNextTutorial()
    {
        if (_tutorialQueue.Count > 0)
        {
            ShowTutorial(_tutorialQueue.Peek());
        }
    }

    public void PlayStepCompleteFeedback()
    {
        _stepCompleteFeedback.PlayFeedbacks();
    }

    [Command("reset_tutorial")]
    public void ResetTutorialPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(_tutorialMoveSO.id);
        PlayerPrefs.DeleteKey(_tutorialMiningSO.id);
        PlayerPrefs.DeleteKey(_tutorialBuildMenuSO.id);
        PlayerPrefs.DeleteKey(_tutorialThrowItemsSO.id);
        PlayerPrefs.DeleteKey(_tutorialCableSO.id);
        PlayerPrefs.DeleteKey(_tutorialArtifact.id);
    }

    [Command("restart_tutorials")]
    public void RestartTutorials()
    {
        ResetTutorialPlayerPrefs();
        ShowTutorial_Move();
        ShowTutorial_Mining();
    }
}
