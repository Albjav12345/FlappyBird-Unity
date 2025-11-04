using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class BaseSpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 5;

    protected List<GameObject> pool = new List<GameObject>();

    protected float timer = 0f;

    private bool _canSpawnOverride = true;

    protected bool CanSpawn => _canSpawnOverride && CustomCanSpawn;

    protected abstract bool CustomCanSpawn { get; }
    protected abstract float DelayTime { get; }
    protected abstract Vector3 GetSpawnPosition();

    // ✅ Evento que avisa cuando se ha spawneado un objeto
    public Action OnSpawn;

    protected virtual void Start()
    {
        CreatePool();
    }

    void Update()
    {
        if (!CanSpawn)
            return;

        timer += Time.deltaTime;

        if (timer >= DelayTime)
        {
            Spawn();
            timer = 0f;
        }
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.parent = transform;
            pool.Add(obj);
        }
    }

    private void Spawn()
    {
        GameObject obj = GetObjectFromPool();
        if (obj == null) return;

        obj.transform.position = GetSpawnPosition();
        obj.SetActive(true);

        // ✅ Evento disparado SOLO cuando se spawnea
        OnSpawn?.Invoke();
    }

    private GameObject GetObjectFromPool()
    {
        foreach (var obj in pool)
            if (!obj.activeSelf)
                return obj;

        return null;
    }

    public void SetSpawnState(bool state)
    {
        _canSpawnOverride = state;
    }

    public void ResetSpawnTimer()
    {
        timer = 0f;
    }
}