using System.Collections.Generic;
using UnityEngine;

public class WorldParameters
{
    public int _chunkDimension = 25;
    public int chunkCountHorizontal;
    public int chunkCountVertical;
    public int oxygenBelowDeepestHeightThreshold;

    public int BlocksHorizontal => chunkCountHorizontal * _chunkDimension;
    public int BlocksVertical => chunkCountVertical * _chunkDimension;

    public int ArtifactCount { get; private set; }

    public NoiseParameters surfaceNoiseParameters;
    public NoiseParameters mountainNoiseParameters;

    public OreParameters coalParameters;
    public OreParameters copperParameters;
    public OreParameters ironParameters;
    public OreParameters goldParameters;
    public OreParameters uraniumParameters;
    public OreParameters blastStoneParameters;
    public OreParameters energyParameters;
    public OreParameters sandParameters;
    public OreParameters iceParameters;

    public OreParameters caveParameters;

    public List<OreParameters> OreParameters => new List<OreParameters> { coalParameters, copperParameters, ironParameters, goldParameters, uraniumParameters, blastStoneParameters, energyParameters, sandParameters, iceParameters };

    public List<BlockType> GetBlockTypes()
    {
        List<BlockType> blockTypes = new List<BlockType>();

        if (coalParameters != null) blockTypes.Add(BlockType.Coal);
        if (copperParameters != null) blockTypes.Add(BlockType.Copper);
        if (ironParameters != null) blockTypes.Add(BlockType.Iron);
        if (goldParameters != null) blockTypes.Add(BlockType.Gold);
        if (uraniumParameters != null) blockTypes.Add(BlockType.Uranium);
        if (blastStoneParameters != null) blockTypes.Add(BlockType.BlastStone);
        if (caveParameters != null) blockTypes.Add(BlockType.Cave);
        if (energyParameters != null) blockTypes.Add(BlockType.Energy);
        if (sandParameters != null) blockTypes.Add(BlockType.Sand);
        if (iceParameters != null) blockTypes.Add(BlockType.Ice);

        return blockTypes;
    }

    public static WorldParameters GetWorldParameters(int levelIndex)
    {
        var instance = new WorldParameters();
        return instance.GenerateWorldParameters(levelIndex);
    }

    private WorldParameters GenerateWorldParameters(int levelIndex)
    {
        if (levelIndex == 0)
            return GetWorldParameters_Level_1();
        else if (levelIndex == 1)
            return GetWorldParameters_Level_2();
        else if (levelIndex == 2)
            return GetWorldParameters_Level_3();

        Debug.LogWarning("Default world parameters (Level 1)");
        return GetWorldParameters_Level_1();
    }

    private WorldParameters GetWorldParameters_Level_1()
    {
        WorldParameters worldParameters = new()
        {
            chunkCountHorizontal = 20,
            chunkCountVertical = 10
        };

        var raise = 100;
        worldParameters.oxygenBelowDeepestHeightThreshold = 50;
        worldParameters.ArtifactCount = 2;

        worldParameters.surfaceNoiseParameters = new NoiseParameters
        {
            blockType = BlockType.Dirt,
            octaves = 4,
            lacunarity = 1f,
            gain = 0.19f,
            amplitude = Random.Range(0.1f, 0.1f),
            frequency = Random.Range(0.003f, 0.004f),
            raise = raise,
            offset = 0f,
            threshold = 0f
        };

        worldParameters.mountainNoiseParameters = new NoiseParameters
        {
            blockType = BlockType.Stone,
            octaves = 4,
            lacunarity = 3f,
            gain = 0.5f,
            amplitude = 0.3f,
            frequency = 0.004f,
            raise = raise - 100,
            offset = 0f,
            threshold = 0f
        };

        worldParameters.coalParameters = new OreParameters
        {
            blockType = BlockType.Coal,
            veinCount = 50,
            minHeight = 0,
            belowDeepestSurfaceHeight = 15,
            minSize = 5,
            maxSize = 10
        };

        worldParameters.ironParameters = new OreParameters
        {
            blockType = BlockType.Iron,
            veinCount = 50,
            minHeight = 0,
            belowDeepestSurfaceHeight = 20,
            minSize = 5,
            maxSize = 15
        };

        worldParameters.copperParameters = new OreParameters
        {
            blockType = BlockType.Copper,
            veinCount = 50,
            minHeight = 0,
            belowDeepestSurfaceHeight = 20,
            minSize = 15,
            maxSize = 20
        };

        worldParameters.caveParameters = new OreParameters
        {
            blockType = BlockType.Cave,
            veinCount = 2,
            minHeight = 0,
            belowDeepestSurfaceHeight = 0,
            minSize = 1,
            maxSize = 2
        };

        return worldParameters;
    }

