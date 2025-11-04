using UnityEngine;
using System.Collections.Generic;

public class BirdEnemiesController : BaseSpawner
{
    public static BirdEnemiesController Instance { get; private set; }

    [Header("Bird Enemy Settings")]
    public float speed;
    public float delayTime;
    public bool canSpawnBirdEnemies = false; // Desactivado al inicio
    
    public bool IsEnabled => canSpawnBirdEnemies;

    // Lista para gestionar las instancias
    private readonly List<BirdEnemy> birdEnemies = new();

    private float spawnX;
    private float randomY;

    protected override float DelayTime => delayTime;
    protected override bool CustomCanSpawn => canSpawnBirdEnemies;

    private void Awake()
    {
        Instance = this;
    }

    protected override void Start()
    {
        base.Start();
        DetectScreenSize();
    }

    private void DetectScreenSize()
    {
        spawnX = Camera.main.aspect * (2 * Camera.main.orthographicSize);
    }

    protected override Vector3 GetSpawnPosition()
    {
        randomY = Random.Range(-1.5f, 1.5f);
        return new Vector3(spawnX, randomY, 0f);
    }

    // ✅ Llamado desde el BaseSpawner cuando se crea un enemigo
    protected void OnObjectSpawned(GameObject spawnedObject)
    {
        var bird = spawnedObject.GetComponent<BirdEnemy>();
        if (bird != null && !birdEnemies.Contains(bird))
        {
            birdEnemies.Add(bird);
        }
    }

    // ✅ Llamar desde el GameManager al reiniciar la partida
    public void ResetAllBirds()
    {
        Debug.Log("RESETING BIRDS");
        BirdEnemy[] allBirds = FindObjectsOfType<BirdEnemy>(true);

        foreach (var bird in allBirds)
        {
            if (bird != null)
            {
                bird.ResetBirdVisuals();
                bird.gameObject.SetActive(false);
            }
        }
    }

    // ✅ Permite activar/desactivar el spawner desde fuera
    public void SetSpawnState(bool state)
    {
        canSpawnBirdEnemies = state;
    }
}