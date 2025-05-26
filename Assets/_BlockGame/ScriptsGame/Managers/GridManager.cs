using ArtTest.Models;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ArtTest.Game
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject gameAreaObject;
        [SerializeField]
        private GameObject gridPrefab;

        private Dictionary<int, GameObject> scoreToEffectObjectMap;

        private int gridWidth = 9;
        private int gridHeight = 9;
        public List<Cell> Cells;
        private Cell[,] gridCells;

        private bool isInitialized = false;
        private HashSet<Cell> cellsToClear = new HashSet<Cell>();
        private HashSet<Block> blocksToCheck = new HashSet<Block>();

        private ObjectPool<Block> blocksPool;
        private ObjectPool<GameObject> visualsPool;

        public event Action<int> OnLinesCleared;
        public event Action OnLinesCheckFinished;

        public void Initialize(GameSettings gameSettings, GameTheme gameTheme, ref ObjectPool<Block> blocksPool, ref ObjectPool<GameObject> visualsPool)
        {
            gridWidth = gameSettings.GridWidth;
            gridHeight = gameSettings.GridHeight;
            gridCells = new Cell[gridWidth, gridHeight];
            Cells = new();

            scoreToEffectObjectMap = new Dictionary<int, GameObject>();
            scoreToEffectObjectMap.TryAdd(0, null);
            scoreToEffectObjectMap.TryAdd(1, gameTheme.Clear1);
            scoreToEffectObjectMap.TryAdd(2, gameTheme.Clear2);
            scoreToEffectObjectMap.TryAdd(3, gameTheme.Clear3);
            scoreToEffectObjectMap.TryAdd(4, gameTheme.Clear4OrMore);
            scoreToEffectObjectMap.TryAdd(default, gameTheme.Clear4OrMore);

            this.blocksPool = blocksPool;
            this.visualsPool = visualsPool;

            var areaWidth = gameAreaObject.GetComponent<SpriteRenderer>().bounds.size.x;
            var areaHeight = gameAreaObject.GetComponent<SpriteRenderer>().bounds.size.y;

            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    var cellObject = Instantiate(gridPrefab, gameAreaObject.transform);
                    cellObject.transform.localPosition = new Vector3(
                        (i - gridWidth / 2) * (areaWidth / gridWidth),
                        (j - gridHeight / 2) * (areaHeight / gridHeight),
                        -1
                    );

                    var cell = cellObject.GetComponent<Cell>();
                    if (!cell)
                    {
                        cell = cellObject.AddComponent<Cell>();
                    }

                    cell.x = i;
                    cell.y = j;

                    gridCells[i, j] = cell;
                    Cells.Add(cell);
                    Debug.Log($"GridManager.Initialize: Created cell at ({i}, {j}) with position {cellObject.transform.localPosition}");
                }
            }

            isInitialized = true;
        }

        public async void TryClearLines()
        {
            int linesCleared = 0;
            List<Tuple<bool, Vector3>> effectPositions = new();

            // Check rows
            for (int y = 0; y < gridHeight; y++)
            {
                bool isFullRow = true;
                for (int x = 0; x < gridWidth; x++)
                {
                    if (gridCells[x, y].OccupyingBlock == null)
                    {
                        isFullRow = false;
                        break;
                    }
                }

                if (isFullRow)
                {
                    linesCleared++;
                    for (int x = 0; x < gridWidth; x++)
                    {
                        cellsToClear.Add(gridCells[x, y]);
                    }

                    var effectPos = gridCells[gridWidth / 2, y].transform.position;
                    effectPositions.Add(new Tuple<bool, Vector3>(true, effectPos));
                }
            }

            // Check columns
            for (int x = 0; x < gridWidth; x++)
            {
                bool isFullColumn = true;
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridCells[x, y].OccupyingBlock == null)
                    {
                        isFullColumn = false;
                        break;
                    }
                }

                if (isFullColumn)
                {
                    linesCleared++;
                    for (int y = 0; y < gridHeight; y++)
                    {
                        cellsToClear.Add(gridCells[x, y]);
                    }

                    var effectPos = gridCells[x, gridHeight / 2].transform.position;
                    effectPositions.Add(new Tuple<bool, Vector3>(false, effectPos));
                }
            }

            // Spawn line clear effects
            foreach (var effect in effectPositions)
            {
                if (scoreToEffectObjectMap.TryGetValue(linesCleared, out GameObject effectPrefab))
                {
                    if (effectPrefab != null)
                    {
                        var effectInstance = Instantiate(effectPrefab, effect.Item2 + new Vector3(0, 0, -5), Quaternion.identity);
                        effectInstance.transform.localEulerAngles += new Vector3(0, 180, 0);
                        if (effect.Item1)
                        {
                            effectInstance.transform.localEulerAngles += new Vector3(0, 0, 90);
                        }
                        Destroy(effectInstance, 1.5f); // Destroy after 1 second
                    }
                }
            }

            if (effectPositions.Count > 0)
            {
                await UniTask.Delay(1500); // Wait for effects to play
            }

            // Release blocks in the cells to clear
            var cellsToClearCopy = new List<Cell>(cellsToClear);
            foreach (Cell cell in cellsToClearCopy)
            {
                var block = cell.OccupyingBlock.transform.parent.GetComponent<Block>();
                switch (linesCleared)
                {
                    case 0:
                        break;

                    case 1:
                        blocksToCheck.Add(block);

                        LeanTween.color(cell.OccupyingBlock, Color.black, 0.5f);
                        LeanTween.scale(cell.OccupyingBlock, Vector3.zero, 0.3f).setEaseInBack().setOnComplete(() =>
                        {
                            block.CellVisuals.Remove(cell.OccupyingBlock);
                            visualsPool.Release(cell.OccupyingBlock);
                            cell.OccupyingBlock = null;
                            cellsToClear.Remove(cell);
                        });
                        break;

                    case 2:
                        blocksToCheck.Add(block);

                        LeanTween.color(cell.OccupyingBlock, Color.white, 0.5f);
                        LeanTween.rotateZ(cell.OccupyingBlock, 720f, 0.6f).setEaseOutBack().setOnComplete(() =>
                        {
                            LeanTween.scale(cell.OccupyingBlock, Vector3.zero, 0.4f).setEaseInBack().setOnComplete(() =>
                            {
                                block.CellVisuals.Remove(cell.OccupyingBlock);
                                visualsPool.Release(cell.OccupyingBlock);
                                cell.OccupyingBlock = null;
                                cellsToClear.Remove(cell);
                            });
                        });
                        break;

                    case 3:
                        blocksToCheck.Add(block);

                        LeanTween.color(cell.OccupyingBlock, Color.cyan, 0.5f);
                        LeanTween.rotateY(cell.OccupyingBlock, 180f, 0.6f).setEaseOutBack().setOnComplete(() =>
                        {
                            LeanTween.scale(cell.OccupyingBlock, Vector3.zero, 0.6f).setEaseInBack().setOnComplete(() =>
                            {
                                block.CellVisuals.Remove(cell.OccupyingBlock);
                                visualsPool.Release(cell.OccupyingBlock);
                                cell.OccupyingBlock = null;
                                cellsToClear.Remove(cell);
                            });
                        });
                        
                        break;

                    case 4:
                        blocksToCheck.Add(block);

                        LeanTween.color(cell.OccupyingBlock, Color.yellow, 0.5f);
                        LeanTween.scale(cell.OccupyingBlock, Vector3.zero, 0.8f).setEaseInBack().setOnComplete(() =>
                        {
                            block.CellVisuals.Remove(cell.OccupyingBlock);
                            visualsPool.Release(cell.OccupyingBlock);
                            cell.OccupyingBlock = null;
                            cellsToClear.Remove(cell);
                        });
                        break;

                    default:
                        blocksToCheck.Add(block);

                        LeanTween.color(cell.OccupyingBlock, Color.red, 0.5f);
                        LeanTween.scale(cell.OccupyingBlock, Vector3.zero, 1f).setEaseInBack().setOnComplete(() =>
                        {
                            block.CellVisuals.Remove(cell.OccupyingBlock);
                            visualsPool.Release(cell.OccupyingBlock);
                            cell.OccupyingBlock = null;
                            cellsToClear.Remove(cell);
                        });
                        break;
                }
            }

            foreach(Block block in blocksToCheck)
            {
                block.TryRelease(ref blocksPool);
            }
            blocksToCheck.Clear();

            CallEventWhenLinesFinishClearing(linesCleared);
        }

        private async void CallEventWhenLinesFinishClearing(int linesCleared)
        {
            await UniTask.WaitUntil(() => cellsToClear.Count == 0);

            cellsToClear.Clear();
            OnLinesCleared?.Invoke(linesCleared);
            OnLinesCheckFinished?.Invoke();
        }
    }
}
