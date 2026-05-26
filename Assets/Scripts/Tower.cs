using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public static Tower Instance;
    public GameObject bulletPrefab;
    public int damage = 1;
    public float fireRate = 1f;
    public float attackRange = 5f;
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
    private float nextFireTime;
    private GameObject currentTarget;
    private int shotsFired;
    private float cooldownUntil;
    private bool cannonBaselineApplied;
    public int extraBullets = 0;  // For "Shoot +1 bullet"
    public bool homingEnabled = false;
    public bool ricochetEnabled = false;
    public bool pierceEnabled = false;

    void Awake()
    {
        ApplyCannonBaseline();
    }

    void Start()
    {
        ApplyCannonBaseline();
    }

    void Update()
    {
        if (Time.time < cooldownUntil)
            return;

        if (Time.time >= nextFireTime)
        {
            RetargetAndFire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void ApplyCannonBaseline()
    {
        if (cannonBaselineApplied)
            return;

        fireRate *= 1.1f;
        cannonBaselineApplied = true;
    }

    void RetargetAndFire()
    {
        currentTarget = GetNearestEnemy();
        if (currentTarget != null)
        {
            FireAtTarget(currentTarget);
        }
    }

    void FireAtTarget(GameObject target)
    {
        shotsFired++;
        FireShell(target, BuildShellStats());

        if (Random.value < doubleChargeChance)
            FireShell(target, BuildShellStats());

        if (glassCannonDesign && shotsFired % 10 == 0)
            cooldownUntil = Time.time + 1.75f;
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
