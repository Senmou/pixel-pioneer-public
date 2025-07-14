using UnityEngine;

public class InfoStation : BaseBuilding
{
    public Singularity Singularity => _singularity;
    public AsteroidGameSystem AsteroidGameSystem => _asteroidGameSystem;

    private Singularity _singularity;
    private AsteroidGameSystem _asteroidGameSystem;

    private new void Start()
    {
        base.Start();
        _singularity = FindAnyObjectByType<Singularity>();
        _asteroidGameSystem = FindAnyObjectByType<AsteroidGameSystem>();
    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (interactionType == Interactor.InteractionType.START)
        {
            InfoStationMenu.Show(this);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            InfoStationMenu.Hide();
        }
    }
}
