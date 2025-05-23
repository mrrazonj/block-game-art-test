using UnityEngine;

namespace ArtTest.Models
{
    [CreateAssetMenu(fileName = "GameTheme", menuName = "_Art Test/Create Game Theme")]
    public class GameTheme : ScriptableObject
    {
        [Header("Block Theme")]
        public Sprite BlockSprite;

        [Header("Play Area Theme")]
        public Sprite GridSquareSprite;
        public Sprite GridPanelSprite;
        public Sprite BackgroundSprite;

        [Header("Sound Theme")]
        public AudioClip BlockPlaceSound;
        public AudioClip BlockPickUpSound;
    }
}
