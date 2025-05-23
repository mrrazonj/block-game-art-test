using ArtTest.Models;
using ArtTest.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArtTest.Game
{
    public class BlockGameManager : MonoBehaviour
    {
        public static BlockGameManager Instance;
        public AudioSource BgmPlayer;
        public AudioSource SfxPlayer;

        [SerializeField]
        private GameSettings gameSettings;
        [SerializeField]
        private GameTheme gameTheme;

        [SerializeField]
        private UIManager uiManager;
        [SerializeField]
        private GridManager gridManager;
        [SerializeField]
        private BlockSpawnManager blockSpawnManager;
        [SerializeField]
        private TextMeshPro scoreText;
        [SerializeField]
        private SpriteRenderer gameArea;

        private int score;
        private int highScore;

        private string highScoreHolderName = string.Empty;
        private const string highScoreKey = "highscore";

        private bool isGameOverCheckerInitialized = false;

        private string playerName = string.Empty;
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
                    highScoreHolderName = $"{data.PlayerName}'s";
                    UpdateUI();
                });

            BgmPlayer.clip = gameTheme.Bgm;
            BgmPlayer.Play();
            BgmPlayer.loop = true;

            gameArea.sprite = gameTheme.GridPanelSprite;

            ConnectUIManager(uiManager);
            ConnectBlockSpawnManager(blockSpawnManager);
            ConnectGridManager(gridManager);
            UpdateUI();
        }

        private void ConnectUIManager(UIManager uiManager = null)
        {
            if (uiManager == null && this.uiManager == null)
            {
                uiManager = FindFirstObjectByType<UIManager>();
                if (uiManager == null)
                {
                    Debug.LogError("UIManager is not assigned.");
                    return;
                }
            }

            if (uiManager != null)
            {
                uiManager = this.uiManager;
            }
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

            blockSpawnManager.Initialize(gameSettings, gameTheme);
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
            var draggable = block.GetComponent<DraggableBlock>();
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
            PlayPlaceSfx(linesCleared);
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
                highScoreHolderName = "Your";
            }

            OnScoreChanged?.Invoke(score);
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = 
                    $"Score: {score}\n" +
                    $"{highScoreHolderName} Highscore: {highScore}";
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
            var draggable = block.GetComponent<DraggableBlock>();

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
            uiManager.ShowGameOverUI(score, highScore);
        }

        public void SaveHighScore()
        {
            if (score < highScore)
            {
                Debug.Log("Score is less than high score. Not saving.");
                RestartGame();
                return;
            }

            playerName = uiManager.GetPlayerName();

            var entry = new Models.HighScoreEntry
            {
                PlayerName = playerName,
                Score = highScore
            };

            Utilities.Utilities.GetFileIO(highScoreKey).SaveToFile(highScoreKey, entry.ToJson());
            Debug.Log($"High score saved: {playerName} - {highScore}");

            RestartGame();
        }

        public void PlayPlaceSfx(int linesCleared)
        {
            int clampedIndex = Mathf.Clamp(linesCleared, 0, gameTheme.BlockPlaceSound.Length -1);
            SfxPlayer.PlayOneShot(gameTheme.BlockPlaceSound[0]);
            SfxPlayer.PlayOneShot(gameTheme.BlockPlaceSound[clampedIndex]);
        }

        public void PlayPickUpSfx()
        {
            SfxPlayer.PlayOneShot(gameTheme.BlockPickUpSound);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
