using UnityEngine;
using Febucci.UI;

public class AsteroidWarning : MonoBehaviour
{
    [SerializeField] private TypewriterByCharacter _typewriter;

    public void Show(string text)
    {
        _typewriter.ShowText(text);
    }

    public void Hide()
    {
        _typewriter.StartDisappearingText();
    }
}
