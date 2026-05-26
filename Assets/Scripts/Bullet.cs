using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    private int damage = 1;
    private Transform target;
    public GameObject shatterEffectPrefab;
    private Vector2 lastKnownDirection;
    private bool hasTarget = true; // Track if bullet has an active target

    public void SetTarget(GameObject enemy)
    {
        if (enemy != null)
        {
            target = enemy.transform;
            lastKnownDirection = (target.position - transform.position).normalized; // Store initial direction
            UpdateRotation();
        }
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    void Update()
    {
        if (hasTarget && target != null)
        {
            lastKnownDirection = (target.position - transform.position).normalized; // Keep updating direction
        }
        else
        {
            hasTarget = false; // The target is gone, continue in last direction
        }

        // Move bullet in the last known direction
        UpdateRotation();
        transform.position += (Vector3)lastKnownDirection * speed * Time.deltaTime;

        // Destroy bullet after 5 seconds to prevent infinite movement
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
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyHealth>().TakeDamage(damage);
            ShatterEffect();
            Destroy(gameObject);
        }
    }

    void ShatterEffect()
    {
        if (shatterEffectPrefab != null)
        {
            Instantiate(shatterEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
