using MoreMountains.Feedbacks;
using UnityEngine;

public class SpaceCapsule : MonoBehaviour
{
    [SerializeField] private MiningStencil _miningStencil;
    [SerializeField] private GameObject _explosionParticles;
    [SerializeField] private MMF_Player _explosionFeedback;

    public void ClipTerrain()
    {
        _miningStencil.gameObject.SetActive(true);
        _miningStencil.transform.up = Vector3.up;
        _miningStencil.gameObject.SetActive(false);
    }

    public void PlayExplosion()
    {
        _explosionFeedback.PlayFeedbacks();
        //_explosionParticles.transform.SetParent(null);
        //_explosionParticles.SetActive(true);
    }
}
