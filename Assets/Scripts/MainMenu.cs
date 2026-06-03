using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    const string CurrentStageKey = "CurrentStage";
    const string SelectedTowerKey = "SelectedTower";
    const string TeslaTowerUnlockedKey = "TeslaTowerUnlocked";
    const string CannonTowerId = "cannon";
    const string TeslaTowerId = "tesla";

    Canvas canvas;
    RectTransform contentRoot;
    TextMeshProUGUI homeTabLabel;
    TextMeshProUGUI towersTabLabel;
    TextMeshProUGUI deckTabLabel;

    void Start()
    {
        Time.timeScale = 1f;
        if (!PlayerPrefs.HasKey(CurrentStageKey))
            PlayerPrefs.SetInt(CurrentStageKey, 1);
        if (!PlayerPrefs.HasKey(SelectedTowerKey))
            PlayerPrefs.SetString(SelectedTowerKey, CannonTowerId);
        if (PlayerPrefs.GetInt(CurrentStageKey, 1) > 1)
            PlayerPrefs.SetInt(TeslaTowerUnlockedKey, 1);
        if (!IsTowerUnlocked(PlayerPrefs.GetString(SelectedTowerKey, CannonTowerId)))
            PlayerPrefs.SetString(SelectedTowerKey, CannonTowerId);

        BuildMenu();
        ShowHome();
    }

    public void StartGame()
    {
        if (!IsTowerUnlocked(PlayerPrefs.GetString(SelectedTowerKey, CannonTowerId)))
            PlayerPrefs.SetString(SelectedTowerKey, CannonTowerId);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!");
        Application.Quit();
    }

    void BuildMenu()
    {
        Canvas[] existingCanvases = FindObjectsByType<Canvas>();
        canvas = null;
        foreach (Canvas existingCanvas in existingCanvases)
        {
            if (existingCanvas.GetComponent<MainMenu>() != null)
            {
                canvas = existingCanvas;
                continue;
            }

            Destroy(existingCanvas.gameObject);
        }

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(900f, 600f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            Destroy(canvas.transform.GetChild(i).gameObject);

        UITheme.StyleCanvas(canvas, new Vector2(900f, 600f));
        RectTransform root = CreatePanel("MainMenuRoot", canvas.transform, UITheme.Background);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        UITheme.AddAccent(root, "TopGlow", new Color(0.12f, 0.54f, 0.76f, 0.9f), new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -4f), Vector2.zero);
        UITheme.AddAccent(root, "LeftRail", new Color(0.08f, 0.3f, 0.44f, 0.55f), Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(4f, 0f));
        UITheme.AddAccent(root, "RightRail", new Color(0.08f, 0.3f, 0.44f, 0.55f), new Vector2(1f, 0f), Vector2.one, new Vector2(-4f, 0f), Vector2.zero);

        TextMeshProUGUI title = CreateText("Title", root, "THE TOWER", 46f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -30f);
        titleRect.sizeDelta = new Vector2(0f, 58f);
        UITheme.StyleText(title, UITheme.Text, true);

        TextMeshProUGUI subtitle = CreateText("Subtitle", root, "Best Tower Defense Game ever 100%", 13f, FontStyles.Bold, TextAlignmentOptions.Center);
        subtitle.color = UITheme.Cyan;
        RectTransform subtitleRect = subtitle.rectTransform;
        subtitleRect.anchorMin = new Vector2(0f, 1f);
        subtitleRect.anchorMax = new Vector2(1f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0f, -82f);
        subtitleRect.sizeDelta = new Vector2(0f, 24f);

        RectTransform nav = CreateRect("Nav", root);
        nav.anchorMin = new Vector2(0f, 0f);
        nav.anchorMax = new Vector2(1f, 0f);
        nav.pivot = new Vector2(0.5f, 0f);
        nav.anchoredPosition = Vector2.zero;
        nav.sizeDelta = new Vector2(0f, 58f);
        HorizontalLayoutGroup navLayout = nav.gameObject.AddComponent<HorizontalLayoutGroup>();
        navLayout.spacing = 0f;
        navLayout.childAlignment = TextAnchor.MiddleCenter;
        navLayout.childControlWidth = true;
        navLayout.childControlHeight = true;
        navLayout.childForceExpandWidth = true;
        navLayout.childForceExpandHeight = true;

        Button homeButton = CreateButton("HomeButton", nav, "HOME", () => ShowHome());
        homeTabLabel = homeButton.GetComponentInChildren<TextMeshProUGUI>();
        Button towersButton = CreateButton("TowersButton", nav, "TOWERS", () => ShowTowers());
        towersTabLabel = towersButton.GetComponentInChildren<TextMeshProUGUI>();
        Button deckButton = CreateButton("DeckButton", nav, "DECK", () => ShowDeck());
        deckTabLabel = deckButton.GetComponentInChildren<TextMeshProUGUI>();

        contentRoot = CreatePanel("Content", root, UITheme.Panel);
        contentRoot.anchorMin = new Vector2(0.5f, 0.5f);
        contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        contentRoot.pivot = new Vector2(0.5f, 0.5f);
        contentRoot.anchoredPosition = new Vector2(0f, -12f);
        contentRoot.sizeDelta = new Vector2(640f, 350f);
        UITheme.AddTopAccent(contentRoot, UITheme.Cyan);

        Button quitButton = CreateButton("QuitButton", root, "QUIT", QuitGame);
        RectTransform quitRect = quitButton.GetComponent<RectTransform>();
        quitRect.anchorMin = new Vector2(1f, 1f);
        quitRect.anchorMax = new Vector2(1f, 1f);
        quitRect.pivot = new Vector2(1f, 1f);
        quitRect.anchoredPosition = new Vector2(-20f, -20f);
        quitRect.sizeDelta = new Vector2(112f, 40f);
    }

    void ShowHome()
    {
        SetTabState(homeTabLabel);
        ClearContent();

        int stage = Mathf.Max(1, PlayerPrefs.GetInt(CurrentStageKey, 1));
        TextMeshProUGUI heading = CreateText("Heading", contentRoot, "MISSION CONTROL", 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(heading.rectTransform, -34f, 36f);
        heading.color = UITheme.CyanBright;

        TextMeshProUGUI stageText = CreateText("Stage", contentRoot, $"STAGE {stage}", 42f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceCenter(stageText.rectTransform, 0f, 76f, 320f, 60f);

        TextMeshProUGUI detail = CreateText("Detail", contentRoot, "15 WAVES  //  COMMANDERS AT 05, 10, 15", 15f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceCenter(detail.rectTransform, 0f, 8f, 460f, 34f);
        detail.color = UITheme.TextMuted;

        Button playButton = CreateButton("PlayButton", contentRoot, "PLAY", StartGame);
        RectTransform playRect = playButton.GetComponent<RectTransform>();
        PlaceCenter(playRect, 0f, -104f, 240f, 58f);
    }

    void ShowTowers()
    {
        SetTabState(towersTabLabel);
        ClearContent();

        TextMeshProUGUI heading = CreateText("Heading", contentRoot, "TOWERS", 26f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(heading.rectTransform, -34f, 36f);
        heading.color = UITheme.CyanBright;

        CreateTowerCard("CannonTowerCard", "CANNON TOWER", "Explosive shells with splash and heavy upgrade scaling.", CannonTowerId, true, -216f);
        CreateTowerCard("TeslaTowerCard", "TESLA TOWER", "Rapid arcs with longer range and steadier damage.", TeslaTowerId, IsTowerUnlocked(TeslaTowerId), 216f);
    }

    void ShowDeck()
    {
        SetTabState(deckTabLabel);
        ClearContent();

        string selectedTower = PlayerPrefs.GetString(SelectedTowerKey, CannonTowerId);
        string towerName = selectedTower == TeslaTowerId ? "TESLA TOWER" : "CANNON TOWER";
        TextMeshProUGUI heading = CreateText("Heading", contentRoot, $"{towerName} DECK", 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(heading.rectTransform, -22f, 32f);
        heading.color = UITheme.CyanBright;

        TextMeshProUGUI detail = CreateText("Detail", contentRoot, "CURRENTLY EQUIPPED TOWER  //  ALL CARDS", 12f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(detail.rectTransform, -52f, 20f);
        detail.color = UITheme.TextMuted;

        RectTransform scrollView = CreateRect("DeckScrollView", contentRoot);
        PlaceCenter(scrollView, 0f, -38f, 604f, 248f);
        ScrollRect scrollRect = scrollView.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 22f;

        RectTransform viewport = CreatePanel("Viewport", scrollView, new Color(0.025f, 0.04f, 0.06f, 0.72f));
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        RectTransform grid = CreateRect("Cards", viewport);
        grid.anchorMin = new Vector2(0f, 1f);
        grid.anchorMax = new Vector2(1f, 1f);
        grid.pivot = new Vector2(0.5f, 1f);
        grid.anchoredPosition = Vector2.zero;
        grid.sizeDelta = Vector2.zero;
        GridLayoutGroup layout = grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.cellSize = new Vector2(184f, 94f);
        layout.spacing = new Vector2(12f, 12f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        ContentSizeFitter fitter = grid.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = grid;

        foreach (UpgradeManager.CardDefinition card in UpgradeManager.GetCardsForTower(selectedTower))
            CreateDeckCard(grid, card);
    }

    void CreateDeckCard(Transform parent, UpgradeManager.CardDefinition definition)
    {
        RectTransform card = CreatePanel(definition.title + "Card", parent, UITheme.PanelRaised);
        UITheme.AddTopAccent(card, GetRarityColor(definition.rarity), 3f);
        Button button = card.gameObject.AddComponent<Button>();
        button.targetGraphic = card.GetComponent<Image>();
        ColorBlock colors = button.colors;
        colors.normalColor = UITheme.PanelRaised;
        colors.highlightedColor = new Color(0.12f, 0.2f, 0.27f, 1f);
        colors.pressedColor = new Color(0.06f, 0.14f, 0.2f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.colorMultiplier = 1f;
        button.colors = colors;
        button.onClick.AddListener(() => ShowDeckCardDetails(definition));

        TextMeshProUGUI rarity = CreateText("Rarity", card, definition.rarity.ToUpperInvariant(), 10f, FontStyles.Bold, TextAlignmentOptions.Left);
        PlaceDeckText(rarity.rectTransform, 10f, -8f, -10f, 14f);
        rarity.color = GetRarityColor(definition.rarity);

        TextMeshProUGUI title = CreateText("Title", card, definition.title, 14f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        PlaceDeckText(title.rectTransform, 10f, -28f, -10f, 36f);

        TextMeshProUGUI level = CreateText("Level", card, "LEVEL 1", 11f, FontStyles.Bold, TextAlignmentOptions.Left);
        PlaceDeckText(level.rectTransform, 10f, -70f, -10f, 16f);
        level.color = UITheme.Amber;
    }

    void ShowDeckCardDetails(UpgradeManager.CardDefinition definition)
    {
        SetTabState(deckTabLabel);
        ClearContent();

        TextMeshProUGUI heading = CreateText("Heading", contentRoot, definition.title, 25f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(heading.rectTransform, -30f, 36f);
        heading.color = UITheme.CyanBright;

        TextMeshProUGUI rarity = CreateText("Rarity", contentRoot, definition.rarity.ToUpperInvariant(), 13f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(rarity.rectTransform, -68f, 20f);
        rarity.color = GetRarityColor(definition.rarity);

        RectTransform detailPanel = CreatePanel("CardDetails", contentRoot, UITheme.PanelRaised);
        PlaceCenter(detailPanel, 0f, -20f, 540f, 168f);
        UITheme.AddTopAccent(detailPanel, GetRarityColor(definition.rarity), 4f);

        TextMeshProUGUI description = CreateText("Description", detailPanel, definition.description, 17f, FontStyles.Normal, TextAlignmentOptions.Center);
        PlaceCenter(description.rectTransform, 0f, 34f, 490f, 62f);

        TextMeshProUGUI level = CreateText("Level", detailPanel, "CURRENT LEVEL  //  1", 13f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceCenter(level.rectTransform, 0f, -32f, 240f, 22f);
        level.color = UITheme.Amber;

        TextMeshProUGUI limits = CreateText("Limits", detailPanel, $"MAX LEVEL  //  {definition.maxStacks}     STARTING COST  //  {definition.baseCost} BITS", 12f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceCenter(limits.rectTransform, 0f, -60f, 480f, 22f);
        limits.color = UITheme.TextMuted;

        Button backButton = CreateButton("BackButton", contentRoot, "BACK TO DECK", ShowDeck);
        PlaceCenter(backButton.GetComponent<RectTransform>(), 0f, -132f, 190f, 44f);
    }

    void CreateTowerCard(string cardName, string towerName, string descriptionText, string towerId, bool unlocked, float x)
    {
        string selectedTower = PlayerPrefs.GetString(SelectedTowerKey, CannonTowerId);
        bool selected = selectedTower == towerId && unlocked;
        Color cardColor = unlocked ? new Color(0.105f, 0.12f, 0.15f, 1f) : new Color(0.078f, 0.084f, 0.096f, 1f);

        RectTransform card = CreatePanel(cardName, contentRoot, cardColor);
        PlaceCenter(card, x, -10f, 196f, 198f);
        UITheme.AddTopAccent(card, selected ? UITheme.Cyan : unlocked ? UITheme.Border : new Color(0.22f, 0.24f, 0.27f, 1f));

        TextMeshProUGUI name = CreateText("Name", card, towerName, 20f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(name.rectTransform, -18f, 34f);
        name.color = unlocked ? new Color(0.91f, 0.94f, 0.98f, 1f) : new Color(0.58f, 0.62f, 0.68f, 1f);

        TextMeshProUGUI description = CreateText("Description", card, unlocked ? descriptionText : "Locked until you beat stage 1.", 14f, FontStyles.Normal, TextAlignmentOptions.Center);
        PlaceCenter(description.rectTransform, 0f, 18f, 160f, 64f);
        description.color = unlocked ? new Color(0.78f, 0.83f, 0.9f, 1f) : new Color(0.5f, 0.54f, 0.6f, 1f);

        Button equipButton = CreateButton("EquipButton", card, selected ? "EQUIPPED" : unlocked ? "EQUIP" : "LOCKED", () =>
        {
            if (!unlocked)
                return;

            PlayerPrefs.SetString(SelectedTowerKey, towerId);
            PlayerPrefs.Save();
            ShowTowers();
        });
        RectTransform equipRect = equipButton.GetComponent<RectTransform>();
        PlaceCenter(equipRect, 0f, -54f, 144f, 42f);
        equipButton.interactable = unlocked && !selected;
    }

    static bool IsTowerUnlocked(string towerId)
    {
        return towerId == CannonTowerId || (towerId == TeslaTowerId && PlayerPrefs.GetInt(TeslaTowerUnlockedKey, 0) == 1);
    }

    void SetTabState(TextMeshProUGUI activeLabel)
    {
        if (homeTabLabel != null)
            homeTabLabel.color = homeTabLabel == activeLabel ? Color.white : new Color(0.68f, 0.73f, 0.8f, 1f);
        if (towersTabLabel != null)
            towersTabLabel.color = towersTabLabel == activeLabel ? Color.white : new Color(0.68f, 0.73f, 0.8f, 1f);
        if (deckTabLabel != null)
            deckTabLabel.color = deckTabLabel == activeLabel ? Color.white : new Color(0.68f, 0.73f, 0.8f, 1f);
    }

    void ClearContent()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);
    }

    static Button CreateButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        Image image = obj.GetComponent<Image>();
        image.color = new Color(0.18f, 0.5f, 0.74f, 1f);

        Button button = obj.GetComponent<Button>();
        UITheme.StyleButton(button);
        button.onClick.AddListener(onClick);

        TextMeshProUGUI text = CreateText("Text", rect, label, 18f, FontStyles.Bold, TextAlignmentOptions.Center);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    static RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        UITheme.StylePanel(obj.GetComponent<Image>(), color);
        return rect;
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
        UITheme.StyleText(label);
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    static void PlaceTop(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(-40f, height);
    }

    static void PlaceCenter(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    static void PlaceDeckText(RectTransform rect, float left, float top, float right, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(left, top - height);
        rect.offsetMax = new Vector2(right, top);
    }

    static Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "Common": return new Color(0.62f, 0.64f, 0.66f, 0.85f);
            case "Uncommon": return new Color(0.22f, 0.9f, 0.34f, 0.9f);
            case "Rare": return new Color(0.22f, 0.48f, 1f, 0.92f);
            case "Epic": return new Color(0.68f, 0.25f, 1f, 0.95f);
            case "Legendary": return new Color(1f, 0.55f, 0.08f, 0.98f);
            default: return UITheme.Border;
        }
    }
}
