public class RespawnMenu : Menu<RespawnMenu>
{
    public static void Show()
    {
        Open();
    }

    public static void Hide()
    {
        Close();
    }

    public void OnRespawnButtonClicked()
    {
        Close();
        Player.Instance.Respawn();
    }

    public override void OnBackPressed()
    {

    }
}
