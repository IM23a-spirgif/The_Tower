using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    const string SelectedTowerKey = "SelectedTower";
    const string TeslaTowerId = "tesla";

    public static Tower Instance;
    public GameObject bulletPrefab;
    public int damage = 2;
    public float fireRate = 1f;
    public float attackRange = 6f;
    public float splashRadius = 1.15f;
    public float splashDamageMultiplier = 0.45f;
    public float shellDamageMultiplier = 1f;
    public float projectileSpeedMultiplier = 1f;
    public float explosionKnockback = 0.18f;
    public float shrapnelEdgeDamageMultiplier;
    public int fragmentCount;
    public float burningGroundDuration;
    public float concussiveSlowMultiplier = 1f;
    public float concussiveSlowDuration;
    public float craterDuration;
    public float craterSlowMultiplier = 1f;
    public float chainDetonationMultiplier;
    public int siegeShellStacks;
    public float directHitDamageMultiplier = 1f;
    public float doubleChargeChance;
    public int siegePlatformStacks;
    public float executionDamageMultiplier;
    public float finisherDamageMultiplier;
    public bool smartTargeting;
    public float delayedFuseSeconds;
    public float delayedFuseRadiusMultiplier = 1f;
    public float volatileSecondaryExplosionMultiplier;
    public bool glassCannonDesign;
    public bool apocalypseRound;
    public bool isTeslaTower;
    public float teslaDamageMultiplier = 1f;
    public int teslaChainCount = 1;
    public float teslaChainFalloff = 0.72f;
    public float teslaChainRange = 3.25f;
    public float teslaForkChance;
    public bool teslaArcBounce;
    public int teslaSuperconductorStacks;
    public float teslaSlowMultiplier = 1f;
    public float teslaSlowDuration;
    public int teslaParalysisStacks;
    public float teslaIonizedBonus;
    public float teslaResidualDamageMultiplier;
    public int teslaEmpSurgeStacks;
    public int teslaCapacitorBankStacks;
    public float teslaOverflowDamage;
    public bool teslaEnergyOverflow;
    public float teslaFeedbackLoopMultiplier;
    public float teslaCriticalExplosionChance;
    public int teslaFieldStacks;
    public float teslaMagneticPull;
    public float teslaVoltageCollapseMultiplier;
    public int teslaMeltdownStacks;
    public bool teslaInfiniteArc;
    public bool teslaStormbringerProtocol;
    private float nextFireTime;
    private GameObject currentTarget;
    private int shotsFired;
    private float cooldownUntil;
    private bool towerBaselineApplied;
    private SpriteRenderer rangeCircle;
    private Camera mainCamera;
    private float teslaFieldNextTick;
    private int teslaCapacitorStacks;
    public int extraBullets = 0;  // For "Shoot +1 bullet"
    public bool homingEnabled = false;
    public bool ricochetEnabled = false;
    public bool pierceEnabled = false;

    void Awake()
    {
        ApplySelectedTowerBaseline();
    }

    void Start()
    {
        ApplySelectedTowerBaseline();
        EnsureRangeCircle();
        FitCameraToRange();
    }

    void Update()
    {
        UpdateRangeCircle();
        FitCameraToRange();
        TickTeslaField();

        if (Time.time < cooldownUntil)
            return;

        if (Time.time >= nextFireTime)
        {
            float cooldownMultiplier = RetargetAndFire();
            nextFireTime = Time.time + fireRate * cooldownMultiplier;
        }
    }

    void EnsureRangeCircle()
    {
        if (rangeCircle != null)
            return;

        GameObject rangeObject = new GameObject("TowerRangeCircle", typeof(SpriteRenderer));
        rangeObject.transform.position = transform.position;
        rangeCircle = rangeObject.GetComponent<SpriteRenderer>();
        rangeCircle.sprite = CreateCircleSprite();
        rangeCircle.color = isTeslaTower
            ? new Color(0.22f, 0.78f, 1f, 0.2f)
            : new Color(1f, 0.68f, 0.22f, 0.16f);
        rangeCircle.sortingOrder = -10;
        UpdateRangeCircle();
    }

    void UpdateRangeCircle()
    {
        if (rangeCircle == null)
            return;

        rangeCircle.transform.position = transform.position;
        rangeCircle.transform.localScale = Vector3.one * (attackRange * 2.08f);
    }

    void FitCameraToRange()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
            return;

        float padding = 0.45f;
        float verticalSize = attackRange + padding;
        float horizontalSize = (attackRange + padding) / Mathf.Max(0.1f, mainCamera.aspect);
        mainCamera.orthographicSize = Mathf.Max(5.5f, verticalSize, horizontalSize);
        Vector3 cameraPosition = transform.position;
        cameraPosition.z = mainCamera.transform.position.z;
        mainCamera.transform.position = cameraPosition;
    }

    static Sprite CreateCircleSprite()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.47f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float fillAlpha = distance <= radius ? 0.75f : 0f;
                float edge = Mathf.Clamp01(1f - Mathf.Abs(distance - radius) / 2.2f);
                float alpha = Mathf.Max(fillAlpha * 0.18f, edge);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    static void CreateTeslaArc(Vector2 start, Vector2 end)
    {
        GameObject arc = new GameObject("TeslaArc", typeof(LineRenderer), typeof(ShockwaveEffect));
        LineRenderer line = arc.GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 6;
        line.startWidth = 0.055f;
        line.endWidth = 0.018f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.5f, 0.9f, 1f, 0.98f);
        line.endColor = new Color(0.95f, 1f, 1f, 0.2f);

        for (int i = 0; i < line.positionCount; i++)
        {
            float t = (float)i / (line.positionCount - 1);
            Vector2 point = Vector2.Lerp(start, end, t);
            if (i > 0 && i < line.positionCount - 1)
                point += Random.insideUnitCircle * 0.12f;
            line.SetPosition(i, point);
        }

        arc.GetComponent<ShockwaveEffect>().Configure(0.12f);
    }

    static void CreateTeslaPulseVisual(Vector2 center, float radius)
    {
        GameObject ring = new GameObject("TeslaPulse", typeof(LineRenderer), typeof(ShockwaveEffect));
        ring.transform.position = center;

        LineRenderer line = ring.GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 56;
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.42f, 0.88f, 1f, 0.78f);
        line.endColor = new Color(0.42f, 0.88f, 1f, 0.78f);

        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = (float)i / line.positionCount * Mathf.PI * 2f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }

        ring.GetComponent<ShockwaveEffect>().Configure(0.25f);
    }

    void ApplySelectedTowerBaseline()
    {
        if (towerBaselineApplied)
            return;

        string selectedTower = PlayerPrefs.GetString(SelectedTowerKey, "cannon");
        if (selectedTower == TeslaTowerId)
        {
            isTeslaTower = true;
            damage = Mathf.Max(1, damage);
            fireRate *= 0.62f;
            attackRange *= 1.2f;
            splashRadius *= 0.45f;
            splashDamageMultiplier *= 0.2f;
            projectileSpeedMultiplier *= 1.35f;
        }
        else
        {
            fireRate *= 1.1f;
        }

        towerBaselineApplied = true;
    }

    float RetargetAndFire()
    {
        currentTarget = GetNearestEnemy();
        if (currentTarget != null)
        {
            return isTeslaTower ? FireTeslaAtTarget(currentTarget) : FireAtTarget(currentTarget);
        }

        teslaCapacitorStacks = 0;
        return 1f;
    }

    float FireAtTarget(GameObject target)
    {
        shotsFired++;
        FireShell(target, BuildShellStats());

        if (Random.value < doubleChargeChance)
            FireShell(target, BuildShellStats());

        if (glassCannonDesign && shotsFired % 10 == 0)
            cooldownUntil = Time.time + 1.75f;

        return 1f;
    }

    float FireTeslaAtTarget(GameObject target)
    {
        shotsFired++;
        if (teslaCapacitorBankStacks > 0)
            teslaCapacitorStacks = Mathf.Min(teslaCapacitorStacks + 1, teslaCapacitorBankStacks * 6);

        List<EnemyHealth> hitEnemies = new List<EnemyHealth>();
        EnemyHealth targetHealth = target.GetComponent<EnemyHealth>();
        if (targetHealth == null)
            return 1f;

        int maxTargets = teslaInfiniteArc ? 64 : Mathf.Max(1, 1 + teslaChainCount);
        float damageMultiplier = teslaDamageMultiplier * (1f + teslaCapacitorStacks * 0.035f);
        if (teslaStormbringerProtocol)
            damageMultiplier *= 1f + Mathf.Min(0.75f, CountEnemiesInRange(transform.position, attackRange) * 0.025f);
        if (teslaOverflowDamage > 0f)
        {
            damageMultiplier += teslaOverflowDamage;
            teslaOverflowDamage = 0f;
        }

        Vector2 chainSource = transform.position;
        EnemyHealth current = targetHealth;
        int chainIndex = 0;
        int totalHit = 0;
        while (current != null && chainIndex < maxTargets)
        {
            float falloff = Mathf.Pow(teslaChainFalloff, chainIndex);
            int dealtDamage = DamageTeslaTarget(current, chainSource, damageMultiplier * falloff, hitEnemies.Count == 0);
            totalHit++;
            hitEnemies.Add(current);
            chainSource = current.transform.position;

            TryForkTesla(current, hitEnemies, damageMultiplier * falloff * 0.62f);

            current = FindNextTeslaTarget(chainSource, hitEnemies);
            if (current == null && teslaArcBounce && hitEnemies.Count > 1)
                current = hitEnemies[Random.Range(0, hitEnemies.Count)];

            chainIndex++;
            if (teslaInfiniteArc && damageMultiplier * Mathf.Pow(teslaChainFalloff, chainIndex) < 0.2f)
                break;
        }

        if (totalHit == 1 && teslaSuperconductorStacks > 0)
            DamageTeslaTarget(targetHealth, transform.position, teslaSuperconductorStacks * 1.2f, false);

        if (teslaEmpSurgeStacks > 0 && shotsFired % 20 == 0)
            TeslaPulse(transform.position, attackRange * 0.9f, 0.7f + teslaEmpSurgeStacks * 0.35f);

        if (teslaFieldStacks > 0 && Time.time >= teslaFieldNextTick)
        {
            teslaFieldNextTick = Time.time + 0.55f;
            TeslaPulse(transform.position, 1.9f + teslaFieldStacks * 0.7f, 0.22f * teslaFieldStacks);
        }

        if (teslaMeltdownStacks > 0 && Random.value < 0.06f * teslaMeltdownStacks)
            cooldownUntil = Time.time + 0.5f;

        float feedback = Mathf.Clamp01(1f - totalHit * teslaFeedbackLoopMultiplier);
        return Mathf.Clamp(feedback, 0.42f, 1f);
    }

    int DamageTeslaTarget(EnemyHealth enemyHealth, Vector2 source, float multiplier, bool primary)
    {
        if (enemyHealth == null)
            return 0;

        int healthBefore = enemyHealth.GetCurrentHealth();
        int shockDamage = Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
        CreateTeslaArc(source, enemyHealth.transform.position);
        bool killed = enemyHealth.TakeDamage(shockDamage);
        ApplyTeslaControl(enemyHealth);
        NotifyTeslaHit();

        if (teslaCriticalExplosionChance > 0f && Random.value < teslaCriticalExplosionChance)
            TeslaPulse(enemyHealth.transform.position, 1.1f, 0.55f);
        if (killed)
        {
            if (teslaEnergyOverflow)
                teslaOverflowDamage += Mathf.Clamp01((shockDamage - healthBefore) / 5f);
            if (teslaVoltageCollapseMultiplier > 0f)
                TeslaPulse(enemyHealth.transform.position, 1.3f, teslaVoltageCollapseMultiplier);
        }

        return shockDamage;
    }

    void TryForkTesla(EnemyHealth fromEnemy, List<EnemyHealth> hitEnemies, float multiplier)
    {
        if (teslaForkChance <= 0f || Random.value > teslaForkChance || fromEnemy == null)
            return;

        EnemyHealth forkTarget = FindNextTeslaTarget(fromEnemy.transform.position, hitEnemies);
        if (forkTarget != null)
            DamageTeslaTarget(forkTarget, fromEnemy.transform.position, multiplier, false);
    }

    void ApplyTeslaControl(EnemyHealth enemyHealth)
    {
        Enemy enemy = enemyHealth.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (teslaSlowDuration > 0f)
                enemy.ApplySlow(teslaSlowMultiplier, teslaSlowDuration);
            if (teslaParalysisStacks > 0 && Random.value < 0.08f * teslaParalysisStacks)
                enemy.ApplyStun(0.28f + 0.12f * teslaParalysisStacks);
            if (teslaMagneticPull > 0f)
                PullEnemiesToward(enemyHealth.transform.position, teslaMagneticPull);
        }

        if (teslaIonizedBonus > 0f)
            enemyHealth.ApplyIonized(teslaIonizedBonus, 2.5f);
        if (teslaResidualDamageMultiplier > 0f)
            enemyHealth.ApplyResidualCurrent(Mathf.Max(1, Mathf.RoundToInt(damage * teslaResidualDamageMultiplier)), 2.4f, 0.8f);
    }

    EnemyHealth FindNextTeslaTarget(Vector2 source, List<EnemyHealth> ignored)
    {
        EnemyHealth best = null;
        float bestDistance = float.MaxValue;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(source, teslaChainRange);
        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag("Enemy"))
                continue;

            EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
            if (enemyHealth == null || ignored.Contains(enemyHealth))
                continue;

            float distance = Vector2.Distance(source, collider.transform.position);
            if (distance < bestDistance)
            {
                best = enemyHealth;
                bestDistance = distance;
            }
        }

        return best;
    }

    void TeslaPulse(Vector2 center, float radius, float damageMultiplier)
    {
        CreateTeslaPulseVisual(center, radius);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag("Enemy"))
                continue;

            EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
                continue;

            enemyHealth.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(damage * teslaDamageMultiplier * damageMultiplier)));
            ApplyTeslaControl(enemyHealth);
        }
    }

    void TickTeslaField()
    {
        if (!isTeslaTower || teslaFieldStacks <= 0 || Time.time < teslaFieldNextTick)
            return;

        teslaFieldNextTick = Time.time + 0.55f;
        TeslaPulse(transform.position, 1.9f + teslaFieldStacks * 0.7f, 0.22f * teslaFieldStacks);
    }

    void PullEnemiesToward(Vector2 center, float distance)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, 1.8f);
        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag("Enemy"))
                continue;

            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
                enemy.PullToward(center, distance);
        }
    }

    int CountEnemiesInRange(Vector2 center, float radius)
    {
        int count = 0;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
                count++;
        }

        return count;
    }

    void NotifyTeslaHit()
    {
        UpgradeManager upgradeManager = FindAnyObjectByType<UpgradeManager>();
        if (upgradeManager != null)
            upgradeManager.EnemyHitByBullet();
    }

    void FireShell(GameObject target, Bullet.ShellStats stats)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet shell = bullet.GetComponent<Bullet>();
        shell.Configure(stats);
        shell.SetTarget(target);
    }

    Bullet.ShellStats BuildShellStats()
    {
        bool siegeShot = siegeShellStacks > 0 && shotsFired % Mathf.Max(2, 6 - siegeShellStacks) == 0;
        bool apocalypseShot = apocalypseRound && shotsFired % 20 == 0;
        float damageBonus = shellDamageMultiplier;

        if (siegePlatformStacks > 0 && Time.time >= cooldownUntil)
            damageBonus *= 1f + 0.35f * siegePlatformStacks;

        Bullet.ShellStats stats = new Bullet.ShellStats
        {
            directDamage = Mathf.Max(1, Mathf.RoundToInt(damage * damageBonus)),
            directHitDamageMultiplier = directHitDamageMultiplier,
            splashRadius = splashRadius * delayedFuseRadiusMultiplier,
            splashDamageMultiplier = splashDamageMultiplier,
            speedMultiplier = projectileSpeedMultiplier,
            knockback = explosionKnockback,
            shrapnelEdgeDamageMultiplier = shrapnelEdgeDamageMultiplier,
            fragmentCount = fragmentCount,
            burningGroundDuration = burningGroundDuration,
            concussiveSlowMultiplier = concussiveSlowMultiplier,
            concussiveSlowDuration = concussiveSlowDuration,
            craterDuration = craterDuration,
            craterSlowMultiplier = craterSlowMultiplier,
            chainDetonationMultiplier = chainDetonationMultiplier,
            executionDamageMultiplier = executionDamageMultiplier,
            finisherDamageMultiplier = finisherDamageMultiplier,
            delayedFuseSeconds = delayedFuseSeconds,
            volatileSecondaryExplosionMultiplier = volatileSecondaryExplosionMultiplier,
        };

        if (siegeShot)
        {
            stats.directDamage = Mathf.RoundToInt(stats.directDamage * 1.8f);
            stats.splashRadius *= 1.6f;
        }

        if (apocalypseShot)
        {
            stats.directDamage = Mathf.RoundToInt(stats.directDamage * 4f);
            stats.splashRadius *= 3.2f;
            stats.splashDamageMultiplier = Mathf.Max(stats.splashDamageMultiplier, 0.8f);
        }

        return stats;
    }

    GameObject GetNearestEnemy()
    {
        float minDist = float.MaxValue;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist && dist <= attackRange)
            {
                minDist = dist;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
    public void OnEnemyDestroyed()
    {
        RetargetAndFire();
    }
}
