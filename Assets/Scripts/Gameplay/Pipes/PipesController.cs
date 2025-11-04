using UnityEngine;

public class PipesController : BaseSpawner
{
    public static PipesController Instance { get; private set; }

    [Header("Pipes Settings")]
    public float speed;
    public float delayTime;

    private float spawnX;
    private float randomY;

    private void Awake()
    {
        Instance = this;
    }

    // Tiempo entre spawns (se usa desde BaseSpawner)
    protected override float DelayTime => delayTime;

    // ✅ Siempre puede spawnear internamente.
    // El GameManager controla la activación real con SetSpawnState().
    protected override bool CustomCanSpawn => true;

    protected override void Start()
    {
        base.Start();
        DetectScreenSize();
    }

    private void DetectScreenSize()
    {
        // Calcula el borde derecho para spawnear fuera de pantalla
        spawnX = Camera.main.aspect * (2 * Camera.main.orthographicSize);
    }

    protected override Vector3 GetSpawnPosition()
    {
        randomY = Random.Range(-1.15f, 1.25f);
        return new Vector3(spawnX, randomY, 0f);
    }
}