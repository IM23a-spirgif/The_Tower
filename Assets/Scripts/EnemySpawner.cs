using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform tower;
    public TextMeshProUGUI waveText;
    public Button startWaveButton;
    public float spawnRadius = 10f;
    private int currentWave = 1;
    private bool isSpawning = false;
    private int enemiesRemaining = 0;
    private int baseEnemyCount = 5;
    private int enemyCountIncrease = 3;
    private float enemySpeed = 1.5f;
    private int enemyBaseHP = 1;
    private float speedIncreaseRate = 0.2f;
    private int hpIncreaseRate = 1;
    private float maxEnemySpeed = 6f;
    private int maxEnemyHP = 20;

    public void StartWave()
    {
        if (!isSpawning)
        {
            currentWave++;
            UpdateWaveText();
            enemiesRemaining = baseEnemyCount + (currentWave) * enemyCountIncrease;
            startWaveButton.gameObject.SetActive(false);
            StartCoroutine(SpawnWave());
        }
    }

    IEnumerator SpawnWave()
    {
        isSpawning = true;
        int enemiesToSpawn = baseEnemyCount + (currentWave) * enemyCountIncrease;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(1f);
        }

        isSpawning = false;
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            // Lower speed scale to 0.1f
            float waveSpeedBonus = currentWave * 0.1f;
            enemyComponent.speed = Mathf.Min(maxEnemySpeed, enemySpeed + waveSpeedBonus);
        }

        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // For HP: baseHP + wave * 0.3 (round it)
            int scaledHP = enemyBaseHP + Mathf.RoundToInt(currentWave * 0.3f);
            scaledHP = Mathf.Min(maxEnemyHP, scaledHP);
            enemyHealth.SetHealth(scaledHP);

            enemyHealth.spawner = this;
        }
    }

    Vector2 GetRandomSpawnPosition()
    {
        float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
        return (Vector2)tower.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
    }

    void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + currentWave;
        }
    }

    public void EnemyDefeated()
    {
        enemiesRemaining--;
        if (enemiesRemaining <= 0)
        {
            startWaveButton.gameObject.SetActive(true);

            // If wave is multiple of 5, show special upgrade choice
            if (currentWave % 5 == 0)
            {
                UpgradeManager upgradeManager = FindObjectOfType<UpgradeManager>();
                if (upgradeManager != null)
                {
                    upgradeManager.ShowSpecialUpgradeChoice();
                }
            }
        }
    }
}