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
        }

        public void LevelCompleted()
        {
            StartCoroutine(FinishLevel(true));
        }

        IEnumerator FinishLevel(bool success){
            yield return new WaitForSeconds(1f);
            if(success){
                _currentLevelIndex++;
            }

            Debug.Log("Level: " + (success ? "Completed" : "Failed") + " " + _currentLevelIndex);
            LoadCurrentLevel();
        }

        public void LevelFailed()
        {
            StartCoroutine(FinishLevel(false));
        }

    }

}
