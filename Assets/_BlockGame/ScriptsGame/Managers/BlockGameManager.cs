using ArtTest.Models;
using ArtTest.Utilities;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ArtTest.Game
{
    public class BlockGameManager : MonoBehaviour
    {
        public static BlockGameManager Instance;

        [SerializeField]
        private GameSettings gameSettings;
        [SerializeField]
        private GameTheme gameTheme;

        [SerializeField]
        private GridManager gridManager;
        [SerializeField]
        private BlockSpawnManager blockSpawnManager;
        [SerializeField]
        private TextMeshProUGUI scoreText, highScoreText;

        private int score;
        private int highScore;

        private const string highScoreKey = "highscore";

        public event Action<int> OnScoreChanged;
        public event Action OnGameEnd;

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

            Utilities.Utilities.GetFileIO(highScoreKey).LoadFromFile(highScoreKey)
                .Then(result =>
                {
                    var data = result.FromJson<Models.HighScoreEntry>();
                    highScore = data.Score;

                    UpdateUI();
                });

            ConnectBlockSpawnManager(blockSpawnManager);
            ConnectGridManager(gridManager);
            UpdateUI();
        }

        private void ConnectBlockSpawnManager(BlockSpawnManager blockSpawnManager = null)
        {
            if (blockSpawnManager == null && this.blockSpawnManager == null)
            {
                blockSpawnManager = FindFirstObjectByType<BlockSpawnManager>();
                if (blockSpawnManager == null)
                {
                    Debug.LogError("BlockSpawnManager is not assigned.");
                    return;
                }
            }

            if (blockSpawnManager != null)
            {
                blockSpawnManager = this.blockSpawnManager;
            }

            blockSpawnManager.Initialize(gameSettings);
            blockSpawnManager.OnBlockSpawned += HandleBlockSpawned;
        }

        private void HandleBlockSpawned(Block block)
        {
            block.GetComponent<Draggable>().OnDragEnd += HandleBlockDragEnd;
        }

        private void HandleBlockDragEnd(Block block)
        {
            var gridPosition = gridManager.GetClosestGridPosition(block.transform.position);

            if (gridManager.CheckValidPlacement(block, gridPosition))
            {
                gridManager.PlaceBlock(block, gridPosition);
                blockSpawnManager.ActiveBlocks.Remove(block);
                block.GetComponent<Draggable>().OnDragEnd -= HandleBlockDragEnd;
                block.GetComponent<Draggable>().enabled = false;

                if (blockSpawnManager.ActiveBlocks.Count == 0)
                {
                    blockSpawnManager.SpawnBlocks();

                    if (!blockSpawnManager.CanPlaceAnyBlock(gridManager, gameSettings))
                    {
                        TriggerGameEnd();
                    }
                }

                return;
            }

            block.GetComponent<Draggable>().ResetPosition();
        }

        private void ConnectGridManager(GridManager gridManager = null)
        {
            if (gridManager == null && this.gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridManager>();
                if (gridManager == null)
                {
                    Debug.LogError("GridManager is not assigned.");
                    return;
                }
            }

            if (gridManager != null)
            {
                gridManager = this.gridManager;
            }

            gridManager.Initialize(gameSettings);

            this.gridManager.OnLinesCleared -= HandleLinesCleared;
            this.gridManager.OnLinesCleared += HandleLinesCleared;
        }

        public void AddScore(int linesCleared)
        {
            ;
            var scoreData = gameSettings.ScorePerLineCleared;
            score += scoreData[Mathf.Clamp(linesCleared - 1, 0, scoreData.Length - 1)];
            if (score > highScore)
            {
                highScore = score;
            }

            OnScoreChanged?.Invoke(score);
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"High Score: {highScore}";
            }
        }

        public void TriggerGameEnd()
        {
            OnGameEnd?.Invoke();
        }

        public void SaveHighScore(string playerName)
        {
            var entry = new Models.HighScoreEntry
            {
                PlayerName = playerName,
                Score = highScore
            };
            Utilities.Utilities.GetFileIO(highScoreKey).SaveToFile(highScoreKey, entry.ToJson());
        }

        private void HandleLinesCleared(int linesCleared)
        {
            AddScore(linesCleared);
        }
    }
}
