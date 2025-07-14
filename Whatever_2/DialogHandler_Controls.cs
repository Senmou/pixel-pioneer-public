using MoreMountains.Tools;
using UnityEngine;

public class DialogHandler_Controls : MonoBehaviour, MMEventListener<LaserCannonShootEvent>
{
    public static DialogHandler_Controls Instance { get; private set; }

    [SerializeField] private DialogSO _controls;
    [SerializeField] private DialogEventSO _endLineEventControlsMoving;
    [SerializeField] private DialogEventSO _endLineEventControlsJumping;
    [SerializeField] private DialogEventSO _endLineEventControlsMining;

    private bool _pressedA;
    private bool _pressedD;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MMEventManager.AddListener(this);
    }

    private void OnDestroy()
    {
        MMEventManager.RemoveListener(this);
    }

    private void Update()
    {
        if (!_pressedA) _pressedA = Input.GetKeyDown(KeyCode.A);
        if (!_pressedD) _pressedD = Input.GetKeyDown(KeyCode.D);

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            _endLineEventControlsMoving.Trigger();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _endLineEventControlsJumping.Trigger();
        }
    }

    public void ShowDialog()
    {
        DialogController.Instance.EnqueueDialog(_controls);
    }

    public void OnMMEvent(LaserCannonShootEvent eventType)
    {
        _endLineEventControlsMining.Trigger();
    }
}
