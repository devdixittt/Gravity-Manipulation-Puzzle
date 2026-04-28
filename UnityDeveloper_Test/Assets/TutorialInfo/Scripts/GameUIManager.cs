using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Text timerText;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject gameOverPanel;


    private void Start()
    {
        gameOverPanel.SetActive(false);

        // Wire up GameManager events
        GameManager.Instance.OnGameOver += ShowGameOver;
        GameManager.Instance.OnTimerUpdate += UpdateTimer;
    }

    /// <summary>Formats remaining seconds as MM:SS and turns red under 30 s.</summary>
    private void UpdateTimer(float secondsRemaining)
    {
        int minutes = Mathf.FloorToInt(secondsRemaining / 60f);
        int seconds = Mathf.FloorToInt(secondsRemaining % 60f);
        timerText.text = $"Time : {minutes:00}:{seconds:00}";
  
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
