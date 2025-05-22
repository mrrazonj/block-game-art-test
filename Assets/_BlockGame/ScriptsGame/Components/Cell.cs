using UnityEngine;

namespace ArtTest.Game
{
    public class Cell : MonoBehaviour
    {
        public int x, y;
        public GameObject OccupyingBlock = null;

        private void Awake()
        {
            var collider = GetComponent<BoxCollider2D>();

            if (collider == null)
                gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
        }
    }
}
