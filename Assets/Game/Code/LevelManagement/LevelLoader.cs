using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlocks
{
    [System.Serializable]
    public class LevelLoader
    {
        [SerializeField] private Grid _gridPrefab;
        
        private Grid _grid;
        public void LoadLevel(TextAsset levelAsset)
        {
            if (_grid == null)
            {
                _grid = Object.Instantiate(_gridPrefab);
            }
            LevelData levelData = JsonUtility.FromJson<LevelData>(levelAsset.text);
            _grid.ClearGrid();
            _grid.InitGrid(levelData);
        }
    }
}
