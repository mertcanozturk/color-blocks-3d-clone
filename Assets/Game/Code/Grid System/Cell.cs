using UnityEngine;

namespace ColorBlocks
{
    public class Cell : MonoBehaviour
    {
        public int col;
        public int row;
        public Block occupyingBlock;

        public bool IsOccupied()
        {
            return occupyingBlock != null;
        }

        public void SetBlock(Block block)
        {
            occupyingBlock = block;
        }

        public void ClearBlock()
        {
            occupyingBlock = null;
        }
    }
}
