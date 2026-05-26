using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TowerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public GameObject floatingTextPrefab;

    [Header("HUD")]
    public Image healthFill;
    public TextMeshProUGUI healthText;
    public Color highHealthColor = new Color(0.18f, 0.88f, 0.42f);
    public Color lowHealthColor = new Color(0.95f, 0.22f, 0.18f);

    private int currentHealth;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        EnsureHealthBar();
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        ShowFloatingDamage(damage);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Debug.Log("Tower Destroyed! Game Over.");
        }
    }

    void UpdateHealthBar()
    {
        float healthPercentage = maxHealth <= 0 ? 0f : (float)currentHealth / maxHealth;

        if (healthFill != null)
        {
            healthFill.fillAmount = healthPercentage;
            healthFill.color = Color.Lerp(lowHealthColor, highHealthColor, healthPercentage);
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    void EnsureHealthBar()
    {
        if (healthFill != null && healthText != null)
            return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("GameUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        Transform existing = canvas.transform.Find("TowerHealthBar");
        RectTransform container = existing != null
            ? existing.GetComponent<RectTransform>()
            : CreateRect("TowerHealthBar", canvas.transform);

        container.anchorMin = new Vector2(1f, 1f);
        container.anchorMax = new Vector2(1f, 1f);
        container.pivot = new Vector2(1f, 1f);
        container.anchoredPosition = new Vector2(-24f, -24f);
        container.sizeDelta = new Vector2(240f, 54f);

        Image panel = container.GetComponent<Image>();
        if (panel == null)
            panel = container.gameObject.AddComponent<Image>();
        panel.color = new Color(0.05f, 0.06f, 0.08f, 0.84f);

        TextMeshProUGUI label = container.Find("Label")?.GetComponent<TextMeshProUGUI>();
        if (label == null)
        {
            label = CreateText("Label", container, "TOWER", 14f);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 1f);
            labelRect.anchoredPosition = new Vector2(12f, -7f);
            labelRect.sizeDelta = new Vector2(90f, 18f);
        }

        if (healthText == null)
        {
            healthText = container.Find("Value")?.GetComponent<TextMeshProUGUI>();
            if (healthText == null)
            {
                healthText = CreateText("Value", container, "", 14f);
                healthText.alignment = TextAlignmentOptions.Right;
                RectTransform textRect = healthText.rectTransform;
                textRect.anchorMin = new Vector2(1f, 1f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.pivot = new Vector2(1f, 1f);
                textRect.anchoredPosition = new Vector2(-12f, -7f);
                textRect.sizeDelta = new Vector2(100f, 18f);
            }
        }

        RectTransform track = container.Find("Track")?.GetComponent<RectTransform>();
        if (track == null)
        {
            track = CreateRect("Track", container);
            track.anchorMin = new Vector2(0f, 0f);
            track.anchorMax = new Vector2(1f, 0f);
            track.pivot = new Vector2(0.5f, 0f);
            track.anchoredPosition = new Vector2(0f, 10f);
            track.sizeDelta = new Vector2(-24f, 16f);

            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = new Color(0.16f, 0.17f, 0.2f, 1f);
        }

        if (healthFill == null)
        {
            healthFill = track.Find("Fill")?.GetComponent<Image>();
            if (healthFill == null)
            {
                RectTransform fill = CreateRect("Fill", track);
                fill.anchorMin = Vector2.zero;
                fill.anchorMax = Vector2.one;
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;

                healthFill = fill.gameObject.AddComponent<Image>();
                healthFill.type = Image.Type.Filled;
                healthFill.fillMethod = Image.FillMethod.Horizontal;
                healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.color = new Color(0.92f, 0.95f, 1f, 1f);
        label.raycastTarget = false;
        return label;
    }

    void ShowFloatingDamage(int damage)
    {
        if (floatingTextPrefab != null)
        {
            GameObject damageTextInstance = Instantiate(
                floatingTextPrefab,
                transform.position + new Vector3(0, 1.5f, 0),
                Quaternion.identity
            );
            TextMeshPro tmp = damageTextInstance.GetComponent<TextMeshPro>();
            tmp.text = "-" + damage.ToString();
            StartCoroutine(FadeAndMoveText(damageTextInstance));
            Destroy(damageTextInstance, 1.5f);
        }
    }
    
    IEnumerator FadeAndMoveText(GameObject textObj)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = textObj.transform.position;
        Vector3 targetPos = startPos + new Vector3(0, 1f, 0); // Move up
        TextMeshPro tmp = textObj.GetComponent<TextMeshPro>();
        Color startColor = tmp.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            textObj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);
            yield return null;
        }
    }
}
