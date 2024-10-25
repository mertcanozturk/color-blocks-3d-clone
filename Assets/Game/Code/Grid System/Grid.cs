using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;
namespace ColorBlocks
{

    public class Grid : MonoBehaviour
    {
        [SerializeField] private GameObject plane;
        [SerializeField] private Cell cellPrefab;
        [SerializeField] private Block[] blockPrefabs;
        [SerializeField] private Gate gatePrefab;
        [SerializeField] private GridParameters gridParameters;

        public static Grid Instance { get; private set; }
        public delegate void BlockMovedHandler(Block block, int fromRow, int fromCol, int toRow, int toCol);
        public event BlockMovedHandler OnBlockMoved;

        private Cell[,] _cells;
        private List<Block> _blocks;
        private List<Gate> _gates;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitGrid(LevelData levelData)
        {
            int cols = levelData.ColCount;
            int rows = levelData.RowCount;


            ClearAll();
            float totalWidth = cols * gridParameters.cellSize + (cols - 1) * gridParameters.cellGap;
            float totalHeight = rows * gridParameters.cellSize + (rows - 1) * gridParameters.cellGap;

            plane.transform.localScale = new Vector3(totalWidth / 10, 1, totalHeight / 10);

            CreateCells(cols, rows, totalWidth - gridParameters.cellSize, totalHeight - gridParameters.cellSize);
            CreateBlocks(levelData);
            CreateGates(levelData);
        }

