using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ColorBlocks.BlockManager;

namespace ColorBlocks
{
    public interface IGridSystem
    {
        void InitGrid(LevelData levelData);
        bool MoveBlock(int dragID,Block block, int direction);
        Vector3 GetCellPosition(int row, int col);
    }
    public class Grid : MonoBehaviour, IGridSystem
    {
        [SerializeField] private GameObject plane;
        [SerializeField] private Pool<Cell> cellPrefab;
        [SerializeField] private Pool<Block>[] blockPrefabs;
        [SerializeField] private Pool<Gate> gatePrefab;
        [SerializeField] private GridParameters gridParameters;

        public static Grid Instance { get; private set; }
        public int MaxMoves => _maxMoves;
        public delegate void BlockMovedHandler(Block block, int fromRow, int fromCol, int toRow, int toCol);
        public delegate void BlockDestroyedHandler(Block block);
        public event BlockMovedHandler OnBlockMoved;
        public event BlockDestroyedHandler OnBlockDestroyed;
        public delegate void MoveCountChangedHandler(int moves);
        public event MoveCountChangedHandler OnMoveCountChanged;

        private ICellManager cellManager;
        private IBlockManager blockManager;
        private IGateManager gateManager;
        private IGridFactory gridFactory;
        private LevelCompletionHandler levelCompletionHandler;
        private int _maxMoves;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeManagers();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeManagers()
        {
            cellPrefab.Init(16);
            gatePrefab.Init(16);
            for (int i = 0; i < blockPrefabs.Length; i++)   
            {
                blockPrefabs[i].Init(10);
            }

            gridFactory = new GridFactory(cellPrefab, blockPrefabs, gatePrefab);
            cellManager = new CellManager(gridFactory, gridParameters);
            gateManager = new GateManager(gridFactory, cellManager, gridParameters);
            blockManager = new BlockManager(gridFactory, cellManager, gridParameters, gateManager);
        }

        public void InitGrid(LevelData levelData)
        {
            int cols = levelData.ColCount;
            int rows = levelData.RowCount;

            ClearAll();
            SetupPlane(cols, rows);

            float totalWidth = cols * gridParameters.cellSize + (cols - 1) * gridParameters.cellGap;
            float totalHeight = rows * gridParameters.cellSize + (rows - 1) * gridParameters.cellGap;

            cellManager.CreateCells(cols, rows, totalWidth - gridParameters.cellSize, totalHeight - gridParameters.cellSize);
            blockManager.CreateBlocks(levelData);
            gateManager.CreateGates(levelData);
            if (levelCompletionHandler == null)
            {
                levelCompletionHandler = new LevelCompletionHandler(this, levelData, blockManager, cellManager);
            }
            else
            {
                levelCompletionHandler.UpdateLevelData(levelData);
            }
            _maxMoves = levelData.MoveLimit;
        }

        private void SetupPlane(int cols, int rows)
        {
            float totalWidth = cols * gridParameters.cellSize + (cols - 1) * gridParameters.cellGap;
            float totalHeight = rows * gridParameters.cellSize + (rows - 1) * gridParameters.cellGap;
            plane.transform.localScale = new Vector3(totalWidth / 10, 1, totalHeight / 10);
        }

        private void ClearAll()
        {
            cellManager?.ClearCells();
            blockManager?.ClearBlocks();
            gateManager?.ClearGates();
        }

        public Vector3 GetCellPosition(int row, int col)
        {
            return cellManager.GetCellPosition(row, col);
        }

        public bool MoveBlock(int dragID,Block block, int direction)
        {
            int oldRow = block.Row;
            int oldCol = block.Col;

            BlockMovedEventArgs moved = blockManager.MoveBlock(dragID, block, direction);
        
            if (moved.result == BlockMovedEventArgs.BlockMoveResult.moved)
            {
                OnBlockMoved?.Invoke(block, oldRow, oldCol, block.Row, block.Col);

                if (cellManager.IsOutOfBounds(block.Row, block.Col))
                {
                    if (gateManager.TryDestroyBlock(block, direction))
                    {
                        cellManager.ClearOccupied(block);
                        OnBlockDestroyed?.Invoke(block);
                    }
                }
            }
            else if (moved.result == BlockMovedEventArgs.BlockMoveResult.destroyed)
            {
                OnBlockDestroyed?.Invoke(block);
            }

            OnMoveCountChanged?.Invoke(blockManager.GetMoveCount());
            
            return moved.result == BlockMovedEventArgs.BlockMoveResult.moved;
        }

        internal void ClearGrid()
        {
            ClearAll();
        }
    }
        [System.Serializable]
        public struct GridParameters
        {
            public float cellSize;
            public float cellGap;
            public float gateOffset;
            public float blockMoveTime;
        }
}
