using System.Collections.Generic;
using UnityEngine;
using System;

public class BackgroundController : MonoBehaviour
{
    public static BackgroundController Instance { get; private set; }

    [SerializeField] private SpriteRenderer _noiseBackground;
    [SerializeField] private List<BackgroundObject> _cloudBackgrounds;

    [Serializable]
    public class BackgroundObject
    {
        public SpriteRenderer renderer;
        public Vector2 speed;
        public Color color;
    }

    private void OnValidate()
    {
        //if (Application.isPlaying && Application.isEditor)
        //{
        //    if (DebugController.Instance == null)
        //        return;
        //    var data = Helper.LastLevelDataSO.LevelData;
        //    InitBackgrounds(data.Origin, data.Width, data.Height);
        //}
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TilemapChunkSystem.Instance.OnWorldSpawned += TilemapChunkSystem_OnWorldSpawned;
    }

    private void Update()
    {
        if (Player.Instance == null)
            return;

        transform.position = Player.Instance.transform.position.WithZ(10f);
    }

    private void OnDestroy()
    {
        TilemapChunkSystem.Instance.OnWorldSpawned -= TilemapChunkSystem_OnWorldSpawned;
    }

    private void TilemapChunkSystem_OnWorldSpawned(object sender, EventArgs e)
    {
        var world = TilemapChunkSystem.Instance.World;
        InitBackgrounds(world.blocksHorizontal, world.blocksVertical);
    }

    public void InitBackgrounds(float levelWidth, float levelHeight)
    {
        foreach (var background in _cloudBackgrounds)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetTexture("_MainTex", background.renderer.sprite.texture);
            propertyBlock.SetVector("_Speed", background.speed);
            propertyBlock.SetColor("_Color", background.color);
            background.renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
