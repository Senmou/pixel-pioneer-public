using UnityEngine;

public class PreventFallingOutOfWorld : MonoBehaviour
{
    private void Update()
    {
        if (transform.position.y < -120f)
        {
            transform.position = transform.position.WithY(60f);
            print("prevent falling out of bounds");
        }
    }
}
