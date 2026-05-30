using TMPro;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 1;
    public GameObject shatterEffectPrefab;
    public EnemySpawner spawner;
    public bool isBoss;
    private int maxHealth = 1;
    private float ionizedDamageMultiplier = 1f;
    private float ionizedEndTime;
    private Coroutine residualCurrentRoutine;

    public bool TakeDamage(int damage)
    {
        if (health <= 0)
            return false;

        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * GetDamageTakenMultiplier()));
        ShowDamageNumber(finalDamage);
        health -= finalDamage;
        if (isBoss && spawner != null)
            spawner.UpdateBossHealth(this, health, maxHealth);

        if (health <= 0)
        {
            if (isBoss && spawner != null)
                spawner.HideBossHealth(this);

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
        if (isBoss && spawner != null)
            spawner.UpdateBossHealth(this, health, maxHealth);
    }

    public float GetHealthPercent()
    {
        return maxHealth <= 0 ? 0f : (float)health / maxHealth;
    }

    public float GetDamageTakenMultiplier()
    {
        if (Time.time >= ionizedEndTime)
            ionizedDamageMultiplier = 1f;

        return ionizedDamageMultiplier;
    }

    public void ApplyIonized(float extraDamageMultiplier, float duration)
    {
        ionizedDamageMultiplier = Mathf.Max(ionizedDamageMultiplier, 1f + extraDamageMultiplier);
        ionizedEndTime = Mathf.Max(ionizedEndTime, Time.time + duration);
    }

    public void ApplyResidualCurrent(int damagePerTick, float duration, float tickInterval)
    {
        if (residualCurrentRoutine != null)
            StopCoroutine(residualCurrentRoutine);

        residualCurrentRoutine = StartCoroutine(ResidualCurrent(damagePerTick, duration, tickInterval));
    }

    IEnumerator ResidualCurrent(int damagePerTick, float duration, float tickInterval)
    {
        float endTime = Time.time + duration;
        while (Time.time < endTime && health > 0)
        {
            yield return new WaitForSeconds(tickInterval);
            if (health > 0)
                TakeDamage(damagePerTick);
        }

        residualCurrentRoutine = null;
    }

    public void NotifySpawner()
    {
        if (spawner != null)
        {
            if (isBoss)
                spawner.HideBossHealth(this);
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

    void ShowDamageNumber(int damage)
    {
        GameObject textObject = new GameObject("EnemyDamageText", typeof(TextMeshPro));
        textObject.transform.position = transform.position + new Vector3(Random.Range(-0.18f, 0.18f), 0.72f, 0f);

        TextMeshPro text = textObject.GetComponent<TextMeshPro>();
        text.text = damage.ToString();
        text.fontSize = 3.1f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = GetDamageTakenMultiplier() > 1f
            ? new Color(0.4f, 0.9f, 1f, 1f)
            : new Color(1f, 0.88f, 0.58f, 1f);
        text.sortingOrder = 50;
        textObject.AddComponent<FloatingWorldText>().Configure(0.75f, new Vector3(0f, 0.58f, 0f));
    }
}
