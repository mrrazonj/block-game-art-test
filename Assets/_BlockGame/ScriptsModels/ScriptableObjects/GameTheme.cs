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
        public AudioClip Bgm;
        public AudioClip[] BlockPlaceSound;
        public AudioClip BlockPickUpSound;

        [Header("Effect Object On Clear")]
        public GameObject Clear1;
        public GameObject Clear2;
        public GameObject Clear3;
        public GameObject Clear4OrMore;
    }
}
