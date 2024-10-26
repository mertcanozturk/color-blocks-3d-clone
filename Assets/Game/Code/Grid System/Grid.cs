using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlocks
{
    public interface IGridSystem
    {
        void InitGrid(LevelData levelData);
        bool MoveBlock(Block block, int direction);
        Vector3 GetCellPosition(int row, int col);
    }
    public class Grid : MonoBehaviour, IGridSystem
    {
        [SerializeField] private GameObject plane;
        [SerializeField] private Cell cellPrefab;
        [SerializeField] private Block[] blockPrefabs;
        [SerializeField] private Gate gatePrefab;
        [SerializeField] private GridParameters gridParameters;

        public static Grid Instance { get; private set; }
        public delegate void BlockMovedHandler(Block block, int fromRow, int fromCol, int toRow, int toCol);
        public event BlockMovedHandler OnBlockMoved;

        private ICellManager cellManager;
        private IBlockManager blockManager;
        private IGateManager gateManager;
        private IGridFactory gridFactory;

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
            gridFactory = new GridFactory(cellPrefab, blockPrefabs, gatePrefab, gridParameters);
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

        public bool MoveBlock(Block block, int direction)
        {
            int oldRow = block.Row;
            int oldCol = block.Col;

            bool moved = blockManager.MoveBlock(block, direction);

            if (moved)
            {
                OnBlockMoved?.Invoke(block, oldRow, oldCol, block.Row, block.Col);

                if (cellManager.IsOutOfBounds(block.Row, block.Col))
                {
                    if (gateManager.TryDestroyBlock(block, direction))
                    {
                        cellManager.ClearOccupied(block);
                    }
                }
            }
            return moved;
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
