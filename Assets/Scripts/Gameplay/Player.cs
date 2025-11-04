using System.Collections;
using UnityEngine;
using TMPro;
using CarterGames.Assets.AudioManager;
using RDG;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] float impulse;
    [SerializeField] float maxRotationUp;
    [SerializeField] float maxRotationDown;
    [SerializeField] float minRotationSpeed;
    [SerializeField] float maxRotationSpeed;
    [SerializeField] int deathVibration;

    [Header("TMP")]
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;

    [Header("Coins")]
    [SerializeField] private GameObject bronzeCoin;
    [SerializeField] private GameObject silverCoin;
    [SerializeField] private GameObject goldenCoin;

    [SerializeField] private ParallaxController parallaxController;

    public bool canPlayJumpSound { get; set; } = false;
    public bool canDie { get; set; } = true;
    public int points { get; set; } = 0;
    public Animator animator { get; private set; }

    private Rigidbody2D rb;
    private bool gameStarted = false;

    // ✅ Estado real del jugador (sustituye a hasCollisioned)
    public bool IsDead { get; private set; } = false;

    public static Player Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        rb.simulated = false;
        animator.enabled = false;

        GameDataController.Instance.LoadGameData();
    }

    private void Update()
    {
        if (!gameStarted || IsDead)
            return;

        pointsText.text = points.ToString();
        Jump();
        RotatePlayer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameStarted || IsDead)
            return;

        if (collision.CompareTag("Pipes") || collision.CompareTag("Ground"))
        {
            Die();
        }
        else if (collision.CompareTag("BirdEnemy"))
        {
            BirdEnemy bird = collision.GetComponent<BirdEnemy>();
            if (bird != null)
                bird.OnHitByPlayer();
            
            Die();
        }
        else if (collision.CompareTag("PlayerPointsCheck"))
        {
            AddPoints();
        }
    }

    // ✅ Ahora el jugador solo depende de sí mismo, no del PipesController
    public void Jump()
    {
        if (Input.GetMouseButtonDown(0) && !IsDead)
        {
            if (canPlayJumpSound)
                AudioManager.Play("jump");

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(0f, impulse), ForceMode2D.Impulse);
        }
    }

    private void RotatePlayer()
    {
        float currentVelocity = rb.linearVelocity.y;
        float targetRotation = Mathf.Clamp(currentVelocity * maxRotationSpeed, maxRotationDown, maxRotationUp);
        float rotationAmount = Mathf.Lerp(
            minRotationSpeed,
            maxRotationSpeed,
            Mathf.InverseLerp(0, maxRotationUp, Mathf.Abs(targetRotation))
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0f, 0f, targetRotation),
            Time.deltaTime * rotationAmount
        );
    }

    public void Die()
    {
        if (IsDead || !canDie)
            return;

        IsDead = true;
        canDie = false;

        // ✅ Detener spawneo
        PipesController.Instance.SetSpawnState(false);
        BirdEnemiesController.Instance.SetSpawnState(false);

        // ✅ Detener movimiento de fondo
        parallaxController.parallax = false;

        // ✅ Cargar datos
        GameDataController.Instance.LoadGameData();

        // ✅ Guardar mejor puntuación
        if (points > GameDataController.Instance.gameData.bestScore)
        {
            GameDataController.Instance.gameData.bestScore = points;
            GameDataController.Instance.SaveGameData();

            LeaderboardManager.Instance.SendScore(
                GameDataController.Instance.gameData.nickname,
                points,
                success => Debug.Log(success ? $"🎉 Nuevo récord ({points})" : "❌ Error al enviar récord")
            );
        }

        // ✅ Actualizar UI
        bestScoreText.text = GameDataController.Instance.gameData.bestScore.ToString();
        scoreText.text = points.ToString();
        GameManager.Instance.UI.ShowGameOver();

        ShowCoinReward();
        AudioManager.Play("hit");
        Vibration.VibrateSwoosh();
        animator.enabled = false;
    }

    private void ShowCoinReward()
    {
        bronzeCoin.SetActive(points <= 10);
        silverCoin.SetActive(points > 10 && points <= 20);
        goldenCoin.SetActive(points > 20);
    }

    private void AddPoints()
    {
        points++;
        AudioManager.Play("score");
        StartCoroutine(DoubleVibration());
        DifficultyManager.Instance.UpdateDifficulty(points);
    }

    private IEnumerator DoubleVibration()
    {
        yield return new WaitForSeconds(0.1f);
        Vibration.VibratePredefined(Vibration.PredefinedEffect.EFFECT_HEAVY_CLICK);
        yield return new WaitForSeconds(0.1f);
        Vibration.VibratePredefined(Vibration.PredefinedEffect.EFFECT_HEAVY_CLICK);
    }
    
    public void RefreshPointsUI()
    {
        pointsText.text = points.ToString();
    }
    
    // ✅ Llamado al empezar la partida
    public void EnableGame()
    {
        rb.simulated = true;
        animator.enabled = true;
        animator.Play(0, -1, 0f);

        parallaxController.parallax = true;
        gameStarted = true;
        IsDead = false;
    }

    // ✅ Cuando se reinicia la partida
    public void Revive()
    {
        IsDead = false;
        canDie = true;
        canPlayJumpSound = true;
    }

    public void DisableRender() => GetComponent<SpriteRenderer>().enabled = false;
    public void EnableRender() => GetComponent<SpriteRenderer>().enabled = true;

    public void PrepareForGameplay()
    {
        Revive();
    }
}
