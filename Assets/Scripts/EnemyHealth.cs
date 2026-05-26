using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 1;
    public GameObject shatterEffectPrefab;
    public EnemySpawner spawner;
    private int maxHealth = 1;

    public bool TakeDamage(int damage)
    {
        if (health <= 0)
            return false;

        health -= damage;
        if (health <= 0)
        {
            NotifySpawner();
            ShatterEffect();
            NotifyUpgradeManager();
            Destroy(gameObject);
            return true;
        }

        return false;
    }

    public int GetCurrentHealth()
    {
        return health;
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth;
        maxHealth = Mathf.Max(1, newHealth);
    }

    public float GetHealthPercent()
    {
        return maxHealth <= 0 ? 0f : (float)health / maxHealth;
    }

    public void NotifySpawner()
    {
        if (spawner != null)
        {
            spawner.EnemyDefeated();
        }
    }
    
    void NotifyUpgradeManager()
    {
        UpgradeManager upgradeManager = FindAnyObjectByType<UpgradeManager>();
        if (upgradeManager != null)
        {
            upgradeManager.EnemyKilled();
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
