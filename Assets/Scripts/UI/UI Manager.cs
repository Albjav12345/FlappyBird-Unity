using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Screens")]
    [SerializeField] private GameObject nicknameCanvas;
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject getReadyCanvas;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject leaderboardCanvas;

    public GameObject GameOverCanvas => gameOverCanvas;

    public void InitializeUI()
    {
        getReadyCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
        gameCanvas.SetActive(true);
        nicknameCanvas.SetActive(false);
        leaderboardCanvas.SetActive(false);
    }

    public void ShowNicknameScreen()
    {
        nicknameCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
        gameCanvas.SetActive(false);
    }

    public void ShowMainMenu()
    {
        nicknameCanvas.SetActive(false);
        leaderboardCanvas.SetActive(false);
        getReadyCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
        gameCanvas.SetActive(true);
    }

    public void ShowGetReady()
    {
        mainMenuCanvas.SetActive(false);
        getReadyCanvas.SetActive(true);
    }

    public void HideGetReady() => getReadyCanvas.SetActive(false);
    public void HideGameOver() => gameOverCanvas.SetActive(false);

    public void ShowSettings()
    {
        mainMenuCanvas.SetActive(false);
        gameCanvas.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        mainMenuCanvas.SetActive(false);
        gameCanvas.SetActive(false);
        leaderboardCanvas.SetActive(true);
    }

    public void ShowGameOver() => gameOverCanvas.SetActive(true);
}