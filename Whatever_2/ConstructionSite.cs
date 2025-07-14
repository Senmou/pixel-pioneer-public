using UnityEngine;

public class ConstructionSite : MonoBehaviour
{
    #region Unity Events
    public void OnSpawnConstructionSite()
    {
        PlayerCamera.Instance.ConstructionSiteStartShaker.PlayFeedbacks();
    }

    public void OnLandingConstructionSite()
    {
        PlayerCamera.Instance.ConstructionSiteLandingShaker.PlayFeedbacks();
    }
    #endregion
}