    private WorldParameters GetWorldParameters_Level_2()
    {
        WorldParameters worldParameters = new();

        worldParameters.chunkCountHorizontal = 30;
        worldParameters.chunkCountVertical = 30;
        worldParameters.ArtifactCount = 2;

        var raise = 200;
        worldParameters.oxygenBelowDeepestHeightThreshold = 25;

        worldParameters.surfaceNoiseParameters = new NoiseParameters
        {
            blockType = BlockType.Dirt,
            octaves = 4,
            lacunarity = 2f,
            gain = 0.19f,
            amplitude = Random.Range(0.1f, 0.3f),
            frequency = Random.Range(0.003f, 0.004f),
            raise = raise,
            offset = 0f,
            threshold = 0f
        };

        worldParameters.mountainNoiseParameters = new NoiseParameters
        {
            blockType = BlockType.Stone,
            octaves = 4,
            lacunarity = 1f,
            gain = 0.4f,
            amplitude = Random.Range(0.1f, 0.15f),
            frequency = Random.Range(0.003f, 0.0035f),
            raise = raise - 100,
            offset = 0f,
            threshold = 0f
        };

        worldParameters.coalParameters = new OreParameters
        {
            blockType = BlockType.Coal,
            veinCount = 70,
            minHeight = 0,
            belowDeepestSurfaceHeight = 15,
            minSize = 15,
            maxSize = 50
        };

        worldParameters.copperParameters = new OreParameters
        {
            blockType = BlockType.Copper,
            veinCount = 40,
            minHeight = 0,
            belowDeepestSurfaceHeight = 20,
            minSize = 10,
            maxSize = 20
        };

        worldParameters.ironParameters = new OreParameters
        {
            blockType = BlockType.Iron,
            veinCount = 30,
            minHeight = 0,
            belowDeepestSurfaceHeight = 70,
            minSize = 25,
            maxSize = 35
        };

        worldParameters.goldParameters = new OreParameters
        {
            blockType = BlockType.Gold,
            veinCount = 10,
            minHeight = 0,
            belowDeepestSurfaceHeight = 100,
            minSize = 10,
            maxSize = 20
        };

        worldParameters.sandParameters = new OreParameters
        {
            blockType = BlockType.Sand,
            veinCount = 20,
            minHeight = 0,
            belowDeepestSurfaceHeight = -100,
            minSize = 150,
            maxSize = 300
        };

        worldParameters.iceParameters = new OreParameters
        {
            blockType = BlockType.Ice,
            veinCount = 30,
            minHeight = 0,
            belowDeepestSurfaceHeight = 0,
            minSize = 15,
            maxSize = 30
        };

        worldParameters.blastStoneParameters = new OreParameters
        {
            blockType = BlockType.BlastStone,
            veinCount = 5,
            minHeight = 0,
            belowDeepestSurfaceHeight = 100,
            minSize = 10,
            maxSize = 25
        };

        worldParameters.caveParameters = new OreParameters
        {
            blockType = BlockType.Cave,
            veinCount = 3,
            minHeight = 0,
            belowDeepestSurfaceHeight = 50,
            minSize = 10,
            maxSize = 10
        };

        return worldParameters;
    }
    private WorldParameters GetWorldParameters_Level_3()
    {
        WorldParameters worldParameters = new();

        worldParameters.chunkCountHorizontal = 30;
        worldParameters.chunkCountVertical = 30;
        worldParameters.ArtifactCount = 2;

        var raise = 300;
        worldParameters.oxygenBelowDeepestHeightThreshold = 20;

        worldParameters.surfaceNoiseParameters = new NoiseParameters
        {
            blockType = BlockType.Dirt,
            octaves = 4,
            lacunarity = 2f,
            gain = 0.19f,
            amplitude = Random.Range(0.1f, 0.3f),
            frequency = Random.Range(0.003f, 0.004f),
            raise = raise,
            offset = 0f,
            threshold = 0f
        };

        worldParameters.mountainNoiseParameters = new NoiseParameters
        {
            blockType = BlockType.Stone,
            octaves = 4,
            lacunarity = 1f,
            gain = 0.4f,
            amplitude = Random.Range(0.2f, 0.25f),
            frequency = Random.Range(0.003f, 0.004f),
            raise = raise - 100,
            offset = 0f,
            threshold = 0f
        };

        worldParameters.coalParameters = new OreParameters
        {
            blockType = BlockType.Coal,
            veinCount = 70,
            minHeight = 0,
            belowDeepestSurfaceHeight = -55,
            minSize = 15,
            maxSize = 50
        };

        worldParameters.copperParameters = new OreParameters
        {
            blockType = BlockType.Copper,
            veinCount = 30,
            minHeight = 0,
            belowDeepestSurfaceHeight = 20,
            minSize = 10,
            maxSize = 50
        };

        worldParameters.ironParameters = new OreParameters
        {
            blockType = BlockType.Iron,
            veinCount = 10,
            minHeight = 0,
            belowDeepestSurfaceHeight = 70,
            minSize = 25,
            maxSize = 75
        };

        worldParameters.goldParameters = new OreParameters
        {
            blockType = BlockType.Gold,
            veinCount = 10,
            minHeight = 0,
            belowDeepestSurfaceHeight = 100,
            minSize = 25,
            maxSize = 75
        };

        worldParameters.uraniumParameters = new OreParameters
        {
            blockType = BlockType.Uranium,
            veinCount = 10,
            minHeight = 0,
            belowDeepestSurfaceHeight = 150,
            minSize = 5,
            maxSize = 10
        };

        worldParameters.blastStoneParameters = new OreParameters
        {
            blockType = BlockType.BlastStone,
            veinCount = 100,
            minHeight = 0,
            belowDeepestSurfaceHeight = 100,
            minSize = 3,
            maxSize = 5
        };

        worldParameters.iceParameters = new OreParameters
        {
            blockType = BlockType.Ice,
            veinCount = 50,
            minHeight = 0,
            belowDeepestSurfaceHeight = 0,
            minSize = 15,
            maxSize = 30
        };

        worldParameters.caveParameters = new OreParameters
        {
            blockType = BlockType.Cave,
            veinCount = 1,
            minHeight = 0,
            belowDeepestSurfaceHeight = 50,
            minSize = 50,
            maxSize = 50
        };

        return worldParameters;
    }
}

