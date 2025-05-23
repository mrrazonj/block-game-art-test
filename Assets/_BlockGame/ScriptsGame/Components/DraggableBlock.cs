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

        private Vector3 placeZOffset = new Vector3(0, 0, -1);

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

            Vector3 cursorWorldPos = cachedCamera.ScreenToWorldPoint(Input.mousePosition);
            cursorWorldPos.z = -1;

            float distance = Vector3.Distance(startPosition, cursorWorldPos);
            transform.position = cursorWorldPos + offset;
        }

        public void OnMouseUp()
        {
            if (!isDragging)
            {
                return;
            }
            isDragging = false;

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
            BlockGameManager.Instance.PlayPlaceSfx();

            enabled = false;
            OnDragEnd?.Invoke(block);
        }
    }
}
