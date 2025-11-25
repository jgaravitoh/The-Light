using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextProjectilePool : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextProjectile projectilePrefab;
    [SerializeField] private RectTransform canvasRect;

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 20;

    [Header("Spawn")]
    [Tooltip("Proyectiles generados por segundo")]
    [SerializeField] private float spawnRate = 2f;

    [Tooltip("Si está en true, spawnea solo con el temporizador interno")]
    [SerializeField] private bool spawnAutomatically = true;

    [Tooltip("Cantidad de proyectiles por ráfaga")]
    [SerializeField] private int spawnPerBurst = 1;   // 👈 NUEVO

    [Tooltip("Estado general de si está permitido spawnear o no")]
    [SerializeField] private bool spawningEnabled = true; // 👈 NUEVO

    [Header("Velocidades (UI units/segundo)")]
    [SerializeField] private float minHorizontalSpeed = 500f;
    [SerializeField] private float maxHorizontalSpeed = 900f;
    [SerializeField] private float minVerticalSpeed = 300f;
    [SerializeField] private float maxVerticalSpeed = 700f;

    [Header("Gravedad y vida")]
    [SerializeField] private float gravity = -1500f;
    [SerializeField] private float projectileLifeTime = 5f;

    private readonly Queue<TextProjectile> _pool = new Queue<TextProjectile>();
    private float _spawnTimer;

    private void Awake()
    {
        if (canvasRect == null)
        {
            Canvas c = GetComponentInParent<Canvas>();
            if (c != null)
                canvasRect = c.GetComponent<RectTransform>();
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateProjectileInPool();
        }
    }

    private void Update()
    {
        // 👇 Ahora también revisa "spawningEnabled"
        if (!spawnAutomatically || !spawningEnabled || spawnRate <= 0f) return;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            // 👇 Spawnea varios por ráfaga
            for (int i = 0; i < spawnPerBurst; i++)
            {
                SpawnProjectile();
            }

            _spawnTimer = 1f / spawnRate;
        }
    }

    private void CreateProjectileInPool()
    {
        TextProjectile proj = Instantiate(projectilePrefab, canvasRect);
        proj.gameObject.SetActive(false);
        _pool.Enqueue(proj);
    }

    private TextProjectile GetFromPool()
    {
        if (_pool.Count == 0)
        {
            CreateProjectileInPool();
        }

        return _pool.Dequeue();
    }

    private void ReturnToPool(TextProjectile proj)
    {
        proj.gameObject.SetActive(false);
        _pool.Enqueue(proj);
    }

    private void SpawnProjectile()
    {
        if (canvasRect == null || projectilePrefab == null) return;

        TextProjectile proj = GetFromPool();

        Rect rect = canvasRect.rect;

        bool fromLeft = Random.value < 0.5f;

        float x = fromLeft ? rect.xMin - 100f : rect.xMax + 100f;
        float y = Random.Range(rect.yMin, rect.yMax);
        Vector2 startPos = new Vector2(x, y);

        float speedX = Random.Range(minHorizontalSpeed, maxHorizontalSpeed);
        float vx = fromLeft ? speedX : -speedX;

        float t = (y - rect.yMin) / rect.height;
        bool isHigh = t > 0.5f;

        float speedY = Random.Range(minVerticalSpeed, maxVerticalSpeed);
        float vy = isHigh ? -speedY : speedY;

        Vector2 initialVelocity = new Vector2(vx, vy);
        Vector2 gravityVec = new Vector2(0f, gravity);

        proj.Init(
            startPos,
            initialVelocity,
            gravityVec,
            projectileLifeTime,
            canvasRect,
            ReturnToPool
        );
    }

    // ----------- API PÚBLICA PARA OTROS SCRIPTS (como ArticleSystemManager) -----------

    /// <summary>
    /// Activa o desactiva completamente el spawn.
    /// </summary>
    public void SetSpawnEnabled(bool enabled)
    {
        spawningEnabled = enabled;
    }

    /// <summary>
    /// Cambia la tasa de spawn (proyectiles por segundo).
    /// </summary>
    public void SetSpawnRate(float newRate)
    {
        spawnRate = Mathf.Max(0f, newRate);
    }

    /// <summary>
    /// Fija cuántos proyectiles salen por ráfaga.
    /// </summary>
    public void SetBurstCount(int newCount)
    {
        spawnPerBurst = Mathf.Max(1, newCount);
    }

    /// <summary>
    /// Incrementa la cantidad de proyectiles por ráfaga.
    /// </summary>
    public void IncrementBurstCount(int delta)
    {
        SetBurstCount(spawnPerBurst + delta);
    }

    /// <summary>
    /// Activa o desactiva el modo automático (Update).
    /// </summary>
    public void SetSpawnAutomatically(bool auto)
    {
        spawnAutomatically = auto;
    }

    public bool IsSpawningEnabled => spawningEnabled;

    /// <summary>
    /// Spawn manual de UNA ráfaga.
    /// </summary>
    public void SpawnOnce()
    {
        for (int i = 0; i < spawnPerBurst; i++)
        {
            SpawnProjectile();
        }
    }
}
