using ArtTest.Models;
using ArtTest.Utilities;
using System;
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

        private bool isGameOverCheckerInitialized = false;

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
            blockSpawnManager.OnBlockSpawned -= HandleOnBlockSpawned;
            blockSpawnManager.OnBlockSpawned += HandleOnBlockSpawned;

            blockSpawnManager.OnSpawnFinished -= HandleOnSpawnFinished;
            blockSpawnManager.OnSpawnFinished += HandleOnSpawnFinished;
        }

        private void HandleOnSpawnFinished()
        {
            CheckForGameOver();
        }

        private void HandleOnBlockSpawned(Block block)
        {
            var draggable = block.GetComponent<Draggable>();
            draggable.OnDragEnd += HandleOnDragEnd;
        }

        private void HandleOnDragEnd(Block block)
        {
            gridManager.TryClearLines();
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

            this.gridManager.OnLinesCheckFinished -= HandleOnLinesCheckFinished;
            this.gridManager.OnLinesCheckFinished += HandleOnLinesCheckFinished;
        }

        private void HandleOnLinesCheckFinished()
        {
            CheckForGameOver();
        }

        private void HandleLinesCleared(int linesCleared)
        {
            AddScore(linesCleared);
        }

        public void AddScore(int linesCleared)
        {
            if (linesCleared == 0)
            {
                return;
            }

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

        private void CheckForGameOver()
        {
            if (!isGameOverCheckerInitialized)
            {
                Debug.Log("Game over checker is not initialized. Initializing now...");
                isGameOverCheckerInitialized = true;
                return;
            }

            if (blockSpawnManager.ActiveBlocks.Count == 0)
            {
                Debug.Log("No active blocks to check. Skipping...");
                return;
            }

            foreach(var block in blockSpawnManager.ActiveBlocks)
            {
                if (CanPlaceBlock(block))
                {
                    Debug.Log("Block can be placed. Game continues.");
                    return;
                }
            }

            Debug.Log("No blocks can be placed. Game over.");
            TriggerGameEnd();
        }

        private bool CanPlaceBlock(Block block)
        {
            var draggable = block.GetComponent<Draggable>();

            foreach (var cell in gridManager.Cells)
            {
                Debug.Log("Checking cell at " + cell.transform.position);
                if (TrySimulatePlacement(block, cell.transform.position))
                {
                    draggable.ResetPosition();
                    return true;
                }
            }

            draggable.ResetPosition();
            return false;
        }

        private bool TrySimulatePlacement(Block block, Vector3 simulatedPosition)
        {
            Vector3 offset = simulatedPosition - block.CellVisuals[0].transform.position;

            foreach(var visual in block.CellVisuals)
            {
                Vector3 cellPosition = visual.transform.position + offset;
                Debug.Log("Simulating placement at " + cellPosition);
                Collider2D hit = Physics2D.OverlapPoint(cellPosition, LayerMask.GetMask("Grid"));
                if (hit == null || !hit.TryGetComponent<Cell>(out var cell) || cell.OccupyingBlock != null)
                {
                    return false;
                }
            }

            return true;
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
    }
}
