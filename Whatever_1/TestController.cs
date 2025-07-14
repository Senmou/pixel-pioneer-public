using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] private Transform _blue;

    private void Update()
    {
        var camPos = Camera.main.transform.position;
        _blue.localPosition = -new Vector3(camPos.x, camPos.y);
    }
}
