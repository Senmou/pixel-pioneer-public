using UnityEngine;

public class TutorialArtifact : BaseTutorial
{
    private float _timer;
    private const float TIMER_MAX = 10f;

    private void Update()
    {
        _timer += Time.deltaTime;
    }

    public override bool CancelCondition()
    {
        return _timer >= TIMER_MAX;
    }
}
