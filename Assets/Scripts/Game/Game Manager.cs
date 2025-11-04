using CarterGames.Assets.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RDG;

public class GameManager : CustomBehaviourBase
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int maxFpsRate = 120;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private ParallaxController parallaxController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private LeaderboardUI leaderboardUI; // ✅ nuevo campo limpio

    public UIManager UI => uiManager;

    private Vector3 initialPlayerPos;
    private Quaternion initialPlayerRot;
    public bool HasPlayButtonBeenClicked { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    protected override void Start()
    {
        ConfigFPS();
        PrepareMainScene();

        GameDataController.Instance.LoadGameData();

        if (!string.IsNullOrEmpty(GameDataController.Instance.gameData.nickname))
        {
            Debug.Log("Jugador detectado: " + GameDataController.Instance.gameData.nickname);
            uiManager.ShowMainMenu();
            player.EnableGame();
        }
        else
        {
            Debug.LogWarning("No hay nickname, mostrando pantalla de entrada...");
            uiManager.ShowNicknameScreen();
            player.DisableRender();
        }

        initialPlayerPos = playerTransform.position;
        initialPlayerRot = playerTransform.rotation;
    }

    private void Update()
    {
        HandleStartGameInput();
    }

    private void ConfigFPS()
    {
        Application.targetFrameRate = maxFpsRate;
    }

    private void PrepareMainScene()
    {
        DifficultyManager.Instance.UpdateDifficulty(player.points);
        
        getFirebaseScores();

        MusicManager.Play("soundTrack", 0);
        uiManager.InitializeUI();
        parallaxController.parallax = true;

        PipesController.Instance.SetSpawnState(false);
        PipesController.Instance.ResetSpawnTimer();

        BirdEnemiesController.Instance.SetSpawnState(false);
        BirdEnemiesController.Instance.ResetSpawnTimer();
        BirdEnemiesController.Instance.ResetAllBirds();

        playerRb.bodyType = RigidbodyType2D.Kinematic;
        player.animator.enabled = true;
        player.EnableGame();
    }

    public void OnNicknameSaved()
    {
        player.EnableRender();
        player.EnableGame();
        uiManager.ShowMainMenu();
    }

    public void OnPlayButtonClick()
    {
        AudioManager.Play("pop");
        Vibration.VibratePredefined(Vibration.PredefinedEffect.EFFECT_HEAVY_CLICK);
        uiManager.ShowGetReady();
        parallaxController.parallax = true;
        HasPlayButtonBeenClicked = true;
    }

    private void HandleStartGameInput()
    {
        if (Input.GetMouseButtonDown(0) && HasPlayButtonBeenClicked)
        {
            uiManager.HideGetReady();

            DifficultyManager.Instance.UpdateDifficulty(player.points);

            PipesController.Instance.SetSpawnState(true);
            PipesController.Instance.ResetSpawnTimer();

            BirdEnemiesController.Instance.SetSpawnState(false);
            BirdEnemiesController.Instance.ResetSpawnTimer();
            BirdEnemiesController.Instance.ResetAllBirds();

            player.points = 0;
            player.RefreshPointsUI();

            player.EnableGame();
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            player.PrepareForGameplay();

            player.Jump();
            HasPlayButtonBeenClicked = false;
        }
    }

    public void RestartGame()
    {
        ResetGameCommon();
        uiManager.ShowGetReady();
        HasPlayButtonBeenClicked = true;
    }

    public void RestartMainScene()
    {
        ResetGameCommon();
        uiManager.ShowMainMenu();
        HasPlayButtonBeenClicked = false;
    }

    private void ResetGameCommon()
    {
        DifficultyManager.Instance.ResetDifficulty();
        DifficultyManager.Instance.UpdateDifficulty(player.points);
        
        BirdEnemiesController.Instance.SetSpawnState(false);
        BirdEnemiesController.Instance.ResetSpawnTimer();
        BirdEnemiesController.Instance.ResetAllBirds();

        parallaxController.parallax = true;
        player.animator.enabled = true;
        AudioManager.Play("pop");
        Vibration.VibratePredefined(Vibration.PredefinedEffect.EFFECT_HEAVY_CLICK);

        player.points = 0;
        player.RefreshPointsUI();
        DifficultyManager.Instance.UpdateDifficulty(player.points);

        uiManager.HideGameOver();
        playerTransform.position = initialPlayerPos;
        playerTransform.rotation = initialPlayerRot;
        playerRb.bodyType = RigidbodyType2D.Static;

        foreach (GameObject pipe in GameObject.FindGameObjectsWithTag("Pipes"))
            pipe.SetActive(false);
    }

    public void OnExitButtonClick() => StartCoroutine(CloseGame());

    private IEnumerator CloseGame()
    {
        AudioManager.Play("pop");
        Vibration.VibratePredefined(Vibration.PredefinedEffect.EFFECT_HEAVY_CLICK);
        yield return new WaitForSeconds(0.1f);
        Application.Quit();
    }

    public void OnSettingsButtonClick()
    {
        AudioManager.Play("pop");
        uiManager.ShowSettings();
        player.gameObject.SetActive(false);
    }
    
    // ==========================
    // 🔹 LEADERBOARD BUTTON 🔹
    // ==========================
    public void OnLeaderboardButtonClick()
    {
        AudioManager.Play("pop");
        Vibration.VibratePredefined(Vibration.PredefinedEffect.EFFECT_HEAVY_CLICK);
        uiManager.ShowLeaderboard();

        // Pedir datos a Firebase
        getFirebaseScores();
    }

    private void getFirebaseScores()
    {
        LeaderboardManager.Instance.GetTopScores(50, leaderboard =>
        {
            if (leaderboard == null || leaderboard.Count == 0)
            {
                Debug.LogWarning("⚠️ No hay puntuaciones en la base de datos todavía.");
                leaderboardUI.ClearEntries();
                return;
            }

            // Filtrar mejores puntuaciones por jugador
            Dictionary<string, ScoreData> bestByPlayer = new Dictionary<string, ScoreData>();
            foreach (var entry in leaderboard)
            {
                if (!bestByPlayer.ContainsKey(entry.name) || entry.score > bestByPlayer[entry.name].score)
                    bestByPlayer[entry.name] = entry;
            }

            // Ordenar descendente
            List<ScoreData> uniqueLeaderboard = new List<ScoreData>(bestByPlayer.Values);
            uniqueLeaderboard.Sort((a, b) => b.score.CompareTo(a.score));

            // Limitar top 5
            int topCount = Mathf.Min(5, uniqueLeaderboard.Count);
            var topPlayers = uniqueLeaderboard.GetRange(0, topCount);

            // Mostrar en UI
            leaderboardUI.Populate(topPlayers);

            // Log
            Debug.Log($"📊 Se leyeron {topPlayers.Count} registros de Firebase.");
            for (int i = 0; i < topPlayers.Count; i++)
            {
                var entry = topPlayers[i];
                Debug.Log($"{i + 1}. {entry.name} — {entry.score} puntos ({entry.time})");
            }
        });
    }
}
