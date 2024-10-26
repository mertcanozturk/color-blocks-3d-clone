using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlocks
{
    public interface IBlockManager
    {
        void CreateBlocks(LevelData levelData);
        bool MoveBlock(Block block, int direction);
        void ClearBlocks();
        List<Block> GetBlocks();
    }

    public class BlockManager : IBlockManager
    {
        private List<Block> _blocks = new List<Block>();
        private IGridFactory _gridFactory;
        private ICellManager _cellManager;
        private GridParameters _gridParameters;
        private IGateManager _gateManager;
        public BlockManager(IGridFactory gridFactory, ICellManager cellManager, GridParameters gridParameters, IGateManager gateManager)
        {
            _gridFactory = gridFactory;
            _cellManager = cellManager;
            _gridParameters = gridParameters;
            _gateManager = gateManager;
        }

        public void CreateBlocks(LevelData levelData)
        {
            ClearBlocks();
            foreach (var movableInfo in levelData.MovableInfo)
            {
                Vector3 position = _cellManager.GetCellPosition(movableInfo.Row, movableInfo.Col);
                Quaternion rotation = CalculateBlockRotation(movableInfo.Direction[0]);
                Block block = _gridFactory.CreateBlock(position, rotation, movableInfo);
                _blocks.Add(block);
                SetOccupiedCells(block);
            }
        }

        public bool MoveBlock(Block block, int direction)
        {
            if (!_blocks.Contains(block) || !block.CanMove(direction))
            {
                return false;
            }

            int oldRow = block.Row;
            int oldCol = block.Col;
            int newRow = oldRow;
            int newCol = oldCol;

            UpdateNewPosition(ref newRow, ref newCol, direction);


            if (IsValidMove(newRow, newCol, block))
            {
                ClearOccupiedCells(block);
                block.Move(newRow, newCol, _gridParameters.blockMoveTime);
                SetOccupiedCells(block);
                return true;
            }

            return false;
        }

        public void ClearBlocks()
        {
            foreach (var block in _blocks)
            {
                UnityEngine.Object.Destroy(block.gameObject);
            }
            _blocks.Clear();
        }

        public List<Block> GetBlocks()
        {
            return _blocks;
        }

        private Quaternion CalculateBlockRotation(int direction)
        {
            return Quaternion.Euler(0, direction % 2 == 0 ? direction * 90 + 90 : direction * 90 - 90, 0);
        }

        private int GetDirection(Block block, int targetRow, int targetCol)
        {
            int currentRow = block.Row;
            int currentCol = block.Col;

            if (targetRow < currentRow) return 0; // Up
            if (targetCol > currentCol) return 1; // Right
            if (targetRow > currentRow) return 2; // Down
            if (targetCol < currentCol) return 3; // Left

            return -1; // No movement or invalid direction
        }

        private void UpdateNewPosition(ref int row, ref int col, int direction)
        {
            switch (direction)
            {
                case 0: row--; break; // Up
                case 1: col++; break; // Right
                case 2: row++; break; // Down
                case 3: col--; break; // Left
            }
        }

        private bool IsValidMove(int row, int col, Block block)
        {
            int length = block.Length;
            for (int i = 0; i < length; i++)
            {
                int checkRow = row;
                int checkCol = col;
                int dir = GetDirection(block, checkRow, checkCol);
                if (dir == -1)
                {
                    return false;
                }
                if (dir % 2 == 0)
                {
                    checkRow += i;
                }
                else
                {
                    checkCol += i;
                }

                if (_cellManager.IsOutOfBounds(checkRow, checkCol))
                {
                    if (_gateManager.TryDestroyBlock(block, dir))
                    {
                        _cellManager.ClearOccupied(block);
                    }
                    return false;
                }

                if (_cellManager.IsOccupied(checkRow, checkCol, block))
                {
                    return false;
                }
            }
            return true;
        }

        private void ClearOccupiedCells(Block block)
        {
            _cellManager.ClearOccupied(block);
        }

        private void SetOccupiedCells(Block block)
        {
            int length = block.Length;
            for (int i = 0; i < length; i++)
            {
                int row = block.Row;
                int col = block.Col;
                if (block.Directions[0] % 2 == 0)
                {
                    row += i;
                }
                else
                {
                    col += i;
                }
                _cellManager.SetOccupied(row, col, block);
            }
        }
    }
}
