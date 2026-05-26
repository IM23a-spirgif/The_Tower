using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 1.5f;
    private Transform tower;
    private EnemyHealth enemyHealth;
    private float slowMultiplier = 1f;
    private float slowEndTime;

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

        transform.position = Vector2.MoveTowards(transform.position, tower.position, speed * slowMultiplier * Time.deltaTime);
        if (Vector2.Distance(transform.position, tower.position) < 0.5f)
        {
            int damageDealt = enemyHealth.GetCurrentHealth();
            tower.GetComponent<TowerHealth>().TakeDamage(damageDealt);
            enemyHealth.NotifySpawner();
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

        transform.position += (Vector3)(direction * distance);
    }
}
