using UnityEngine.Tilemaps;
using UnityEngine;
using System;

public partial class TilemapChunkSystem
{
    [Serializable]
    public class Chunk
    {
        public Vector3Int LocalPosition => _localPosition;
        public Vector3 WorldPosition => new Vector3(_localPosition.x * _chunkUnitSize, _localPosition.y * _chunkUnitSize);
        public BlockType[,] Blocks => _blocks;
        public bool IsReady { get; set; }
        public bool ShadowsNeedUpdate { get; set; }
        public TileChangeData[] EmptyTileChangeDataArray { get; private set; }
        public bool PreventAutoRelease => _preventAutoRelease;

        private float _chunkUnitSize;
        private Vector3Int _localPosition;
        private BlockType[,] _blocks;
        private int _dimension;
        private bool _preventAutoRelease;

        public Chunk(int dimension = 25, float chunkUnitSize = 25f)
        {
            ShadowsNeedUpdate = true;
            _blocks = new BlockType[dimension, dimension];
            _chunkUnitSize = chunkUnitSize;
            _dimension = dimension;

            EmptyTileChangeDataArray = new TileChangeData[dimension * dimension];
        }

        public void SetPosition(Vector3Int localPosition)
        {
            _localPosition = localPosition;
        }

        public Vector3Int GetBlockTilemapPos(Vector3Int localBlockPosition)
        {
            Vector3Int chunkIndex = new();
            chunkIndex.x = _localPosition.x * Instance._worldParameters._chunkDimension;
            chunkIndex.y = _localPosition.y * Instance._worldParameters._chunkDimension;
            chunkIndex.z = 1;
            return chunkIndex + localBlockPosition;
        }

        public Vector3Int GetBlockLocalPos(Vector3Int tileIndex)
        {
            Vector3Int localBlockPos = new();
            localBlockPos.x = tileIndex.x - (_localPosition.x * _dimension);
            localBlockPos.y = tileIndex.y - (_localPosition.y * _dimension);
            return localBlockPos;
        }

        public void UpdateBlock(Vector3Int globalTileIndex, BlockType blockType)
        {
            var blockLocalPos = GetBlockLocalPos(globalTileIndex);
            Blocks[blockLocalPos.x, blockLocalPos.y] = blockType;
        }

        public void AddBlock(int x, int y, BlockType blockType)
        {
            Blocks[x, y] = blockType;
        }

        public void SetPreventAutoRelease(bool value)
        {
            _preventAutoRelease = value;
        }
    }
}
