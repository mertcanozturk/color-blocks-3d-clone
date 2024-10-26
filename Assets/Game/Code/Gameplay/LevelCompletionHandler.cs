using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlocks
{
    public class LevelCompletionHandler
    {
        private LevelData _levelData;
        private IBlockManager _blockManager;
        private ICellManager _cellManager;

        public LevelCompletionHandler(Grid grid, LevelData levelData, IBlockManager blockManager, ICellManager cellManager)
        {
            _levelData = levelData;
            _blockManager = blockManager;
            _cellManager = cellManager;
            grid.OnBlockMoved -= OnBlockMoved;
            grid.OnBlockMoved += OnBlockMoved;
            grid.OnBlockDestroyed -= OnBlockDestroyed;
            grid.OnBlockDestroyed += OnBlockDestroyed;
        }

        internal void UpdateLevelData(LevelData levelData)
        {
            _levelData = levelData;
        }

        private void OnBlockMoved(Block block, int fromRow, int fromCol, int toRow, int toCol)
        {
            CheckLevelCompletion();
        }

        private void OnBlockDestroyed(Block block)
        {
            CheckLevelCompletion();
        }

        private void CheckLevelCompletion()
        {
            
            if (_levelData.MoveLimit > 0 && _blockManager.GetMoveCount() >= _levelData.MoveLimit)
            {
                GameManager.Instance.LevelFailed();
                return;
            }

            if (!_cellManager.HasOccupiedCells())
            {
                GameManager.Instance.LevelCompleted();
            }
        }

    }
}
