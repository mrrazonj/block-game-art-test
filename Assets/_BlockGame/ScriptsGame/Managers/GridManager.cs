using System;
using ArtTest.Models;
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
        private Cell[,] gridCells;

        private bool isInitialized = false;

        public event Action<int> OnLinesCleared;

        public void Initialize(GameSettings gameSettings)
        {
            gridWidth = gameSettings.GridWidth;
            gridHeight = gameSettings.GridHeight;
            gridCells = new Cell[gridWidth, gridHeight];

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
                    gridCells[i, j] = cell;
                }
            }

            isInitialized = true;
        }

        public Vector2Int GetClosestGridPosition(Vector3 worldPosition)
        {
            if (!isInitialized)
            {
                Debug.LogError("GridManager is not initialized.");
                return Vector2Int.zero;
            }

            Vector3 localPosition = gameAreaObject.transform.InverseTransformPoint(worldPosition);

            float cellWidth = gameAreaObject.GetComponent<SpriteRenderer>().bounds.size.x / gridWidth;
            float cellHeight = gameAreaObject.GetComponent<SpriteRenderer>().bounds.size.y / gridHeight;

            int x = Mathf.FloorToInt((localPosition.x + (gridWidth / 2f) * cellWidth) / cellWidth);
            int y = Mathf.FloorToInt((localPosition.y + (gridHeight / 2f) * cellHeight) / cellHeight);

            return new Vector2Int(x, y);
        }

        public bool CheckValidPlacement(Block block, Vector2Int position)
        {
            if (!isInitialized)
            {
                Debug.LogError("GridManager is not initialized.");
                return false;
            }

            foreach (var cell in block.CellsOccupied)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;

                print($"CheckValidPlacement: {x}, {y}");

                if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                {
                    return false; // Out of bounds
                }
                if (gridCells[x, y] != null && gridCells[x, y].IsOccupied)
                {
                    return false; // Cell is already occupied
                }
            }

            return true; // Valid placement
        }

        public void PlaceBlock(Block block, Vector2Int position)
        {
            if (!isInitialized)
            {
                Debug.LogError("GridManager is not initialized.");
                return;
            }

            if (!CheckValidPlacement(block, position))
            {
                Debug.LogError("Invalid placement for the block.");
                return;
            }

            foreach (var cell in block.CellsOccupied)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;
                gridCells[x, y].IsOccupied = true; // Mark the cell as occupied
            }

            int cleared = ClearLines();
            OnLinesCleared?.Invoke(cleared);
        }

        public int ClearLines()
        {
            int linesCleared = 0;
            for (int y = 0; y < gridHeight; y++)
            {
                bool isFullLine = true;
                for (int x = 0; x < gridWidth; x++)
                {
                    if (gridCells[x, y] == null || !gridCells[x, y].IsOccupied)
                    {
                        isFullLine = false;
                        break;
                    }
                }
                if (isFullLine)
                {
                    linesCleared++;
                    // Clear the line and shift down
                    for (int x = 0; x < gridWidth; x++)
                    {
                        gridCells[x, y].IsOccupied = false;
                    }
                }
            }
            return linesCleared;
        }
    }
}
