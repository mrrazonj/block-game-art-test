using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ArtTest.Game
{
    public class Block : MonoBehaviour
    {
        public List<Vector2Int> CellsOccupied;
        public Vector2Int Position;

        private Color blockColor;

        public List<GameObject> CellVisuals = new();

        public void Initialize(Sprite sprite, List<Vector2Int> shape, Color color, ref ObjectPool<GameObject> visualsPool)
        {
            CellsOccupied = shape;
            blockColor = color;

            CellVisuals.Clear();

            if (transform.childCount > 0)
            {
                foreach (Transform child in transform)
                {
                    visualsPool.Release(child.gameObject);
                }
            }

            foreach (var cell in CellsOccupied)
            {
                var cellVisual = visualsPool.Get().GetComponent<SpriteRenderer>();
                cellVisual.transform.SetParent(this.transform);
                cellVisual.sprite = sprite;
                cellVisual.color = blockColor;
                cellVisual.transform.localPosition = new Vector3(cell.x, cell.y, 0);
                cellVisual.transform.localScale = Vector3.one;
                CellVisuals.Add(cellVisual.gameObject);
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

            foreach(var visual in CellVisuals)
            {
                var localPos = visual.transform.localPosition;
                localPos -= (Vector3)center;
                visual.transform.localPosition = localPos;
            }
        }

        public void TryRelease(ref ObjectPool<Block> blockPool)
        {
            if (transform.childCount == 0)
            {
                blockPool.Release(this);
            }
        }
    }
}
