using UnityEngine;

namespace ArtTest.Models
{
    [CreateAssetMenu(fileName = "NewGameSettings", menuName = "_Art Test/Create Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Grid Settings")]
        public int GridWidth = 9;
        public int GridHeight = 9;

        [Header("Score Settings")]
        public int[] ScorePerLineCleared = { 10, 20, 50, 100, 200 };

        [Header("Block Settings")]
        public BlockData[] BlockData;
    }
}
