using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlocks
{
    public interface IGateManager
    {
        void CreateGates(LevelData levelData);
        void ClearGates();
        bool TryDestroyBlock(Block block, int direction);
    }
    public class GateManager : IGateManager
    {
        private List<Gate> _gates = new List<Gate>();
        private IGridFactory _gridFactory;
        private ICellManager _cellManager;
        private GridParameters _gridParameters;

        public GateManager(IGridFactory gridFactory, ICellManager cellManager, GridParameters gridParameters)
        {
            _gridFactory = gridFactory;
            _cellManager = cellManager;
            _gridParameters = gridParameters;
        }

        public void CreateGates(LevelData levelData)
        {
            ClearGates();
            foreach (var exitInfo in levelData.ExitInfo)
            {
                Vector3 position = CalculateGatePosition(exitInfo);
                Quaternion rotation = Quaternion.Euler(0, exitInfo.Direction * 90, 0);
                Gate gate = _gridFactory.CreateGate(position, rotation, exitInfo);
                _gates.Add(gate);
            }
        }

        public void ClearGates()
        {
            foreach (var gate in _gates)
            {
                UnityEngine.Object.Destroy(gate.gameObject);
            }
            _gates.Clear();
        }

        public bool TryDestroyBlock(Block block, int direction)
        {
            foreach (var gate in _gates)
            {
                bool isVerticalGate = gate.Direction % 2 == 0;
                bool isAligned = isVerticalGate ? block.Col == gate.Col : block.Row == gate.Row;

                if (isAligned && gate.IsMatch(block.Color, direction))
                {
                    gate.SmashBlock(block);
                    return true;
                }
            }
            return false;
        }

        private Vector3 CalculateGatePosition(ExitInfo exitInfo)
        {
            Vector3 position = _cellManager.GetCellPosition(exitInfo.Row, exitInfo.Col);
            switch (exitInfo.Direction)
            {
                case 0: position.z += _gridParameters.gateOffset; break;
                case 1: position.x += _gridParameters.gateOffset; break;
                case 2: position.z -= _gridParameters.gateOffset; break;
                case 3: position.x -= _gridParameters.gateOffset; break;
            }
            return position;
        }
    }

}
