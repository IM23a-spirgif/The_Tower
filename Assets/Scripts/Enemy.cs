using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 1.5f;
    private Transform tower;
    private EnemyHealth enemyHealth;
    private float slowMultiplier = 1f;
    private float slowEndTime;
    private Vector2 knockbackVelocity;
    private float knockbackEndTime;

    void Start()
    {
        GameObject towerObject = GameObject.FindGameObjectWithTag("Tower");

        if (towerObject != null)
        {
            tower = towerObject.transform;
        }
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (tower == null || enemyHealth == null) return;
        if (Time.time >= slowEndTime)
            slowMultiplier = 1f;

        if (Time.time < knockbackEndTime && knockbackVelocity.sqrMagnitude > 0.01f)
        {
            transform.position += (Vector3)(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, 8f * Time.deltaTime);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, tower.position, speed * slowMultiplier * Time.deltaTime);
        if (Vector2.Distance(transform.position, tower.position) < 0.5f)
        {
            int damageDealt = enemyHealth.GetCurrentHealth();
            tower.GetComponent<TowerHealth>().TakeDamage(damageDealt);
            enemyHealth.NotifySpawner();
            if (enemyHealth.isBoss && enemyHealth.spawner != null)
                enemyHealth.spawner.HideBossHealth(enemyHealth);
            Destroy(gameObject);
        }
    }

    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = Mathf.Min(slowMultiplier, Mathf.Clamp(multiplier, 0.1f, 1f));
        slowEndTime = Mathf.Max(slowEndTime, Time.time + duration);
    }

    public void ApplyKnockback(Vector2 source, float distance)
    {
        Vector2 direction = ((Vector2)transform.position - source).normalized;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        transform.position += (Vector3)(direction * distance * 0.45f);
        knockbackVelocity = direction * Mathf.Max(2.5f, distance * 12f);
        knockbackEndTime = Time.time + 0.22f;
    }
}
