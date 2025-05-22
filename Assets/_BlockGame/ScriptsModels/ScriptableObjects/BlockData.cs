using System.Collections.Generic;
using UnityEngine;

namespace ArtTest.Models
{
    [CreateAssetMenu(fileName = "BlockShape", menuName = "_Art Test/Create Block Shape")]
    public class BlockData : ScriptableObject
    {
        public List<Vector2Int> Cells = new List<Vector2Int>();
        public Color BlockColor = Color.white;
    }
}
