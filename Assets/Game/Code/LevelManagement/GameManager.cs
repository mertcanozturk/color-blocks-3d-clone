using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlocks
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private LevelLoader _levelLoader;
        [SerializeField] private LevelSet _levelSet;

        private int _currentLevelIndex = 0;
        private int _moves = 0;
        private int _currentLevel;
        public int CurrentLevel => _currentLevel;

        public Action<int> OnLevelStarted;
        public Action<int> OnLevelCompleted;
        public Action<int> OnLevelFailed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadCurrentLevel();

            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadCurrentLevel()
        {
            _levelLoader.LoadLevel(_levelSet.GetLevel(_currentLevelIndex));
            OnLevelStarted?.Invoke(_currentLevel);
        }

        public void LevelCompleted()
        {
            StartCoroutine(FinishLevel(true));
        }

        IEnumerator FinishLevel(bool success){
            yield return new WaitForSeconds(1f);
            if(success){
                _currentLevelIndex++;
                _currentLevel++;
                OnLevelCompleted?.Invoke(_currentLevel);
            }
            else
            {
                OnLevelFailed?.Invoke(_currentLevel);
            }
            LoadCurrentLevel();
        }

        public void LevelFailed()
        {
            StartCoroutine(FinishLevel(false));
        }

    }

}
