using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    const int WavesPerStage = 15;
    const string CurrentStageKey = "CurrentStage";
    const string TeslaTowerUnlockedKey = "TeslaTowerUnlocked";

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
    private int baseEnemyCount = 9;
    private int enemyCountIncrease = 5;
    private float enemySpeed = 1.05f;
    private int enemyBaseHP = 1;
    private float maxEnemySpeed = 5.2f;
    private int maxEnemyHP = 32;
    private RectTransform bossHealthBar;
    private Image bossHealthFill;
    private TextMeshProUGUI bossHealthText;
    private EnemyHealth activeBoss;

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
            yield return new WaitForSeconds(0.72f);
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
            float waveSpeedBonus = currentWave * 0.022f;
            float stageSpeedBonus = (currentStage - 1) * 0.018f;
            enemyComponent.speed = Mathf.Min(maxEnemySpeed, enemySpeed + waveSpeedBonus + stageSpeedBonus);
            if (boss)
                enemyComponent.speed *= 0.72f;
        }

        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            int scaledHP = enemyBaseHP
                + Mathf.RoundToInt((currentWave - 1) * 0.15f)
                + Mathf.RoundToInt((currentStage - 1) * 0.45f);
            if (boss)
                scaledHP = Mathf.RoundToInt((scaledHP + currentStage + currentWave * 0.4f) * 2.6f);
            scaledHP = Mathf.Min(boss ? maxEnemyHP * 4 : maxEnemyHP, scaledHP);
            enemyHealth.SetHealth(scaledHP);

            enemyHealth.spawner = this;
            enemyHealth.isBoss = boss;
        }

        if (boss)
        {
            newEnemy.name = $"Boss Wave {currentWave}";
            newEnemy.transform.localScale *= 1.8f;
            SpriteRenderer spriteRenderer = newEnemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(0.95f, 0.32f, 0.28f, 1f);

            activeBoss = enemyHealth;
            EnsureBossHealthBar();
            UpdateBossHealth(enemyHealth, enemyHealth.GetCurrentHealth(), enemyHealth.GetCurrentHealth());
        }
        else
        {
            ApplyEnemyVariant(newEnemy, enemyComponent, enemyHealth);
        }
    }

    void ApplyEnemyVariant(GameObject enemyObject, Enemy enemyComponent, EnemyHealth enemyHealth)
    {
        if (enemyComponent == null || enemyHealth == null)
            return;

        float waveFactor = Mathf.Clamp01(currentWave / 15f);
        float fastChance = Mathf.Lerp(0.08f, 0.24f, waveFactor);
        float tankChance = Mathf.Lerp(0.05f, 0.2f, waveFactor);
        float roll = Random.value;

        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        if (roll < fastChance)
        {
            enemyObject.name = "Fast Enemy";
            enemyComponent.speed *= 1.55f;
            enemyObject.transform.localScale *= 0.82f;
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(0.35f, 0.78f, 1f, 1f);
        }
        else if (roll < fastChance + tankChance)
        {
            enemyObject.name = "Tank Enemy";
            enemyComponent.speed *= 0.62f;
            enemyObject.transform.localScale *= 1.32f;
            enemyHealth.SetHealth(Mathf.Max(2, Mathf.RoundToInt(enemyHealth.GetCurrentHealth() * 1.8f)));
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(0.78f, 0.56f, 0.28f, 1f);
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
                StartCoroutine(ShowUpgradeChoiceAfterIntermission(upgradeManager, IsBossWave(currentWave)));
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

    IEnumerator ShowUpgradeChoiceAfterIntermission(UpgradeManager upgradeManager, bool bossWave)
    {
        yield return new WaitForSeconds(1f);
        if (upgradeManager != null)
            upgradeManager.ShowWaveUpgradeChoice(PrepareNextWave, bossWave);
        else
            PrepareNextWave();
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
        HideBossHealth(activeBoss);
        int nextStage = currentStage + 1;
        bool unlockedTeslaTower = currentStage == 1 && PlayerPrefs.GetInt(TeslaTowerUnlockedKey, 0) == 0;
        PlayerPrefs.SetInt(CurrentStageKey, nextStage);
        if (unlockedTeslaTower)
            PlayerPrefs.SetInt(TeslaTowerUnlockedKey, 1);
        PlayerPrefs.Save();
        ShowWinScreen(nextStage, unlockedTeslaTower);
    }

    public void UpdateBossHealth(EnemyHealth boss, int currentHealth, int maxHealth)
    {
        if (boss == null)
            return;

        activeBoss = boss;
        EnsureBossHealthBar();
        if (bossHealthBar != null)
            bossHealthBar.gameObject.SetActive(true);

        float percent = maxHealth <= 0 ? 0f : Mathf.Clamp01((float)Mathf.Max(0, currentHealth) / maxHealth);
        if (bossHealthFill != null)
        {
            bossHealthFill.fillAmount = 1f;
            bossHealthFill.color = Color.Lerp(new Color(0.48f, 0.035f, 0.055f, 1f), UITheme.Red, percent);
            RectTransform fillRect = bossHealthFill.rectTransform;
            fillRect.anchorMax = new Vector2(percent, fillRect.anchorMax.y);
            fillRect.offsetMax = Vector2.zero;
        }

        if (bossHealthText != null)
            bossHealthText.text = $"BOSS {Mathf.Max(0, currentHealth)}/{maxHealth}";
    }

    public void HideBossHealth(EnemyHealth boss)
    {
        if (boss != null && activeBoss != null && boss != activeBoss)
            return;

        activeBoss = null;
        if (bossHealthBar != null)
            bossHealthBar.gameObject.SetActive(false);
    }

    void ShowWinScreen(int nextStage, bool unlockedTeslaTower)
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
        background.color = new Color(0.005f, 0.012f, 0.022f, 0.9f);
        background.raycastTarget = true;

        UITheme.AddAccent(rect, "VictoryLine", UITheme.Green, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220f, 118f), new Vector2(220f, 121f));
        TextMeshProUGUI title = CreateOverlayText("Title", rect, "STAGE SECURED", 46f, FontStyles.Bold);
        title.color = UITheme.Green;
        PlaceOverlayRect(title.rectTransform, 0f, 76f, 420f, 70f);

        TextMeshProUGUI subtitle = CreateOverlayText("Subtitle", rect, $"Stage {currentStage} complete. Stage {nextStage} unlocked.", 20f, FontStyles.Normal);
        PlaceOverlayRect(subtitle.rectTransform, 0f, unlockedTeslaTower ? 30f : 22f, 520f, 42f);

        if (unlockedTeslaTower)
        {
            TextMeshProUGUI unlockText = CreateOverlayText("UnlockText", rect, "You unlocked: Tesla Tower", 22f, FontStyles.Bold);
            unlockText.color = UITheme.CyanBright;
            PlaceOverlayRect(unlockText.rectTransform, 0f, -14f, 520f, 42f);
        }

        Button menuButton = CreateOverlayButton("MenuButton", rect, "MAIN MENU", ReturnToMenu);
        PlaceOverlayRect(menuButton.GetComponent<RectTransform>(), 0f, unlockedTeslaTower ? -86f : -58f, 220f, 54f);
    }

    void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void EnsureBossHealthBar()
    {
        if (bossHealthBar != null)
            return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        Transform existing = canvas.transform.Find("BossHealthBar");
        bossHealthBar = existing != null ? existing.GetComponent<RectTransform>() : CreateRect("BossHealthBar", canvas.transform);
        bossHealthBar.anchorMin = new Vector2(0.5f, 1f);
        bossHealthBar.anchorMax = new Vector2(0.5f, 1f);
        bossHealthBar.pivot = new Vector2(0.5f, 1f);
        bossHealthBar.anchoredPosition = new Vector2(0f, -126f);
        bossHealthBar.sizeDelta = new Vector2(360f, 48f);
        bossHealthBar.gameObject.SetActive(false);

        Image panel = bossHealthBar.GetComponent<Image>();
        if (panel == null)
            panel = bossHealthBar.gameObject.AddComponent<Image>();
        UITheme.StylePanel(panel, new Color(0.12f, 0.035f, 0.05f, 0.94f), new Color(0.9f, 0.18f, 0.22f, 0.9f));
        UITheme.AddTopAccent(bossHealthBar, UITheme.Red, 3f);

        RectTransform track = bossHealthBar.Find("Track")?.GetComponent<RectTransform>();
        if (track == null)
        {
            track = CreateRect("Track", bossHealthBar);
            track.anchorMin = new Vector2(0f, 0f);
            track.anchorMax = new Vector2(1f, 0f);
            track.pivot = new Vector2(0.5f, 0f);
            track.anchoredPosition = new Vector2(0f, 8f);
            track.sizeDelta = new Vector2(-24f, 16f);
            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = new Color(0.2f, 0.06f, 0.08f, 1f);
        }

        bossHealthFill = track.Find("Fill")?.GetComponent<Image>();
        if (bossHealthFill == null)
        {
            RectTransform fill = CreateRect("Fill", track);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            bossHealthFill = fill.gameObject.AddComponent<Image>();
        }
        bossHealthFill.type = Image.Type.Simple;

        bossHealthText = bossHealthBar.Find("Text")?.GetComponent<TextMeshProUGUI>();
        if (bossHealthText == null)
        {
            bossHealthText = CreateOverlayText("Text", bossHealthBar, "BOSS", 15f, FontStyles.Bold);
            bossHealthText.rectTransform.anchorMin = new Vector2(0f, 1f);
            bossHealthText.rectTransform.anchorMax = new Vector2(1f, 1f);
            bossHealthText.rectTransform.pivot = new Vector2(0.5f, 1f);
            bossHealthText.rectTransform.anchoredPosition = new Vector2(0f, -4f);
            bossHealthText.rectTransform.sizeDelta = new Vector2(-16f, 20f);
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
        UITheme.StylePanel(panelImage, UITheme.Panel, UITheme.Border);
        UITheme.AddTopAccent(panel, UITheme.Cyan, 3f);

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
        waveText.color = UITheme.Text;
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
            image.color = new Color(0.08f, 0.42f, 0.61f, 1f);
        }

        UITheme.StyleButton(startWaveButton);

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
        UITheme.StyleText(label);
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    static Button CreateOverlayButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        Button button = obj.GetComponent<Button>();
        UITheme.StyleButton(button);
        button.onClick.AddListener(onClick);

        TextMeshProUGUI text = CreateOverlayText("Text", rect, label, 18f, FontStyles.Bold);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
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