        private void CreateCells(int cols, int rows, float totalWidth, float totalHeight)
        {
            _cells = new Cell[cols, rows];
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector3 position = new Vector3(x * (gridParameters.cellSize + gridParameters.cellGap), 0, (rows - y - 1) * (gridParameters.cellSize + gridParameters.cellGap));
                    position.x -= totalWidth / 2;
                    position.z -= totalHeight / 2;
                    _cells[x, y] = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                    _cells[x, y].col = x;
                    _cells[x, y].row = y;
                }
            }
        }

        private void CreateBlocks(LevelData levelData)
        {
            if (_blocks == null)
            {
                _blocks = new List<Block>();
            }

            for (int i = 0; i < levelData.MovableInfo.Count; i++)
            {
                var movableInfo = levelData.MovableInfo[i];
                if (movableInfo.Length > blockPrefabs.Length)
                {
                    Debug.LogError("Block length is greater than the number of block prefabs");
                    continue;
                }
                var blockPosition = _cells[movableInfo.Col, movableInfo.Row].transform.position;
                var dir = movableInfo.Direction[0];
                var blockRotation = Quaternion.Euler(0, dir % 2 == 0 ? dir * 90 + 90 : dir * 90 - 90, 0);
                var block = Instantiate(blockPrefabs[movableInfo.Length - 1], blockPosition, blockRotation);
                block.transform.parent = transform;
                block.SetValues(movableInfo.Row, movableInfo.Col, movableInfo.Direction, movableInfo.Colors, movableInfo.Length);
                _blocks.Add(block);
                SetOccupiedGridElements(block);
            }
        }

        private void CreateGates(LevelData levelData)
        {
            if (_gates == null)
            {
                _gates = new List<Gate>();
            }

            for (int i = 0; i < levelData.ExitInfo.Count; i++)
            {
                var exitInfo = levelData.ExitInfo[i];
                var gatePosition = _cells[exitInfo.Col, exitInfo.Row].transform.position;
                switch (exitInfo.Direction)
                {
                    case 0:
                        gatePosition.z += gridParameters.gateOffset;
                        break;
                    case 1:
                        gatePosition.x += gridParameters.gateOffset;
                        break;
                    case 2:
                        gatePosition.z -= gridParameters.gateOffset;
                        break;
                    case 3:
                        gatePosition.x -= gridParameters.gateOffset;
                        break;
                }
                var gateRotation = Quaternion.Euler(0, exitInfo.Direction * 90, 0);
                var gate = Instantiate(gatePrefab, gatePosition, gateRotation);
                gate.transform.parent = transform;
                gate.Init(exitInfo.Row, exitInfo.Col, exitInfo.Direction, exitInfo.Colors);
                _gates.Add(gate);
            }
        }

        private void ClearAll()
        {
            if (_cells == null) return;
            for (int x = 0; x < _cells.GetLength(0); x++)
            {
                for (int y = 0; y < _cells.GetLength(1); y++)
                {
                    Destroy(_cells[x, y].gameObject);
                }
            }

            if (_blocks != null)
            {
                for (int i = 0; i < _blocks.Count; i++)
                {
                    Destroy(_blocks[i].gameObject);
                }
            }
            _blocks.Clear();
        }

        public Vector3 GetCellPosition(int row, int col)
        {
            return _cells[col, row].transform.position;
        }

        public bool MoveBlock(Block block, int direction)
        {
            if(!_blocks.Contains(block))
            {
                return false;
            }

            if (!block.CanMove(direction))
            {
                Debug.Log($"Block cannot move in direction {direction}");
                return false;
            }

            int oldRow = block.Row;
            int oldCol = block.Col;
            int newRow = oldRow;
            int newCol = oldCol;

            switch (direction)
            {
                case 0: newRow--; break; // Up
                case 1: newCol++; break; // Right
                case 2: newRow++; break; // Down
                case 3: newCol--; break; // Left
            }

            if (IsOutOfBounds(newRow, newCol))
            {
                TryDestroyBlock(block, direction);
                return false;
            }
            else
            {
                if (IsValidMove(newRow, newCol, block))
                {
                    ClearOccupiedGridElements(block);
                    block.Move(newRow, newCol, gridParameters.blockMoveTime);
                    SetOccupiedGridElements(block);

                    OnBlockMoved?.Invoke(block, oldRow, oldCol, newRow, newCol);
                    Debug.Log($"Block moved from ({oldCol}, {oldRow}) to ({newCol}, {newRow})");
                    return true;
                }
            }

            return false;
        }

        private bool IsValidMove(int row, int col, Block block)
        {
            int length = block.Length;
            for (int i = 0; i < length; i++)
            {
                var dir = CalculateDirection(row, col, block);
                if (dir % 2 == 0)
                {
                    if (IsOutOfBounds(row + i, col))
                    {
                        TryDestroyBlock(block, dir);
                        return false;
                    }
                    var element = _cells[col, row + i];
                    if (element.IsOccupied() && element.occupyingBlock != block)
                    {
                        return false;
                    }
                }
                else
                {
                    if (IsOutOfBounds(row, col + i))
                    {
                        TryDestroyBlock(block, dir);
                        return false;
                    }
                    var element = _cells[col + i, row];
                    if (element.IsOccupied() && element.occupyingBlock != block)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private int CalculateDirection(int moveRow, int moveCol, Block block)
        {
            //- 0: Up
            //- 1: Right
            //- 2: Down
            //- 3: Left
            int dir = 0;
            if (moveRow < block.Row) dir = 0;
            else if (moveRow > block.Row) dir = 2;
            else if (moveCol > block.Col) dir = 1;
            else if (moveCol < block.Col) dir = 3;
            return dir;
        }

        IEnumerator DestroyBlock(Block block, Gate gate)
        {            
            _blocks.Remove(block);
            yield return new WaitForSeconds(gridParameters.blockMoveTime);
            gate.SmashBlock(block);
            ClearOccupiedGridElements(block);

            if(_blocks.Count == 0)
            {
                //Game Finished
                Debug.Log("Game Finished");
            }
        }

        private void TryDestroyBlock(Block block, int direction)
        {
            for (int i = 0; i < _gates.Count; i++)
            {
                var gate = _gates[i];
                var dir = gate.Direction;

                bool isVerticalGate = dir % 2 == 0;
                bool isAligned = isVerticalGate ? block.Col == gate.Col : block.Row == gate.Row;

                if (isAligned && gate.IsMatch(block.Color, direction))
                {
                    StartCoroutine(DestroyBlock(block, gate));
                    return;
                }
            }
        }

        private bool IsOutOfBounds(int row, int col)
        {
            return row < 0 || row >= _cells.GetLength(1) ||
                   col < 0 || col >= _cells.GetLength(0);
        }

        private void ClearOccupiedGridElements(Block block)
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

        private void SetOccupiedGridElements(Block block)
        {
            int length = block.Length;
            for (int i = 0; i < length; i++)
            {
                var dir = block.Directions[0];
                if (dir % 2 == 0)
                {
                    _cells[block.Col, block.Row + i].SetBlock(block);
                }
                else
                {
                    _cells[block.Col + i, block.Row].SetBlock(block);
                }
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
}
