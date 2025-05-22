using ArtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArtTest.Game
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject gameAreaObject;
        [SerializeField]
        private GameObject gridPrefab;

        private int gridWidth = 9;
        private int gridHeight = 9;
        public List<Cell> Cells;
        private Cell[,] gridCells;

        private bool isInitialized = false;

        public event Action<int> OnLinesCleared;
        public event Action OnLinesCheckFinished;

        public void Initialize(GameSettings gameSettings)
        {
            gridWidth = gameSettings.GridWidth;
            gridHeight = gameSettings.GridHeight;
            gridCells = new Cell[gridWidth, gridHeight];
            Cells = new();

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
                        0
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

        public void TryClearLines()
        {
            int linesCleared = 0;
            List<Cell> cellsToClear = new List<Cell>();

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
                }
            }

            for (int x = 0; x < gridWidth; x++)
            {
                bool isFullColumn = false;
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
                }
            }

            foreach(Cell cell in cellsToClear)
            {
                Destroy(cell.OccupyingBlock);
                cell.OccupyingBlock = null;
            }

            OnLinesCleared?.Invoke(linesCleared);
            OnLinesCheckFinished?.Invoke();
        }
    }
}
