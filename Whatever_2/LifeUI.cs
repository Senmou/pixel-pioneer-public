using UnityEngine;
using System;
using System.Collections;

public class LifeUI : MonoBehaviour
{
    [SerializeField] private LifeUI_Heart _heartTemplate;
    [SerializeField] private Transform _container;

    private LifeUI_Heart[] _heartArray;

    private void Start()
    {
        _heartTemplate.gameObject.SetActive(false);
        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        Init();
        Player.Instance.OnHealthChanged += Player_OnHealthChanged;
    }

    private void OnDestroy()
    {
        Player.Instance.OnHealthChanged -= Player_OnHealthChanged;
        PlayerSpawner.Instance.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;
    }

    private void Player_OnHealthChanged(object sender, EventArgs e)
    {
        UpdateUI();
    }

    public void Init()
    {
        _heartArray = new LifeUI_Heart[Player.Instance.MaxHealth];
        for (int i = 0; i < Player.Instance.MaxHealth; i++)
        {
            var heart = Instantiate(_heartTemplate, _container);
            heart.gameObject.SetActive(true);
            _heartArray[i] = heart;
        }

        StartCoroutine(UpdateUIDelayed(showAnimation: false));
    }

    public void UpdateUI(bool showAnimation = true)
    {
        for (int i = 0; i < _heartArray.Length; i++)
        {
            _heartArray[i].UpdateUI(isFull: i < Player.Instance.CurrentHealth, showAnimation);
        }
    }

    private IEnumerator UpdateUIDelayed(bool showAnimation)
    {
        yield return null;
        UpdateUI(showAnimation);
    }
}
