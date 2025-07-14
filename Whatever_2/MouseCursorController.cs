using UnityEngine;

public class MouseCursorController : MonoBehaviour
{
    public static MouseCursorController Instance { get; private set; }

    [SerializeField] private Texture2D _mouseCursorDefault;
    [SerializeField] private Texture2D _mouseCursorCrosshair;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Cursor.SetCursor(_mouseCursorDefault, Vector2.zero, CursorMode.Auto);
    }

    public void SetCursor_Crosshair()
    {
        Cursor.SetCursor(_mouseCursorCrosshair, new Vector2(_mouseCursorCrosshair.width / 2f, _mouseCursorCrosshair.height / 2f), CursorMode.Auto);
    }

    public void SetCursor_Default()
    {
        Cursor.SetCursor(_mouseCursorDefault, Vector2.zero, CursorMode.Auto);
    }
}
