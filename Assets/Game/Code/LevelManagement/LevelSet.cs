using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlocks
{   
    [CreateAssetMenu(fileName = "LevelSet", menuName = "ColorBlocks/Data/LevelSet", order = 0)]
    public class LevelSet : ScriptableObject
    {
        [SerializeField] private TextAsset[] _levels;
        
        public TextAsset GetLevel(int index)
        {
            if (index < 0 || index >= _levels.Length)
            {
                return _levels[Random.Range(0, _levels.Length)];
            }
            return _levels[index];
        }
    }
}