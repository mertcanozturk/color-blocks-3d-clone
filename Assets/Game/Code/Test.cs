using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ColorBlocks
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        public int rows = 10;
        public int cols = 10;
        [SerializeField] private TextAsset levelJson;

        [Button]
        public void InitGrid()
        {
            var levelData = JsonUtility.FromJson<LevelData>(levelJson.text);
            grid.InitGrid(levelData);
        }

    }

    [System.Serializable]
    public struct LevelData
    {
        public int MoveLimit;
        public int RowCount;
        public int ColCount;
        public List<CellInfo> CellInfo;
        public List<MovableInfo> MovableInfo;
        public List<ExitInfo> ExitInfo;
    }

    [System.Serializable]
    public struct CellInfo
    {
        public int Row;
        public int Col;
    }

    [System.Serializable]
    public struct MovableInfo
    {
        public int Row;
        public int Col;
        public List<int> Direction;
        public int Length;
        public int Colors;
    }

    [System.Serializable]
    public struct ExitInfo
    {
        public int Row;
        public int Col;
        public int Direction;
        public int Colors;
    }
}
