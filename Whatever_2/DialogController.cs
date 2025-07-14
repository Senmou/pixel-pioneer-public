using System.Collections.Generic;
using MoreMountains.Feedbacks;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using Febucci.UI;
using TMPro;

public class DialogController : MonoBehaviour
{
    public static DialogController Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private GameObject _textBox;
    [SerializeField] private TypewriterByCharacter _typeWriter;
    [SerializeField] private TextMeshProUGUI _progressUI;
    [SerializeField] private SimpleSlider _slider;
    [SerializeField] private MMF_Player _fullProgressFeedback;

    [Space(10)]
    [Header("Timings")]
    [SerializeField] private float _secondsPerLetter;
    [SerializeField] private float _minDurationPerLine;
    [SerializeField] private float _maxDurationPerLine;
    [SerializeField] private float _dialogDelay;
    [SerializeField] private float _hideTextDelay;
    [SerializeField] private float _sliderSpeed;

    private bool _isLineVisible;
    private bool _pressedReturn;
    private bool _isQueueBusy;
    private bool _shouldHideText;
    private float _hideTextTimer;
    private Queue<DialogSO> _dialogQueue = new();
    private Coroutine _sliderCo;

    private List<string> _shownDialogIdList = new();

    private void Awake()
    {
        Instance = this;

        _typeWriter.onTextShowed.AddListener(TypeWriter_OnTextShowed);

        HideTextBox();

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onSave += OnSave;
        saveComp.onLoad += OnLoad;
    }

    private void OnDestroy()
    {
        _typeWriter.onTextShowed.RemoveAllListeners();
    }

    private void TypeWriter_OnTextShowed()
    {
        _isLineVisible = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _pressedReturn = true;
        }

        if (_isQueueBusy || !_shouldHideText)
            return;

        _hideTextTimer += Time.deltaTime;
        if (_hideTextTimer >= _hideTextDelay)
        {
            _hideTextTimer = 0f;
            _shouldHideText = false;
            HideTextBox();
        }
    }

    public void EnqueueDialog(DialogSO dialog)
    {
        if (dialog.debug)
        {
            print("Dialog enqueued");
        }

        if (dialog.oneShot && _shownDialogIdList.Contains(dialog.Id))
            return;

        _dialogQueue.Enqueue(dialog);

        if (!_isQueueBusy)
        {
            _isQueueBusy = true;
            ShowText();
        }
    }

    private void ShowText()
    {
        var dialog = _dialogQueue.Dequeue();

        if (!_shownDialogIdList.Contains(dialog.Id))
            _shownDialogIdList.Add(dialog.Id);

        StartCoroutine(ShowTextCo(dialog));
    }

    private IEnumerator ShowTextCo(DialogSO dialog)
    {
        yield return new WaitForSeconds(dialog.delay);
        _textBox.SetActive(true);
        _shouldHideText = true;

        foreach (var line in dialog.lines)
        {
            var timer = 0f;
            _isLineVisible = false;

            line.Init(onProgress: () =>
            {
                _progressUI.text = $"{100f * line.progress:0}%";
                UpdateSlider(line);
            });

            yield return new WaitForSeconds(line.delay);

            var text = line.GetLocalizedText();

            _text.text = $"{text}";

            if (line.onShowEvent != null)
                line.onShowEvent.Trigger();

            var duration = text.Length * _secondsPerLetter;
            duration = Mathf.Clamp(duration, _minDurationPerLine, _maxDurationPerLine);

            duration += line.extraDuration;

            while (!line.ShouldSkip)
            {
                duration = 0f;
                yield return null;
            }

            while (timer < duration)
            {
                if (_pressedReturn && !_isLineVisible)
                {
                    _pressedReturn = false;
                    _typeWriter.SkipTypewriter();
                }
                else if (_pressedReturn && _isLineVisible)
                {
                    _pressedReturn = false;
                    break;
                }

                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            if (line.ShouldSkip && line.ShowProgress)
                _fullProgressFeedback.PlayFeedbacks();

            yield return new WaitForSeconds(line.delayAfterSkip);

            if (line.onHideEvent != null)
                line.onHideEvent.Trigger();

            line.Destroy();
        }

        if (_dialogQueue.Count > 0)
        {
            yield return new WaitForSeconds(_dialogDelay);
            ShowText();
        }
        else if (_dialogQueue.Count == 0)
            _isQueueBusy = false;
    }

    private void UpdateSlider(DialogLine line)
    {
        _slider.gameObject.SetActive(line.ShowProgress);

        if (_sliderCo != null)
            StopCoroutine(_sliderCo);
        _sliderCo = StartCoroutine(AnimateSliderCo(line.progress));
    }

    private IEnumerator AnimateSliderCo(float progress)
    {
        while (true)
        {
            var sliderValue = Mathf.MoveTowards(_slider.Value, progress, _sliderSpeed * Time.deltaTime);
            _slider.SetValue(sliderValue);

            if (_slider.Value.IsApprox(progress, 0.001f))
                break;

            yield return null;
        }
    }

    private void HideTextBox()
    {
        _textBox.SetActive(false);
        _slider.gameObject.SetActive(false);
    }

    public class SaveData
    {
        public List<string> shownDialogIdList = new();
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        saveData.shownDialogIdList = _shownDialogIdList;
        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        _shownDialogIdList = saveData.shownDialogIdList;
    }
}
