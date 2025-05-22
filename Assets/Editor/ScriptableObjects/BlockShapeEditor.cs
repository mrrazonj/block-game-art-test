using ArtTest.Models;
using UnityEditor;
using UnityEngine;

namespace ArtTest.Editor
{
    [CustomEditor(typeof(Models.BlockData))]
    public class BlockShapeEditor : UnityEditor.Editor
    {
        private const int gridSize = 4;
        private bool[,] grid = new bool[gridSize, gridSize];
        private Color blockColor = Color.white;

        private void OnEnable()
        {
            BlockData data = (BlockData)target;
            foreach (Vector2Int cell in data.Cells)
            {
                if (cell.x >= 0 && cell.x < gridSize && cell.y >= 0 && cell.y < gridSize)
                {
                    grid[cell.x, cell.y] = true;
                }
            }

            blockColor = data.BlockColor;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Define Block Shape", EditorStyles.boldLabel);

            for (int y = 0; y < gridSize; y++)
            {
                GUILayout.BeginHorizontal();
                for (int x = 0; x < gridSize; x++)
                {
                    grid[x, y] = GUILayout.Toggle(grid[x, y], "", GUILayout.Width(20), GUILayout.Height(20));
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Block Color", EditorStyles.boldLabel);

            blockColor = EditorGUILayout.ColorField(blockColor);

            BlockData data = (BlockData)target;
            if (GUILayout.Button("Clear"))
            {
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        grid[x, y] = false;
                    }
                }
                blockColor = Color.white;
                data.Cells.Clear();
            }
            if (GUILayout.Button("Save"))
            {
                data.Cells.Clear();
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        if (grid[x, y])
                        {
                            data.Cells.Add(new Vector2Int(x, y));
                        }
                    }
                }
                data.BlockColor = blockColor;

                EditorUtility.SetDirty(data);
            }
        }
    }
}
