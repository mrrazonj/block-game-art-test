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

        public override bool Equals(object other)
        {
            if (other is Cell cell)
            {
                return x == cell.x && y == cell.y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Hash128.Compute($"{x},{y}").GetHashCode();
        }
    }
}
