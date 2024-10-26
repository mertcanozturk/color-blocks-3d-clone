using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ColorBlocks.Menus
{
    public class GameHud : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _movesText;

        private void Start()
        {
            GameManager.Instance.OnLevelStarted += OnLevelStarted;
            GameManager.Instance.OnLevelCompleted += OnLevelCompleted;
            GameManager.Instance.OnLevelFailed += OnLevelFailed;
            UpdateLevelText(GameManager.Instance.CurrentLevel);
            OnMoveCountChanged(0);
            Grid.Instance.OnMoveCountChanged += OnMoveCountChanged;
        }

        private void OnMoveCountChanged(int moves)
        {
            UpdateMoveCountText(moves, Grid.Instance.MaxMoves);
        }

        private void OnLevelFailed(int level)
        {
            UpdateLevelText(level);
        }

        private void OnLevelCompleted(int level)
        {
            UpdateLevelText(level);
        }

        private void OnLevelStarted(int level)
        {
            UpdateLevelText(level);
            UpdateMoveCountText(0, Grid.Instance.MaxMoves);
        }

        private void UpdateLevelText(int level)
        {
            _levelText.text = $"Lv. {level + 1}";
        }
        private void UpdateMoveCountText(int moves, int maxMoves)
        {
            if (maxMoves == 0)
            {
                _movesText.text = $"Moves: {moves}";
            }
            else
            {
                _movesText.text = $"Moves: {moves}/{maxMoves}";
            }
        }

    }
}