public class NoiseParameters
{
    public BlockType blockType;

    public int octaves;
    public float lacunarity;
    public float gain;
    public float amplitude;
    public float frequency;
    public int raise;
    public float offset;
    public float threshold;
}

public class OreParameters
{
    public BlockType blockType;
    public int veinCount;
    public int minHeight;
    public int belowDeepestSurfaceHeight;
    public int minSize;
    public int maxSize;
}

public class World
{
    public readonly Block[,] blocks;
    public readonly int blocksVertical;
    public readonly int blocksHorizontal;
    public int deepestSurfaceHeight;

    private Vector2 _playerSpawnPosition;
    public Vector2 PlayerSpawnPosition => _playerSpawnPosition;

    private List<Vector2> _artifactSpawnPositions;
    public List<Vector2> ArtifactSpawnPositions => _artifactSpawnPositions;

    public World(int blocksHorizontal, int blocksVertical, Vector2 playerSpawnPosition)
    {
        this.blocksVertical = blocksVertical;
        this.blocksHorizontal = blocksHorizontal;

        deepestSurfaceHeight = 0;
        _playerSpawnPosition = playerSpawnPosition;

        blocks = new Block[blocksHorizontal, blocksVertical];
        _artifactSpawnPositions = new List<Vector2>();
    }

    public bool GetBlock(int worldPosX, int worldPosY, out Block block)
    {
        if (worldPosX < 0 || worldPosX > blocksHorizontal - 1 || worldPosY < 0 || worldPosY > blocksVertical - 1)
        {
            block = default;
            return false;
        }

        block = blocks[worldPosX, worldPosY];
        return true;
    }

    public bool GetBlock(Vector3Int worldPos, out Block block)
    {
        if (worldPos.x < 0 || worldPos.x > blocksHorizontal - 1 || worldPos.y < 0 || worldPos.y > blocksVertical - 1)
        {
            block = default;
            return false;
        }

        return GetBlock(worldPos.x, worldPos.y, out block);
    }

    public void SetBlock(int x, int y, Block block)
    {
        if (x < 0 || x > blocksHorizontal - 1 || y < 0 || y > blocksVertical - 1)
            return;

        blocks[x, y] = block;
    }

    private void AddRange(int x, int startY, int endY, Block block)
    {
        for (int y = startY; y < endY; y++)
        {
            blocks[x, y] = block;
        }
    }

