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

    void Start()
    {
        EnsureWaveText();
        StyleStartWaveButton();
        UpdateWaveText();
    }

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
            waveText.text = $"WAVE {currentWave}";
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
                UpgradeManager upgradeManager = FindAnyObjectByType<UpgradeManager>();
                if (upgradeManager != null)
                {
                    upgradeManager.ShowSpecialUpgradeChoice();
                }
            }
        }
    }

    void EnsureWaveText()
    {
        if (waveText != null)
            return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        Transform existing = canvas.transform.Find("WaveCounter");
        if (existing != null)
        {
            waveText = existing.GetComponentInChildren<TextMeshProUGUI>();
            if (waveText != null)
                return;
        }

        GameObject panelObject = new GameObject("WaveCounter", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.SetParent(canvas.transform, false);
        panel.anchorMin = new Vector2(0.5f, 1f);
        panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchoredPosition = new Vector2(0f, -24f);
        panel.sizeDelta = new Vector2(150f, 42f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.06f, 0.08f, 0.84f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(panel, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        waveText = textObject.GetComponent<TextMeshProUGUI>();
        waveText.alignment = TextAlignmentOptions.Center;
        waveText.fontSize = 21f;
        waveText.fontStyle = FontStyles.Bold;
        waveText.color = new Color(0.92f, 0.95f, 1f, 1f);
        waveText.raycastTarget = false;
    }

    void StyleStartWaveButton()
    {
        if (startWaveButton == null)
            return;

        RectTransform rect = startWaveButton.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -76f);
            rect.sizeDelta = new Vector2(180f, 44f);
        }

        Image image = startWaveButton.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.2f, 0.66f, 0.9f, 1f);
        }

        ColorBlock colors = startWaveButton.colors;
        colors.normalColor = new Color(0.2f, 0.66f, 0.9f, 1f);
        colors.highlightedColor = new Color(0.33f, 0.78f, 1f, 1f);
        colors.pressedColor = new Color(0.12f, 0.48f, 0.7f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.18f, 0.22f, 0.27f, 0.7f);
        colors.colorMultiplier = 1f;
        startWaveButton.colors = colors;

        TextMeshProUGUI label = startWaveButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = "START WAVE";
            label.fontSize = 20f;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.raycastTarget = false;
        }
    }
}
