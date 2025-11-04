using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Difficulty")]
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float maxDifficultyPoints = 50f;

    private float difficultyPercent = 0f;


    // =====================================================================
    // PIPE SETTINGS
    // =====================================================================
    [Header("Pipe Settings")]
    public float basePipeSpeed = 2f;
    public float maxPipeSpeed = 3.5f;

    public float targetPipeSpacing = 4.0f;
    public float pipeSpacingRandomness = 0.5f;


    // =====================================================================
    // BIRD SETTINGS
    // =====================================================================
    [Header("Bird Settings")]
    public float baseBirdSpeed = 1.8f;
    public float maxBirdSpeed = 3.0f;

    public float targetBirdSpacing = 5.0f;
    public float birdSpacingRandomness = 0.5f;

    public float unlockBirdsPercent = 0.35f;


    private void Awake()
    {
        Instance = this;

        // ✅ Registrar eventos SOLO una vez
        PipesController.Instance.OnSpawn += OnPipeSpawned;
        BirdEnemiesController.Instance.OnSpawn += OnBirdSpawned;
    }


    // =====================================================================
    // ✅ DIFICULTAD CONTINUA
    // =====================================================================
    public void UpdateDifficulty(int points)
    {
        difficultyPercent = Mathf.Clamp01(points / maxDifficultyPoints);
        float D = difficultyCurve.Evaluate(difficultyPercent);

        // ✅ Velocidad continua y suave para pipes y birds
        PipesController.Instance.speed = Mathf.Lerp(basePipeSpeed, maxPipeSpeed, D);
        BirdEnemiesController.Instance.speed = Mathf.Lerp(baseBirdSpeed, maxBirdSpeed, D);

        // ✅ Parallax sincronizado con la velocidad de las tuberías
        ParallaxController.Instance.SetParallaxSpeed(PipesController.Instance.speed);

        // ✅ Activación de pájaros
        bool birdsOn = difficultyPercent >= unlockBirdsPercent;
        BirdEnemiesController.Instance.SetSpawnState(birdsOn);
    }



    // =====================================================================
    // ✅ EVENTO PIPE SPAWN → ajustar delay aquí (spacing perfecto)
    // =====================================================================
    private void OnPipeSpawned()
    {
        float spacing = targetPipeSpacing +
                        Random.Range(-pipeSpacingRandomness, pipeSpacingRandomness);

        float speed = PipesController.Instance.speed;

        PipesController.Instance.delayTime = spacing / speed;
        PipesController.Instance.ResetSpawnTimer();
    }


    // =====================================================================
    // ✅ EVENTO BIRD SPAWN
    // =====================================================================
    private void OnBirdSpawned()
    {
        if (!BirdEnemiesController.Instance.IsEnabled)
            return;

        float spacing = targetBirdSpacing +
                        Random.Range(-birdSpacingRandomness, birdSpacingRandomness);

        float speed = BirdEnemiesController.Instance.speed;

        BirdEnemiesController.Instance.delayTime = spacing / speed;
        BirdEnemiesController.Instance.ResetSpawnTimer();
    }


    // =====================================================================
    // ✅ RESET PERFECTO
    // =====================================================================
    public void ResetDifficulty()
    {
        difficultyPercent = 0f;

        PipesController.Instance.speed = basePipeSpeed;
        BirdEnemiesController.Instance.speed = baseBirdSpeed;

        PipesController.Instance.delayTime = targetPipeSpacing / basePipeSpeed;
        BirdEnemiesController.Instance.delayTime = targetBirdSpacing / baseBirdSpeed;

        PipesController.Instance.ResetSpawnTimer();
        BirdEnemiesController.Instance.ResetSpawnTimer();

        BirdEnemiesController.Instance.SetSpawnState(false);
    }
}
