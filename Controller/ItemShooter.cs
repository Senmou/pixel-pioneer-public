using UnityEngine;
using System;

public class ItemShooter : MonoBehaviour
{
    public event EventHandler<OnItemShotEventArgs> OnItemShot;
    public class OnItemShotEventArgs : EventArgs
    {
        public InventorySlot slot;
        public GameObject item;
    }

    public static ItemShooter Instance { get; private set; }

    [SerializeField] private AudioPlayer _shootAudioPlayer;

    private float _shootTimer;
    private const float SHOOT_DELAY = 0.2f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Player.Instance.IsDead)
            return;

        _shootTimer += Time.deltaTime;

        if (InputController.Instance.IsPressed_RightMouseButton && _shootTimer >= SHOOT_DELAY && !Helper.IsPointerOverUIElement())
        {
            _shootTimer = 0f;

            var selectedSlot = PlayerInventoryUI.Instance.GetSelectedSlot();
            ShootItem(selectedSlot);
        }
    }

    private void ShootItem(InventorySlot slot)
    {
        if (slot.ItemStack != null && slot.ItemStack.itemSO.preventThrowing)
            return;

        var mousePos = Helper.MousePos;
        var dir = (mousePos - transform.position).normalized;

        var spawnedItem = slot.RemoveAndDropItem(addForceTowardsPlayer: false);
        if (spawnedItem == null)
            return;

        spawnedItem.transform.position = Player.Instance.transform.position + new Vector3(0f, 1f);

        var body = spawnedItem.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.linearVelocity = Vector3.zero;
        body.AddForce(dir * 20, ForceMode2D.Impulse);

        _shootAudioPlayer.PlaySound(allowWhilePlaying: true);

        var blastStone = spawnedItem.GetComponent<BlastStone>();
        if (blastStone != null)
        {
            blastStone.DetonateOnContact();
        }

        OnItemShot?.Invoke(this, new OnItemShotEventArgs { item = spawnedItem, slot = slot });
    }
}
