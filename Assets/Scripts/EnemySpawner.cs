using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    const int WavesPerStage = 15;
    const string CurrentStageKey = "CurrentStage";

    public GameObject enemyPrefab;
    public Transform tower;
    public TextMeshProUGUI waveText;
    public Button startWaveButton;
    public float spawnRadius = 10f;
    private int currentWave = 0;
    private int currentStage = 1;
    private bool isSpawning = false;
    private bool stageComplete = false;
    private int enemiesRemaining = 0;
    private int baseEnemyCount = 5;
    private int enemyCountIncrease = 3;
    private float enemySpeed = 1.5f;
    private int enemyBaseHP = 1;
    private float maxEnemySpeed = 6f;
    private int maxEnemyHP = 20;

    void Start()
    {
        currentStage = Mathf.Max(1, PlayerPrefs.GetInt(CurrentStageKey, 1));
        EnsureWaveText();
        StyleStartWaveButton();
        UpdateWaveText();
    }

    public void StartWave()
    {
        if (isSpawning || stageComplete || currentWave >= WavesPerStage)
            return;

        currentWave++;
        isSpawning = true;
        UpdateWaveText();
        enemiesRemaining = GetEnemyCountForWave(currentWave);
        if (startWaveButton != null)
            startWaveButton.gameObject.SetActive(false);
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        int enemiesToSpawn = GetNormalEnemyCount(currentWave);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy(false);
            yield return new WaitForSeconds(1f);
        }

        if (IsBossWave(currentWave))
        {
            SpawnEnemy(true);
        }

        isSpawning = false;
    }

    void SpawnEnemy(bool boss)
    {
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            // Lower speed scale to 0.1f
            float waveSpeedBonus = currentWave * 0.1f;
            float stageSpeedBonus = (currentStage - 1) * 0.08f;
            enemyComponent.speed = Mathf.Min(maxEnemySpeed, enemySpeed + waveSpeedBonus + stageSpeedBonus);
            if (boss)
                enemyComponent.speed *= 0.72f;
        }

        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // For HP: baseHP + wave * 0.3 (round it)
            int scaledHP = enemyBaseHP + Mathf.RoundToInt(currentWave * 0.3f) + Mathf.RoundToInt((currentStage - 1) * 0.5f);
            if (boss)
                scaledHP = Mathf.RoundToInt((scaledHP + currentStage + currentWave) * 4.5f);
            scaledHP = Mathf.Min(boss ? maxEnemyHP * 4 : maxEnemyHP, scaledHP);
            enemyHealth.SetHealth(scaledHP);

            enemyHealth.spawner = this;
        }

        if (boss)
        {
            newEnemy.name = $"Boss Wave {currentWave}";
            newEnemy.transform.localScale *= 1.8f;
            SpriteRenderer spriteRenderer = newEnemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(0.95f, 0.32f, 0.28f, 1f);
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
            int displayWave = Mathf.Clamp(isSpawning ? currentWave : currentWave + 1, 1, WavesPerStage);
            waveText.text = $"Wave {displayWave}/{WavesPerStage}";
        }
    }

    public void EnemyDefeated()
    {
        if (stageComplete)
            return;

        enemiesRemaining--;
        if (enemiesRemaining <= 0)
        {
            if (currentWave >= WavesPerStage)
            {
                CompleteStage();
                return;
            }

            UpgradeManager upgradeManager = FindAnyObjectByType<UpgradeManager>();
            if (upgradeManager != null)
            {
                upgradeManager.ShowWaveUpgradeChoice(PrepareNextWave);
                return;
            }

            PrepareNextWave();
        }
    }

    void PrepareNextWave()
    {
        if (startWaveButton != null)
            startWaveButton.gameObject.SetActive(true);
        UpdateWaveText();
    }

    int GetNormalEnemyCount(int wave)
    {
        return baseEnemyCount + wave * enemyCountIncrease + Mathf.Max(0, currentStage - 1);
    }

    int GetEnemyCountForWave(int wave)
    {
        return GetNormalEnemyCount(wave) + (IsBossWave(wave) ? 1 : 0);
    }

    static bool IsBossWave(int wave)
    {
        return wave == 5 || wave == 10 || wave == 15;
    }

    void CompleteStage()
    {
        stageComplete = true;
        isSpawning = false;
        int nextStage = currentStage + 1;
        PlayerPrefs.SetInt(CurrentStageKey, nextStage);
        PlayerPrefs.Save();
        ShowWinScreen(nextStage);
    }

    void ShowWinScreen(int nextStage)
    {
        Time.timeScale = 0f;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        GameObject overlay = new GameObject("WinOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        Image background = overlay.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.78f);
        background.raycastTarget = true;

        TextMeshProUGUI title = CreateOverlayText("Title", rect, "YOU WIN", 46f, FontStyles.Bold);
        PlaceOverlayRect(title.rectTransform, 0f, 76f, 420f, 70f);

        TextMeshProUGUI subtitle = CreateOverlayText("Subtitle", rect, $"Stage {currentStage} complete. Stage {nextStage} unlocked.", 20f, FontStyles.Normal);
        PlaceOverlayRect(subtitle.rectTransform, 0f, 22f, 520f, 42f);

        Button menuButton = CreateOverlayButton("MenuButton", rect, "MAIN MENU", ReturnToMenu);
        PlaceOverlayRect(menuButton.GetComponent<RectTransform>(), 0f, -58f, 220f, 54f);
    }

    void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
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

    static TextMeshProUGUI CreateOverlayText(string name, Transform parent, string text, float fontSize, FontStyles style)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.94f, 0.96f, 1f, 1f);
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    static Button CreateOverlayButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        Image image = obj.GetComponent<Image>();
        image.color = new Color(0.18f, 0.5f, 0.74f, 1f);

        Button button = obj.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.25f, 0.62f, 0.88f, 1f);
        colors.pressedColor = new Color(0.12f, 0.36f, 0.56f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.16f, 0.18f, 0.22f, 1f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        TextMeshProUGUI text = CreateOverlayText("Text", rect, label, 18f, FontStyles.Bold);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    static void PlaceOverlayRect(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }
}
