using UnityEngine;

public class RotateParticleVfx : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _offset;

    private void Update()
    {
        var targetDir = Player.Instance.transform.position - transform.position;
        var angle = Vector3.Angle(targetDir, Player.Instance.transform.up);

        //var angles = transform.eulerAngles;
        //angles.y = -90f;
        //angles.x = angle + _offset;
        //transform.eulerAngles = angles;

        transform.LookAt(Player.Instance.transform);
    }
}
