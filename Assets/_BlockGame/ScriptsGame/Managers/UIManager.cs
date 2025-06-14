using TMPro;
using UnityEngine;

namespace ArtTest.Game
{
    public class UIManager : MonoBehaviour
    {
        [Header("Game Over UI")]
        [SerializeField]
        private GameObject gameOverBlackoutPanel;
        [SerializeField]
        private GameObject gameOverPanel;
        [SerializeField]
        private GameObject scoreLabel;
        [SerializeField]
        private GameObject nameInput;

        public event System.Action<string> OnNameInputSubmitted;

        private void Awake()
        {
            gameOverBlackoutPanel.SetActive(false);
            gameOverPanel.transform.localScale = Vector3.zero;
        }

        public void Initialize()
        {
            Awake();
        }

        public void ShowGameOverUI(int currentScore, int highScore)
        {
            gameOverBlackoutPanel.SetActive(true);
            scoreLabel.GetComponent<TextMeshProUGUI>().text = $"Score: {currentScore}\nHigh Score: {highScore}";
            LeanTween.scale(gameOverPanel, Vector3.one, 1f).setEaseOutBack();
        }

        public string GetPlayerName()
        {
            return nameInput.GetComponent<TMP_InputField>().text;
        }
    }
}
