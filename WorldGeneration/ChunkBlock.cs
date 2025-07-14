using System;

public partial class TilemapChunkSystem
{
    [Serializable]
    public class ChunkBlock
    {
        public Chunk Chunk { get; private set; }
        public BlockType BlockType { get; private set; }
        public bool IsDirty { get; private set; }

        public ChunkBlock(BlockType blockType, Chunk chunk)
        {
            Chunk = chunk;
            BlockType = blockType;
        }

        public void SetBlockType(BlockType blockType, bool isDirty = true)
        {
            BlockType = blockType;
            IsDirty = isDirty;
        }

        public void SetDirty(bool isDirty) => IsDirty = isDirty;
    }
}
