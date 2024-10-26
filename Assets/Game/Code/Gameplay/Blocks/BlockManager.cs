using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlocks
{
    public interface IBlockManager
    {
        public int GetMoveCount();
        void CreateBlocks(LevelData levelData);
        BlockMovedEventArgs MoveBlock(int dragID, Block block, int direction);
        void ClearBlocks();

    }

    public class BlockManager : IBlockManager
    {
        private List<Block> _blocks = new List<Block>();
        private IGridFactory _gridFactory;
        private ICellManager _cellManager;
        private GridParameters _gridParameters;
        private IGateManager _gateManager;
        private int _moveCount = 0;
        private int _lastDragID = -1;

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
            _moveCount = 0;
            foreach (var movableInfo in levelData.MovableInfo)
            {
                Vector3 position = _cellManager.GetCellPosition(movableInfo.Row, movableInfo.Col);
                Quaternion rotation = CalculateBlockRotation(movableInfo.Direction[0]);
                Block block = _gridFactory.CreateBlock(position, rotation, movableInfo);
                _blocks.Add(block);
                SetOccupiedCells(block);
            }
        }

        public BlockMovedEventArgs MoveBlock(int dragID, Block block, int direction)
        {
            if (!_blocks.Contains(block) || !block.CanMove(direction))
            {
                return new BlockMovedEventArgs { result = BlockMovedEventArgs.BlockMoveResult.failed };
            }

            int oldRow = block.Row;
            int oldCol = block.Col;
            int newRow = oldRow;
            int newCol = oldCol;

            UpdateNewPosition(ref newRow, ref newCol, direction);


            BlockMovedEventArgs.BlockMoveResult result = TryToMove(newRow, newCol, block);
            if (result == BlockMovedEventArgs.BlockMoveResult.moved)
            {
                ClearOccupiedCells(block);
                block.Move(newRow, newCol, _gridParameters.blockMoveTime);
                SetOccupiedCells(block);
                if (_lastDragID != dragID)
                {
                    _moveCount++;
                    _lastDragID = dragID;
                }

            }
            else if (result == BlockMovedEventArgs.BlockMoveResult.destroyed)
            {
                _blocks.Remove(block);
                ClearOccupiedCells(block);
                if (_lastDragID != dragID)
                {
                    _moveCount++;
                    _lastDragID = dragID;
                }
                return new BlockMovedEventArgs { result = result };
            }

            return new BlockMovedEventArgs
            {
                result = result,
                block = block,
                fromRow = oldRow,
                fromCol = oldCol,
                toRow = newRow,
                toCol = newCol
            };
        }

        public int GetMoveCount()
        {
            return _moveCount;
        }

        public void ClearBlocks()
        {
            foreach (var block in _blocks)
            {
                block.gameObject.SetActive(false);
            }
            _blocks.Clear();
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

        private BlockMovedEventArgs.BlockMoveResult TryToMove(int row, int col, Block block)
        {
            int length = block.Length;
            for (int i = 0; i < length; i++)
            {
                int checkRow = row;
                int checkCol = col;
                int dir = GetDirection(block, checkRow, checkCol);
                if (dir == -1)
                {
                    return BlockMovedEventArgs.BlockMoveResult.failed;
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
                        return BlockMovedEventArgs.BlockMoveResult.destroyed;
                    }
                    return BlockMovedEventArgs.BlockMoveResult.failed;

                }
                else if (_cellManager.IsOccupied(checkRow, checkCol, block))
                {
                    return BlockMovedEventArgs.BlockMoveResult.failed;
                }

            }
            return BlockMovedEventArgs.BlockMoveResult.moved;
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

    public struct BlockMovedEventArgs
    {
        public enum BlockMoveResult
        {
            destroyed,
            moved,
            failed
        }
        public BlockMoveResult result;
        public Block block;
        public int fromRow;
        public int fromCol;
        public int toRow;
        public int toCol;
    }
}
