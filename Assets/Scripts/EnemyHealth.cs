using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 1;
    public GameObject shatterEffectPrefab;
    public EnemySpawner spawner;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            NotifySpawner();
            ShatterEffect();
            NotifyUpgradeManager();
            Destroy(gameObject);
        }
    }

    public int GetCurrentHealth()
    {
        return health;
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth;
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
        UpgradeManager upgradeManager = FindObjectOfType<UpgradeManager>();
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