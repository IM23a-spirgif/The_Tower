using System.Collections.Generic;
using UnityEngine;

public class CannonAreaEffect : MonoBehaviour
{
    readonly Dictionary<EnemyHealth, float> nextDamageTick = new Dictionary<EnemyHealth, float>();

    float duration;
    int damagePerSecond;
    float slowMultiplier = 1f;
    float expiresAt;

    public void Configure(float effectDuration, int burnDamagePerSecond, float craterSlowMultiplier)
    {
        duration = effectDuration;
        damagePerSecond = burnDamagePerSecond;
        slowMultiplier = Mathf.Clamp(craterSlowMultiplier, 0.1f, 1f);
        expiresAt = Time.time + duration;
        Destroy(gameObject, duration);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy"))
            return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && slowMultiplier < 1f)
            enemy.ApplySlow(slowMultiplier, 0.18f);

        if (damagePerSecond <= 0)
            return;

        EnemyHealth health = other.GetComponent<EnemyHealth>();
        if (health == null)
            return;

        if (!nextDamageTick.TryGetValue(health, out float nextTick))
            nextTick = 0f;

        if (Time.time < nextTick || Time.time >= expiresAt)
            return;

        health.TakeDamage(Mathf.Max(1, damagePerSecond));
        nextDamageTick[health] = Time.time + 1f;
    }
}
