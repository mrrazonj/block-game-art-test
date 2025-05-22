using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArtTest.Game
{
    public class Draggable : MonoBehaviour
    {
        private Vector3 startPosition;
        private Vector3 offset;
        private Camera cachedCamera;

        private bool isDragging = false;

        public event Action<Block> OnDragEnd;

        private void Awake()
        {
            cachedCamera = Camera.main;
        }

        public void ResetPosition()
        {
            transform.position = startPosition;
        }

        public void OnMouseDown()
        {
            print($"OnMouseDown: {gameObject.name}");

            startPosition = transform.position;

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
            cursorWorldPos.z = 0;

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

            OnDragEnd?.Invoke(GetComponent<Block>());
        }
    }
}
