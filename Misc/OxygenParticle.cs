using UnityEngine;

public class OxygenParticle : MonoBehaviour
{
    [SerializeField] private float _oxygenAmount;

    private OxygenGenerator _oxygenGenerator;

    public void Init(OxygenGenerator oxygenGenerator)
    {
        _oxygenGenerator = oxygenGenerator;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody != Player.Instance.Body)
            return;

        OxygenController.Instance.IncOxygen(_oxygenAmount);
        _oxygenGenerator.OnParticleCollected();

        Destroy(gameObject);
    }
}
