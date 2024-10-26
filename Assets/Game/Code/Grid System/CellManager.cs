using UnityEngine;

namespace ColorBlocks
{
    public interface ICellManager
    {
        void CreateCells(int cols, int rows, float totalWidth, float totalHeight);
        Vector3 GetCellPosition(int row, int col);
        bool IsOccupied(int row, int col, Block block);
        void SetOccupied(int row, int col, Block block);
        void ClearOccupied(Block block);
        bool IsOutOfBounds(int row, int col);
        bool IsEdge(Block block);
        void ClearCells();
    }

    public class CellManager : ICellManager
    {
        private Cell[,] _cells;
        private IGridFactory _gridFactory;
        private GridParameters _gridParameters;

        public CellManager(IGridFactory gridFactory, GridParameters gridParameters)
        {
            _gridFactory = gridFactory;
            _gridParameters = gridParameters;
        }

        public void CreateCells(int cols, int rows, float totalWidth, float totalHeight)
        {
            _cells = new Cell[cols, rows];
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector3 position = CalculateCellPosition(x, y, cols, rows, totalWidth, totalHeight);
                    _cells[x, y] = _gridFactory.CreateCell(position, x, y);
                }
            }
        }

        public Vector3 GetCellPosition(int row, int col)
        {
            return _cells[col, row].transform.position;
        }

        public bool IsOccupied(int row, int col, Block block)
        {
            return _cells[col, row].IsOccupied() && _cells[col, row].occupyingBlock != block;
        }

        public void SetOccupied(int row, int col, Block block)
        {
            _cells[col, row].SetBlock(block);
        }

        public void ClearOccupied(Block block)
        {
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    if (_cells[i, j].occupyingBlock == block)
                    {
                        _cells[i, j].ClearBlock();
                    }
                }
            }
        }

        public bool IsOutOfBounds(int row, int col)
        {
            return row < 0 || row >= _cells.GetLength(1) ||
                   col < 0 || col >= _cells.GetLength(0);
        }

        public bool IsEdge(Block block)
        {
            int row = block.Row;
            int col = block.Col;
            int direction = block.Directions[0];
            if (direction % 2 == 0)
            {
                row += block.Length - 1;
            }

            return row == 0 || row == _cells.GetLength(1) - 1 ||
                   col == 0 || col == _cells.GetLength(0) - 1;
        }

        public void ClearCells()
        {
            if (_cells == null) return;
            foreach (var cell in _cells)
            {
                Object.Destroy(cell.gameObject);
            }
            _cells = null;
        }

        private Vector3 CalculateCellPosition(int x, int y, int cols, int rows, float totalWidth, float totalHeight)
        {
            Vector3 position = new Vector3(
                x * (_gridParameters.cellSize + _gridParameters.cellGap),
                0,
                (rows - y - 1) * (_gridParameters.cellSize + _gridParameters.cellGap)
            );
            position.x -= totalWidth / 2;
            position.z -= totalHeight / 2;
            return position;
        }
    }
}

