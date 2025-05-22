using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtTest.Game
{
    public class Block : MonoBehaviour
    {
        public List<Vector2Int> CellsOccupied;
        public Vector2Int Position;

        private Color blockColor;

        [SerializeField]
        private GameObject cellVisualPrefab;
        private List<GameObject> cellVisuals = new();

        public void Initialize(List<Vector2Int> shape, Color color)
        {
            CellsOccupied = shape;
            blockColor = color;

            foreach(var visual in cellVisuals)
            {
                Destroy(visual);
            }

            cellVisuals.Clear();

            foreach (var cell in CellsOccupied)
            {
                var cellVisual = Instantiate(cellVisualPrefab, transform).GetComponent<SpriteRenderer>();
                cellVisual.color = blockColor;
                cellVisual.transform.localPosition = new Vector3(cell.x, cell.y, 0);
                cellVisuals.Add(cellVisual.gameObject);
            }

            CenterVisuals();
        }

        private void CenterVisuals()
        {
            if (CellsOccupied.Count == 0)
            {
                return;
            }

            Vector2 center = Vector2.zero;
            foreach (var cell in CellsOccupied)
            {
                center += (Vector2)cell;
            }
            center /= CellsOccupied.Count;

            foreach(var visual in cellVisuals)
            {
                var localPos = visual.transform.localPosition;
                localPos -= (Vector3)center;
                visual.transform.localPosition = localPos;
            }
        }
    }
}
