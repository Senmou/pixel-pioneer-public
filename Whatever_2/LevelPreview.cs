using UnityEngine;

public class LevelPreview : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Space(10)]
    [Header("Colors")]
    [SerializeField] private Color _airColor;
    [SerializeField] private Color _caveColor;
    [SerializeField] private Color _dirtColor;
    [SerializeField] private Color _stoneColor;
    [SerializeField] private Color _coalColor;
    [SerializeField] private Color _ironColor;
    [SerializeField] private Color _copperColor;
    [SerializeField] private Color _goldColor;
    [SerializeField] private Color _baseHeightColor;

    public void CreateSprite(WorldParameterSO worldParameters)
    {
        //var world = World.GenerateWorld(worldParameters);
        //Texture2D texture = new Texture2D(worldParameters.BlocksHorizontal, worldParameters.BlocksVertical);

        //for (int x = 0; x < world.blocks.GetLength(0); x++)
        //{
        //    for (int y = 0; y < world.blocks.GetLength(1); y++)
        //    {
        //        var block = world.blocks[x, y];
        //        switch (block.blockType)
        //        {
        //            case BlockType.Air: texture.SetPixel(x, y, _airColor); break;
        //            case BlockType.Cave: texture.SetPixel(x, y, _caveColor); break;
        //            case BlockType.Dirt: texture.SetPixel(x, y, _dirtColor); break;
        //            case BlockType.Stone: texture.SetPixel(x, y, _stoneColor); break;
        //            case BlockType.Coal: texture.SetPixel(x, y, _coalColor); break;
        //            case BlockType.Iron: texture.SetPixel(x, y, _ironColor); break;
        //            case BlockType.Copper: texture.SetPixel(x, y, _copperColor); break;
        //            case BlockType.Gold: texture.SetPixel(x, y, _goldColor); break;
        //        }

        //        if (y == worldParameters.oxygenHeightThreshold)
        //            texture.SetPixel(x, y, _baseHeightColor);
        //    }
        //}

        //texture.filterMode = FilterMode.Point;
        //texture.Apply();

        //var sprite = Sprite.Create(texture, new Rect(0f, 0f, worldParameters.BlocksHorizontal, worldParameters.BlocksVertical), Vector2.zero, pixelsPerUnit: 1);
        //_spriteRenderer.sprite = sprite;
    }
}
