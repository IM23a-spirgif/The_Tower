using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public struct ShellStats
    {
        public int directDamage;
        public float directHitDamageMultiplier;
        public float splashRadius;
        public float splashDamageMultiplier;
        public float speedMultiplier;
        public float knockback;
        public float shrapnelEdgeDamageMultiplier;
        public int fragmentCount;
        public float burningGroundDuration;
        public float concussiveSlowMultiplier;
        public float concussiveSlowDuration;
        public float craterDuration;
        public float craterSlowMultiplier;
        public float chainDetonationMultiplier;
        public float executionDamageMultiplier;
        public float finisherDamageMultiplier;
        public float delayedFuseSeconds;
        public float volatileSecondaryExplosionMultiplier;
    }

    public float speed = 5f;
    public GameObject shatterEffectPrefab;

    ShellStats stats;
    Transform target;
    Vector2 lastKnownDirection;
    bool hasTarget = true;
    bool exploded;

    void Awake()
    {
        stats = new ShellStats
        {
            directDamage = 1,
            directHitDamageMultiplier = 1f,
            splashRadius = 1.15f,
            splashDamageMultiplier = 0.45f,
            speedMultiplier = 1f,
            concussiveSlowMultiplier = 1f,
            craterSlowMultiplier = 1f,
        };
    }

    public void Configure(ShellStats shellStats)
    {
        stats = shellStats;
        stats.directDamage = Mathf.Max(1, stats.directDamage);
        stats.directHitDamageMultiplier = Mathf.Max(0f, stats.directHitDamageMultiplier);
        stats.splashRadius = Mathf.Max(0.25f, stats.splashRadius);
        stats.speedMultiplier = Mathf.Max(0.1f, stats.speedMultiplier);
        if (stats.concussiveSlowMultiplier <= 0f)
            stats.concussiveSlowMultiplier = 1f;
        if (stats.craterSlowMultiplier <= 0f)
            stats.craterSlowMultiplier = 1f;
    }

    public void SetTarget(GameObject enemy)
    {
        if (enemy != null)
        {
            target = enemy.transform;
            lastKnownDirection = (target.position - transform.position).normalized;
            UpdateRotation();
        }
    }

    public void SetDamage(int newDamage)
    {
        stats.directDamage = newDamage;
    }

    void Update()
    {
        if (exploded)
            return;

        if (hasTarget && target != null)
        {
            lastKnownDirection = (target.position - transform.position).normalized;
        }
        else
        {
            hasTarget = false;
        }

        UpdateRotation();
        transform.position += (Vector3)lastKnownDirection * (speed * stats.speedMultiplier) * Time.deltaTime;
        Destroy(gameObject, 5f);
    }

    void UpdateRotation()
    {
        if (lastKnownDirection.sqrMagnitude <= Mathf.Epsilon)
            return;

        transform.right = lastKnownDirection;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!exploded && other.CompareTag("Enemy"))
            StartCoroutine(ExplodeAfterFuse(other.GetComponent<EnemyHealth>()));
    }

    IEnumerator ExplodeAfterFuse(EnemyHealth directTarget)
    {
        exploded = true;
        if (stats.delayedFuseSeconds > 0f)
        {
            GetComponent<Collider2D>().enabled = false;
            float elapsed = 0f;
            while (elapsed < stats.delayedFuseSeconds)
            {
                elapsed += Time.deltaTime;
                transform.position += (Vector3)lastKnownDirection * (speed * stats.speedMultiplier) * Time.deltaTime;
                yield return null;
            }
        }

        Explode(directTarget);
        Destroy(gameObject);
    }

    void Explode(EnemyHealth directTarget)
    {
        Vector2 center = transform.position;
        NotifyBulletHit();
        ShatterEffect();
        CreateShockwave(center, stats.splashRadius);

        HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();
        bool directKilled = false;

        if (directTarget != null)
        {
            Collider2D directCollider = directTarget.GetComponent<Collider2D>();
            if (directCollider != null)
                ApplyExplosionControl(directCollider, center - lastKnownDirection * 0.35f);

            int directDamage = CalculateDirectDamage(directTarget);
            directKilled = directTarget.TakeDamage(directDamage);
            hitEnemies.Add(directTarget);
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, stats.splashRadius);
        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag("Enemy"))
                continue;

            EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
            if (enemyHealth == null || hitEnemies.Contains(enemyHealth))
                continue;

            float distance = Vector2.Distance(center, collider.transform.position);
            float falloff = Mathf.Clamp01(1f - distance / stats.splashRadius);
            int splashDamage = Mathf.Max(1, Mathf.RoundToInt(stats.directDamage * stats.splashDamageMultiplier * Mathf.Lerp(0.5f, 1f, falloff)));

            if (stats.shrapnelEdgeDamageMultiplier > 0f && distance >= stats.splashRadius * 0.68f)
                splashDamage += Mathf.Max(1, Mathf.RoundToInt(stats.directDamage * stats.shrapnelEdgeDamageMultiplier));

            bool killed = enemyHealth.TakeDamage(splashDamage);
            hitEnemies.Add(enemyHealth);
            ApplyExplosionControl(collider, center);

            if (killed)
                TriggerChainDetonation(collider.transform.position);
        }

        if (directKilled)
        {
            TriggerChainDetonation(center);
            TriggerVolatileDetonation(center);
        }

        SpawnFragments(center);
        SpawnLingeringArea(center);
    }

    int CalculateDirectDamage(EnemyHealth targetHealth)
    {
        float multiplier = stats.directHitDamageMultiplier;
        float healthPercent = targetHealth.GetHealthPercent();

        if (healthPercent >= 0.8f)
            multiplier *= 1f + stats.executionDamageMultiplier;
        if (healthPercent <= 0.3f)
            multiplier *= 1f + stats.finisherDamageMultiplier;

        return Mathf.Max(1, Mathf.RoundToInt(stats.directDamage * multiplier));
    }

    void ApplyExplosionControl(Collider2D enemyCollider, Vector2 center)
    {
        Enemy enemy = enemyCollider.GetComponent<Enemy>();
        if (enemy == null)
            return;

        if (stats.knockback > 0f)
            enemy.ApplyKnockback(center, stats.knockback);
        if (stats.concussiveSlowDuration > 0f)
            enemy.ApplySlow(stats.concussiveSlowMultiplier, stats.concussiveSlowDuration);
    }

    void SpawnFragments(Vector2 center)
    {
        if (stats.fragmentCount <= 0)
            return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, stats.splashRadius * 1.75f);
        int fragmentsLeft = stats.fragmentCount;
        foreach (Collider2D collider in colliders)
        {
            if (fragmentsLeft <= 0)
                break;
            if (!collider.CompareTag("Enemy"))
                continue;

            EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
                continue;

            CreateFragmentVisual(center, collider.transform.position);
            enemyHealth.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(stats.directDamage * 0.35f)));
            fragmentsLeft--;
        }
    }

    void SpawnLingeringArea(Vector2 center)
    {
        if (stats.burningGroundDuration <= 0f && stats.craterDuration <= 0f)
            return;

        GameObject area = new GameObject("CannonLingeringArea", typeof(CircleCollider2D), typeof(CannonAreaEffect));
        area.transform.position = center;

        CircleCollider2D circle = area.GetComponent<CircleCollider2D>();
        circle.isTrigger = true;
        circle.radius = stats.splashRadius * 0.72f;

        CannonAreaEffect effect = area.GetComponent<CannonAreaEffect>();
        effect.Configure(
            Mathf.Max(stats.burningGroundDuration, stats.craterDuration),
            stats.burningGroundDuration > 0f ? Mathf.Max(1, Mathf.RoundToInt(stats.directDamage * 0.18f)) : 0,
            stats.craterDuration > 0f ? stats.craterSlowMultiplier : 1f,
            stats.splashRadius * 0.72f);
    }

    void TriggerChainDetonation(Vector2 center)
    {
        if (stats.chainDetonationMultiplier <= 0f)
            return;

        DamageSecondaryExplosion(center, stats.splashRadius * 0.65f, stats.chainDetonationMultiplier);
    }

    void TriggerVolatileDetonation(Vector2 center)
    {
        if (stats.volatileSecondaryExplosionMultiplier <= 0f || Random.value > 0.2f)
            return;

        DamageSecondaryExplosion(center, stats.splashRadius * 0.75f, stats.volatileSecondaryExplosionMultiplier);
    }

    void DamageSecondaryExplosion(Vector2 center, float radius, float damageMultiplier)
    {
        CreateShockwave(center, radius);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag("Enemy"))
                continue;

            EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(stats.directDamage * damageMultiplier)));
        }
    }

    void NotifyBulletHit()
    {
        UpgradeManager upgradeManager = FindAnyObjectByType<UpgradeManager>();
        if (upgradeManager != null)
            upgradeManager.EnemyHitByBullet();
    }

    void ShatterEffect()
    {
        if (shatterEffectPrefab != null)
            Instantiate(shatterEffectPrefab, transform.position, Quaternion.identity);
    }

    static void CreateShockwave(Vector2 center, float radius)
    {
        GameObject ring = new GameObject("CannonShockwave", typeof(LineRenderer), typeof(ShockwaveEffect));
        ring.transform.position = center;

        LineRenderer line = ring.GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 48;
        line.startWidth = 0.035f;
        line.endWidth = 0.035f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.95f, 0.82f, 0.45f, 0.82f);
        line.endColor = new Color(0.95f, 0.82f, 0.45f, 0.82f);

        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = (float)i / line.positionCount * Mathf.PI * 2f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }

        ring.GetComponent<ShockwaveEffect>().Configure(0.28f);
    }

    static void CreateFragmentVisual(Vector2 start, Vector2 end)
    {
        GameObject fragment = new GameObject("ShrapnelFragment", typeof(LineRenderer), typeof(ShockwaveEffect));
        fragment.transform.position = start;

        LineRenderer line = fragment.GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.045f;
        line.endWidth = 0.012f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.82f, 0.84f, 0.88f, 0.95f);
        line.endColor = new Color(0.82f, 0.84f, 0.88f, 0.1f);

        fragment.GetComponent<ShockwaveEffect>().Configure(0.18f);
    }
}
