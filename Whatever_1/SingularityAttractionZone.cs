using UnityEngine;

public interface ISingularityAffectee
{
    public Rigidbody2D Body { get; }
    public void OnEnterSingularityCore();
    public bool IsAttractable { get => true; }
}

[RequireComponent(typeof(Collider2D))]
public class SingularityAttractionZone : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private bool _towardCenter;
    [SerializeField] private LayerMask _layerMask;

    private void OnTriggerStay2D(Collider2D other)
    {
        var singularityAffectee = other.GetComponent<ISingularityAffectee>();

        if (singularityAffectee == null || !singularityAffectee.IsAttractable)
            return;

        singularityAffectee.Body.bodyType = RigidbodyType2D.Kinematic;
        singularityAffectee.Body.angularVelocity = Random.value * 360f;
        singularityAffectee.Body.linearVelocity = Vector3.zero;
        singularityAffectee.Body.transform.position = Vector2.MoveTowards(singularityAffectee.Body.transform.position, transform.position, _speed * Time.deltaTime);
    }
}
