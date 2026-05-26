using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public GameObject[] normalUpgradePrefabs;
    public GameObject[] specialUpgradePrefabs;

    [HideInInspector] public List<UpgradeCard.UpgradeType> unlockedUpgrades = new List<UpgradeCard.UpgradeType>();

    public Transform specialUpgradeSpawnParent;
    public float specialUpgradeSpacing = 2f;

    public Tower tower;
    public int bits = 5;
    public TextMeshProUGUI bitsText;

    private readonly Dictionary<UpgradeCard.UpgradeType, int> upgradeLevels = new Dictionary<UpgradeCard.UpgradeType, int>();
    private readonly Dictionary<UpgradeCard.UpgradeType, int> upgradeCosts = new Dictionary<UpgradeCard.UpgradeType, int>();
    private readonly Dictionary<UpgradeCard.UpgradeType, UpgradeCard> activeCards = new Dictionary<UpgradeCard.UpgradeType, UpgradeCard>();

    private RectTransform upgradeDock;
    private RectTransform upgradeCardRow;
    private GameObject specialOverlay;

    private static readonly UpgradeCard.UpgradeType[] NormalUpgradeTypes =
    {
        UpgradeCard.UpgradeType.FireRate,
        UpgradeCard.UpgradeType.Range,
        UpgradeCard.UpgradeType.Damage
    };

    private static readonly UpgradeCard.UpgradeType[] SpecialUpgradeTypes =
    {
        UpgradeCard.UpgradeType.MultiShot,
        UpgradeCard.UpgradeType.Homing,
        UpgradeCard.UpgradeType.Ricochet
    };

    void Start()
    {
        EnsureUpgradeDock();
        InitializeUpgrades();
        UpdateBitsText();
    }

    public void EnemyKilled()
    {
        bits++;
        UpdateBitsText();
        UpdateUpgradeCards();
    }

    void InitializeUpgrades()
    {
        foreach (UpgradeCard.UpgradeType type in Enum.GetValues(typeof(UpgradeCard.UpgradeType)))
        {
            if (!upgradeLevels.ContainsKey(type))
                upgradeLevels[type] = 0;
            if (!upgradeCosts.ContainsKey(type))
                upgradeCosts[type] = GetBaseCost(type);
        }

        foreach (UpgradeCard.UpgradeType type in NormalUpgradeTypes)
        {
            SpawnUpgradeCard(type, 0f);
        }
    }

    void SpawnUpgradeCard(UpgradeCard.UpgradeType upgradeType, float xPos)
    {
        if (activeCards.ContainsKey(upgradeType))
            return;

        EnsureUpgradeDock();

        int currentLevel = upgradeLevels[upgradeType] + 1;
        GameObject cardObject = CreateUpgradeCardObject(upgradeType, upgradeCardRow, false);
        UpgradeCard upgradeCard = cardObject.GetComponent<UpgradeCard>();
        upgradeCard.upgradeType = upgradeType;
        upgradeCard.upgradeLevel = currentLevel;
        upgradeCard.upgradeCost = upgradeCosts[upgradeType];
        upgradeCard.upgradeManager = this;
        upgradeCard.UpdateUpgradeUI();

        Button button = cardObject.GetComponent<Button>();
        button.onClick.AddListener(() => TryUpgrade(upgradeType));

        activeCards[upgradeType] = upgradeCard;
        UpdateCardAffordability(upgradeCard);
    }

    public int GetBaseCost(UpgradeCard.UpgradeType type)
    {
        switch (type)
        {
            case UpgradeCard.UpgradeType.FireRate: return 3;
            case UpgradeCard.UpgradeType.Range: return 2;
            case UpgradeCard.UpgradeType.Damage: return 5;
            default: return 1;
        }
    }

    public void TryUpgrade(UpgradeCard.UpgradeType type)
    {
        if (!upgradeCosts.ContainsKey(type) || bits < upgradeCosts[type])
            return;

        bits -= upgradeCosts[type];
        upgradeLevels[type]++;
        upgradeCosts[type] += Mathf.Max(1, Mathf.RoundToInt(upgradeCosts[type] * 0.5f));
        ApplyUpgrade(type);
        UpdateBitsText();
    }

    void ApplyUpgrade(UpgradeCard.UpgradeType type)
    {
        switch (type)
        {
            case UpgradeCard.UpgradeType.FireRate:
                tower.fireRate *= 0.9f;
                break;
            case UpgradeCard.UpgradeType.Range:
                tower.attackRange += 2.5f;
                break;
            case UpgradeCard.UpgradeType.Damage:
                tower.damage += 1;
                break;
            case UpgradeCard.UpgradeType.Ricochet:
                tower.ricochetEnabled = true;
                break;
            case UpgradeCard.UpgradeType.Homing:
                tower.homingEnabled = true;
                break;
            case UpgradeCard.UpgradeType.MultiShot:
                tower.extraBullets += 1;
                break;
        }

        UpdateUpgradeCards();
    }

    void UpdateUpgradeCards()
    {
        foreach (UpgradeCard card in activeCards.Values)
        {
            card.upgradeLevel = upgradeLevels[card.upgradeType] + 1;
            card.upgradeCost = upgradeCosts[card.upgradeType];
            card.UpdateUpgradeUI();
            UpdateCardAffordability(card);
        }
    }

    void UpdateBitsText()
    {
        if (bitsText == null)
            return;

        StyleBitsText();
        bitsText.text = "BITS " + bits;
    }

    void StyleBitsText()
    {
        RectTransform rect = bitsText.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -24f);
            rect.sizeDelta = new Vector2(150f, 42f);
        }

        bitsText.alignment = TextAlignmentOptions.Center;
        bitsText.fontSize = 21f;
        bitsText.fontStyle = FontStyles.Bold;
        bitsText.color = new Color(0.98f, 0.87f, 0.32f, 1f);
        bitsText.raycastTarget = false;

        Outline outline = bitsText.GetComponent<Outline>();
        if (outline == null)
            outline = bitsText.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.55f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }

    public void ShowSpecialUpgradeChoice()
    {
        EnsureUpgradeDock();

        List<UpgradeCard.UpgradeType> available = new List<UpgradeCard.UpgradeType>();
        foreach (UpgradeCard.UpgradeType type in SpecialUpgradeTypes)
        {
            if (!unlockedUpgrades.Contains(type))
                available.Add(type);
        }

        if (available.Count == 0)
            return;

        for (int i = 0; i < available.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, available.Count);
            UpgradeCard.UpgradeType temp = available[i];
            available[i] = available[rand];
            available[rand] = temp;
        }

        Time.timeScale = 0f;
        specialOverlay = CreateOverlay();
        RectTransform row = CreateRect("SpecialUpgradeRow", specialOverlay.transform);
        row.anchorMin = new Vector2(0.5f, 0.5f);
        row.anchorMax = new Vector2(0.5f, 0.5f);
        row.pivot = new Vector2(0.5f, 0.5f);
        row.sizeDelta = new Vector2(590f, 190f);

        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        int numToShow = Mathf.Min(3, available.Count);
        for (int i = 0; i < numToShow; i++)
        {
            UpgradeCard.UpgradeType type = available[i];
            GameObject cardObject = CreateUpgradeCardObject(type, row, true);
            UpgradeCard card = cardObject.GetComponent<UpgradeCard>();
            card.upgradeType = type;
            card.upgradeLevel = 1;
            card.upgradeCost = 0;
            card.upgradeManager = this;
            card.UpdateUpgradeUI();
            card.upgradeCostText.text = "UNLOCK";

            Button button = cardObject.GetComponent<Button>();
            button.onClick.AddListener(() => SelectSpecialUpgrade(type));
        }
    }

    public void UnlockSpecialUpgrade(UpgradeCard.UpgradeType type)
    {
        if (!upgradeLevels.ContainsKey(type))
        {
            upgradeLevels[type] = 0;
            upgradeCosts[type] = GetBaseCost(type);
        }

        if (!unlockedUpgrades.Contains(type))
            unlockedUpgrades.Add(type);

        SpawnUpgradeCard(type, 0f);
    }

    void SelectSpecialUpgrade(UpgradeCard.UpgradeType type)
    {
        UnlockSpecialUpgrade(type);

        if (specialOverlay != null)
            Destroy(specialOverlay);

        specialOverlay = null;
        Time.timeScale = 1f;
    }

    void EnsureUpgradeDock()
    {
        if (upgradeDock != null)
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

        Transform existingDock = canvas.transform.Find("UpgradeDock");
        upgradeDock = existingDock != null ? existingDock.GetComponent<RectTransform>() : CreateRect("UpgradeDock", canvas.transform);
        upgradeDock.anchorMin = new Vector2(0f, 0f);
        upgradeDock.anchorMax = new Vector2(1f, 0f);
        upgradeDock.pivot = new Vector2(0.5f, 0f);
        upgradeDock.anchoredPosition = Vector2.zero;
        upgradeDock.sizeDelta = new Vector2(0f, 150f);
        upgradeDock.SetAsLastSibling();

        Image dockImage = upgradeDock.GetComponent<Image>();
        if (dockImage == null)
            dockImage = upgradeDock.gameObject.AddComponent<Image>();
        dockImage.color = new Color(0.03f, 0.035f, 0.045f, 0.94f);
        dockImage.raycastTarget = true;

        Transform existingRow = upgradeDock.Find("Cards");
        upgradeCardRow = existingRow != null ? existingRow.GetComponent<RectTransform>() : CreateRect("Cards", upgradeDock);
        upgradeCardRow.anchorMin = new Vector2(0.5f, 0.5f);
        upgradeCardRow.anchorMax = new Vector2(0.5f, 0.5f);
        upgradeCardRow.pivot = new Vector2(0.5f, 0.5f);
        upgradeCardRow.anchoredPosition = new Vector2(0f, 6f);
        upgradeCardRow.sizeDelta = new Vector2(720f, 126f);

        HorizontalLayoutGroup layout = upgradeCardRow.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
            layout = upgradeCardRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    GameObject CreateUpgradeCardObject(UpgradeCard.UpgradeType type, Transform parent, bool specialChoice)
    {
        GameObject cardObject = new GameObject(type + "Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UpgradeCard));
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.SetParent(parent, false);

        LayoutElement layout = cardObject.AddComponent<LayoutElement>();
        layout.preferredWidth = specialChoice ? 180f : 150f;
        layout.preferredHeight = specialChoice ? 180f : 116f;
        layout.minWidth = layout.preferredWidth;
        layout.minHeight = layout.preferredHeight;

        Image image = cardObject.GetComponent<Image>();
        image.color = specialChoice ? new Color(0.11f, 0.12f, 0.16f, 0.98f) : new Color(0.1f, 0.11f, 0.14f, 0.98f);

        Button button = cardObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.16f, 0.19f, 0.25f, 1f);
        colors.pressedColor = new Color(0.06f, 0.28f, 0.36f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.08f, 0.08f, 0.09f, 0.78f);
        colors.colorMultiplier = 1f;
        button.colors = colors;

        UpgradeCard card = cardObject.GetComponent<UpgradeCard>();
        card.upgradeText = CreateText("UpgradeText", cardRect, GetDisplayName(type), specialChoice ? 20f : 17f, FontStyles.Bold, TextAlignmentOptions.Center);
        card.upgradeDescription = CreateText("UpgradeDescription", cardRect, GetDescription(type), specialChoice ? 13f : 11f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        card.upgradeCostText = CreateText("UpgradeCost", cardRect, "", specialChoice ? 12f : 11f, FontStyles.Bold, TextAlignmentOptions.Center);

        PlaceCardText(card.upgradeText.rectTransform, 10f, -8f, -10f, 28f, true);
        PlaceCardText(card.upgradeDescription.rectTransform, 12f, -40f, -12f, 44f, true);
        PlaceCardText(card.upgradeCostText.rectTransform, 10f, 8f, -10f, 22f, false);

        return cardObject;
    }

    GameObject CreateOverlay()
    {
        Canvas canvas = upgradeDock.GetComponentInParent<Canvas>();
        GameObject overlay = new GameObject("SpecialUpgradeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        Image image = overlay.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.74f);
        image.raycastTarget = true;

        TextMeshProUGUI title = CreateText("Title", rect, "CHOOSE UPGRADE", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 128f);
        titleRect.sizeDelta = new Vector2(420f, 42f);

        return overlay;
    }

    void UpdateCardAffordability(UpgradeCard card)
    {
        Button button = card.GetComponent<Button>();
        if (button != null)
            button.interactable = bits >= card.upgradeCost;

        if (card.upgradeCostText != null)
        {
            card.upgradeCostText.color = bits >= card.upgradeCost
                ? new Color(0.98f, 0.87f, 0.32f, 1f)
                : new Color(0.95f, 0.34f, 0.32f, 1f);
        }
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = new Color(0.9f, 0.93f, 0.98f, 1f);
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    static void PlaceCardText(RectTransform rect, float left, float top, float right, float height, bool fromTop)
    {
        rect.anchorMin = fromTop ? new Vector2(0f, 1f) : new Vector2(0f, 0f);
        rect.anchorMax = fromTop ? new Vector2(1f, 1f) : new Vector2(1f, 0f);
        rect.pivot = fromTop ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(left, fromTop ? -height : top);
        rect.offsetMax = new Vector2(right, fromTop ? top : top + height);
    }

    static string GetDisplayName(UpgradeCard.UpgradeType type)
    {
        switch (type)
        {
            case UpgradeCard.UpgradeType.FireRate: return "FIRE RATE";
            case UpgradeCard.UpgradeType.Range: return "RANGE";
            case UpgradeCard.UpgradeType.Damage: return "DAMAGE";
            case UpgradeCard.UpgradeType.MultiShot: return "MULTI SHOT";
            case UpgradeCard.UpgradeType.Homing: return "HOMING";
            case UpgradeCard.UpgradeType.Ricochet: return "RICOCHET";
            default: return type.ToString().ToUpperInvariant();
        }
    }

    static string GetDescription(UpgradeCard.UpgradeType type)
    {
        switch (type)
        {
            case UpgradeCard.UpgradeType.FireRate: return "Shoot 10% faster.";
            case UpgradeCard.UpgradeType.Range: return "Increase targeting range.";
            case UpgradeCard.UpgradeType.Damage: return "Add 1 damage per bullet.";
            case UpgradeCard.UpgradeType.MultiShot: return "Fire one extra bullet.";
            case UpgradeCard.UpgradeType.Homing: return "Bullets track targets.";
            case UpgradeCard.UpgradeType.Ricochet: return "Shots can bounce onward.";
            default: return "";
        }
    }
}
