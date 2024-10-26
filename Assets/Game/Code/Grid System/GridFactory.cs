using UnityEngine;

namespace ColorBlocks
{
    public class GridFactory : IGridFactory
    {
        private Cell _cellPrefab;
        private Block[] _blockPrefabs;
        private Gate _gatePrefab;
        private GridParameters _gridParameters;

        public GridFactory(Cell cellPrefab, Block[] blockPrefabs, Gate gatePrefab, GridParameters gridParameters)
        {
            _cellPrefab = cellPrefab;
            _blockPrefabs = blockPrefabs;
            _gatePrefab = gatePrefab;
            _gridParameters = gridParameters;
        }

        public Cell CreateCell(Vector3 position, int col, int row)
        {
            Cell cell = Object.Instantiate(_cellPrefab, position, Quaternion.identity);
            cell.col = col;
            cell.row = row;
            return cell;
        }

        public Block CreateBlock(Vector3 position, Quaternion rotation, MovableInfo movableInfo)
        {
            if (movableInfo.Length > _blockPrefabs.Length)
            {
                Debug.LogError("Block length is greater than the number of block prefabs");
                return null;
            }

            Block block = Object.Instantiate(_blockPrefabs[movableInfo.Length - 1], position, rotation);
            block.SetValues(movableInfo.Row, movableInfo.Col, movableInfo.Direction, movableInfo.Colors, movableInfo.Length);
            return block;
        }

        public Gate CreateGate(Vector3 position, Quaternion rotation, ExitInfo exitInfo)
        {
            Gate gate = Object.Instantiate(_gatePrefab, position, rotation);
            gate.Init(exitInfo.Row, exitInfo.Col, exitInfo.Direction, exitInfo.Colors);
            return gate;
        }
    }
    public interface IGridFactory
    {
        Cell CreateCell(Vector3 position, int col, int row);
        Block CreateBlock(Vector3 position, Quaternion rotation, MovableInfo movableInfo);
        Gate CreateGate(Vector3 position, Quaternion rotation, ExitInfo exitInfo);
    }
}