    public static World GenerateWorld(WorldParameters worldParameters, CaveSchemeSOList caveSchemeSOList, int seed)
    {
        UnityEngine.Random.InitState(seed);
        World world = new World(worldParameters.BlocksHorizontal, worldParameters.BlocksVertical, playerSpawnPosition: Vector2.zero);

        CreateSurface(worldParameters.surfaceNoiseParameters, out world.deepestSurfaceHeight);
        CreateMountains(worldParameters.mountainNoiseParameters);

        foreach (var oreParams in worldParameters.OreParameters)
        {
            if (oreParams == null)
                continue;

            CreateOre(oreParams, world.deepestSurfaceHeight);
        }

        CreateCaves(worldParameters.caveParameters, world.deepestSurfaceHeight);
        CreateTrees();

        SetPlayerSpawnPosition();
        CreateSpaceForPortal();

        return world;

        void CreateSurface(NoiseParameters surfaceSettings, out int deepestSurfaceHeight)
        {
            var deepestHeight = int.MaxValue;

            for (int x = 0; x < worldParameters.BlocksHorizontal; x++)
            {
                var noise = SurfaceNoise1D(x, surfaceSettings);

                for (int y = 0; y < worldParameters.BlocksVertical; y++)
                {
                    var terrainHeight = (int)(noise * worldParameters.BlocksVertical) + surfaceSettings.raise;

                    if (terrainHeight < deepestHeight)
                        deepestHeight = terrainHeight;

                    if (y > terrainHeight)
                    {
                        world.AddRange(x, startY: terrainHeight + 1, endY: worldParameters.BlocksVertical - terrainHeight - 1, new Block(BlockType.Air));
                        break;
                    }
                    else
                    {
                        var stoneNoise = Mathf.PerlinNoise(x * Mathf.PI / 50f, y * Mathf.PI / 50f);

                        Block block;
                        if (stoneNoise < 0.3f)
                        {
                            block = new(BlockType.Stone);
                        }
                        else
                        {
                            block = new(BlockType.Dirt);
                        }
                        world.SetBlock(x, y, block);
                    }
                }
            }

            deepestSurfaceHeight = deepestHeight;
        }

        void CreateMountains(NoiseParameters groundSettings)
        {
            for (int x = 0; x < worldParameters.BlocksHorizontal; x++)
            {
                for (int y = 0; y < worldParameters.BlocksVertical; y++)
                {
                    world.GetBlock(x, y, out Block block);
                    if (block.blockType != BlockType.Air)
                        continue;

                    var noise = SurfaceNoise1D(x, groundSettings);
                    var terrainHeight = (int)(noise * worldParameters.BlocksVertical) + groundSettings.raise;

                    if (y <= terrainHeight)
                    {
                        block = new Block(BlockType.Stone);
                        world.SetBlock(x, y, block);
                    }
                }
            }
        }

        float SurfaceNoise1D(float worldPosX, NoiseParameters groundSettings)
        {
            var amplitude = groundSettings.amplitude;
            var frequency = groundSettings.frequency;
            var lacunarity = groundSettings.lacunarity;
            var gain = groundSettings.gain;

            float noise = 0f;
            for (int i = 0; i < groundSettings.octaves; i++)
            {
                noise += amplitude * Mathf.PerlinNoise1D(frequency * worldPosX);
                frequency *= lacunarity;
                amplitude *= gain;
            }
            return noise;
        }


        void CreateOre(OreParameters oreParameters, int deepestSurfaceHeight)
        {
            for (int i = 0; i < oreParameters.veinCount; i++)
            {
                var startPosX = Random.Range(0, world.blocksHorizontal);
                var startPosY = Random.Range(oreParameters.minHeight, deepestSurfaceHeight - oreParameters.belowDeepestSurfaceHeight);

                var size = Random.Range(oreParameters.minSize, oreParameters.maxSize + 1);
                Vector3Int pos = new Vector3Int(startPosX, startPosY);

                for (int k = 0; k < size; k++)
                {
                    List<Vector3Int> offsets = new List<Vector3Int> { new Vector3Int(0, 0), new Vector3Int(-1, 0), new Vector3Int(1, 0), new Vector3Int(0, 1), new Vector3Int(0, -1) };

                    foreach (var offset in offsets)
                    {
                        if (oreParameters.blockType == BlockType.Energy)
                        {
                            if (offset != new Vector3Int(0, 0))
                                break;
                        }

                        var offsetPos = pos + offset;
                        if (world.GetBlock(offsetPos, out Block currentBlock))
                        {
                            if (currentBlock.blockType == BlockType.Air || currentBlock.blockType == BlockType.Cave)
                                continue;
                        }

                        world.SetBlock(offsetPos.x, offsetPos.y, new Block(oreParameters.blockType));
                    }

                    var dir = Random.Range(0, 4);
                    switch (dir)
                    {
                        case 0: // left
                            {
                                pos.x -= 1;
                            }
                            break;
                        case 1: // right
                            {
                                pos.x += 1;
                            }
                            break;
                        case 2: // top
                            {
                                pos.y += 1;
                            }
                            break;
                        case 3: // bot
                            {
                                pos.y -= 1;
                            }
                            break;
                    }
                }
            }
        }

        void CreateCaves(OreParameters caveParameters, int deepestSurfaceHeight)
        {
            if (caveParameters == null)
            {
                Debug.LogWarning("No cave parameters");
                return;
            }

            var caveGenerator = new CaveGenerator();

            List<Vector3> allCaveBlocks = new();

            for (int i = 0; i < caveParameters.veinCount; i++)
            {
                var startPosX = Random.Range(0, world.blocksHorizontal);
                var startPosY = Random.Range(caveParameters.minHeight, deepestSurfaceHeight - caveParameters.belowDeepestSurfaceHeight);

                var caveBlockList = caveGenerator.CreateCave(new Vector2Int(startPosX, startPosY), caveParameters, caveSchemeSOList);
                allCaveBlocks.AddRange(caveBlockList);

                foreach (var pos in caveBlockList)
                {
                    world.GetBlock((int)pos.x, (int)pos.y, out Block block);
                    if (block.blockType == BlockType.Air)
                        continue;

                    world.SetBlock((int)pos.x, (int)pos.y, new Block(BlockType.Cave));
                }
            }

            for (int i = 0; i < worldParameters.ArtifactCount; i++)
            {
                var randomPos = allCaveBlocks.Random();
                world.ArtifactSpawnPositions.Add(randomPos);
            }
        }

        void CreateTrees()
        {
            int treeCount = 10;
            for (int i = 0; i < treeCount; i++)
            {
                var posX = i * worldParameters.BlocksHorizontal / treeCount;
                world.GetTopMostSolidBlock(posX, out int posY);

                PrefabManager.Instance.Prefabs.treeTilemapPrefab.GetBlocks(out var blocks);
                foreach (var block in blocks)
                {
                    var localPos = block.Item1 - PrefabManager.Instance.Prefabs.treeTilemapPrefab.Origin;
                    var blockType = block.Item2;

                    world.SetBlock((int)(posX + localPos.x), (int)(posY + localPos.y + 1), new Block(blockType));
                }
            }
        }

        void SetPlayerSpawnPosition()
        {
            var horizontalCenterPos = world.blocksHorizontal / 2;
            world.GetTopMostSolidBlock(horizontalCenterPos, out int y);
            world._playerSpawnPosition = new Vector2(horizontalCenterPos, y + 1);
        }

        void CreateSpaceForPortal()
        {
            var width = 3;
            var height = 4;
            var portalPosition = new Vector2Int((int)world.PlayerSpawnPosition.x, (int)world.PlayerSpawnPosition.y);

            for (int x = portalPosition.x; x < portalPosition.x + width; x++)
            {
                for (int y = portalPosition.y; y < portalPosition.y + height; y++)
                {
                    world.SetBlock(x, y, new Block(BlockType.Air));
                }
            }
        }
    }

    public bool GetTopMostSolidBlock(int x, out int topMostPosition)
    {
        for (int y = blocks.GetLength(1) - 1; y > 0; y--)
        {
            var blockType = blocks[x, y].blockType;
            if (blockType != BlockType.Air && blockType != BlockType.TreeTrunk && blockType != BlockType.TreeLeaf && blockType != BlockType.Cave)
            {
                topMostPosition = y;
                return true;
            }
        }
        topMostPosition = -1;
        return false;
    }
}

public enum BlockType
{
    Air,
    Cave,
    Dirt,
    Stone,
    Coal,
    Grass,
    Iron,
    Copper,
    Gold,
    Uranium,
    BlastStone,
    Energy,
    TreeTrunk,
    TreeLeaf,
    Sand,
    Ice
}

public struct Block
{
    public readonly BlockType blockType;

    public Block(BlockType blockType)
    {
        this.blockType = blockType;
    }
}