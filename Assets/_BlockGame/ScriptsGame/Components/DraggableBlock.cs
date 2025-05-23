using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArtTest.Game
{
    public class DraggableBlock : MonoBehaviour
    {
        private Vector3 startPosition;
        private Vector3 offset;
        private Camera cachedCamera;

        private bool isDragging = false;
        private bool isPlaced = false;

        private List<GameObject> ghostObjects = new();

        private Vector3 placeZOffset = new Vector3(0, 0, -2);

        public event Action<Block> OnDragEnd;

        private void Awake()
        {
            cachedCamera = Camera.main;
            startPosition = transform.position;
        }

        private void ClearEvents()
        {
            OnDragEnd = null;
        }

        public void ResetPosition()
        {
            transform.position = startPosition;
        }

        public void OnMouseDown()
        {
            if (isPlaced)
            {
                return;
            }

            BlockGameManager.Instance.PlayPickUpSfx();

            Vector3 cursorWorldPos = cachedCamera.ScreenToWorldPoint(Input.mousePosition);
            cursorWorldPos.z = 0;

            offset = transform.position - cursorWorldPos;
            isDragging = true;
        }

        public void OnMouseDrag()
        {
            if (!isDragging)
            {
                return;
            }

            foreach(var ghostObject in ghostObjects)
            {
                Destroy(ghostObject);
            }
            ghostObjects.Clear();

            Vector3 cursorWorldPos = cachedCamera.ScreenToWorldPoint(Input.mousePosition);
            cursorWorldPos.z = -1;

            List<Cell> hoveringCells = new();
            var visuals = GetComponent<Block>().CellVisuals;
            foreach (var visual in visuals)
            {
                Vector3 worldPosition = visual.transform.position;
                Collider2D hit = Physics2D.OverlapPoint(worldPosition, LayerMask.GetMask("Grid"));

                if (hit == null || !hit.TryGetComponent<Cell>(out var cell) || cell.OccupyingBlock != null)
                {
                    continue;
                }

                var ghostObject = Instantiate(visual, transform);
                ghostObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);

                ghostObjects.Add(ghostObject);
                hoveringCells.Add(cell);
            }

            if (ghostObjects.Count != 0)
            {
                for(int i = 0; i < ghostObjects.Count; i++)
                {
                    ghostObjects[i].transform.position = hoveringCells[i].transform.position;
                }
            }

            float distance = Vector3.Distance(startPosition, cursorWorldPos);
            float dragFactor = 1 + distance * 0.15f; // Dynamic Drag Offset
            transform.position = cursorWorldPos + offset * dragFactor;
        }

        public void OnMouseUp()
        {
            if (!isDragging)
            {
                return;
            }
            isDragging = false;

            foreach (var ghostObject in ghostObjects)
            {
                Destroy(ghostObject);
            }
            ghostObjects.Clear();

            var block = GetComponent<Block>();
            var visuals = block.CellVisuals;

            List<Cell> hitCells = new();
            foreach (var visual in visuals)
            {
                Vector3 worldPosition = visual.transform.position;
                Collider2D hit = Physics2D.OverlapPoint(worldPosition, LayerMask.GetMask("Grid"));

                if (hit == null || !hit.TryGetComponent<Cell>(out var cell) || cell.OccupyingBlock != null)
                {
                    ResetPosition();
                    return;
                }

                hitCells.Add(cell);
            }

            for (int i = 0; i < visuals.Count; i++)
            {
                visuals[i].transform.position = hitCells[i].transform.position + placeZOffset;
                hitCells[i].OccupyingBlock = visuals[i];
            }

            isPlaced = true;

            enabled = false;
            OnDragEnd?.Invoke(block);
        }
    }
}
