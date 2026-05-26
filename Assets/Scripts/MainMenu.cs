using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    const string CurrentStageKey = "CurrentStage";
    const string SelectedTowerKey = "SelectedTower";
    const string CannonTowerId = "cannon";

    Canvas canvas;
    RectTransform contentRoot;
    TextMeshProUGUI homeTabLabel;
    TextMeshProUGUI towersTabLabel;

    void Start()
    {
        Time.timeScale = 1f;
        if (!PlayerPrefs.HasKey(CurrentStageKey))
            PlayerPrefs.SetInt(CurrentStageKey, 1);
        if (!PlayerPrefs.HasKey(SelectedTowerKey))
            PlayerPrefs.SetString(SelectedTowerKey, CannonTowerId);

        BuildMenu();
        ShowHome();
    }

    public void StartGame()
    {
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

        RectTransform root = CreatePanel("MainMenuRoot", canvas.transform, new Color(0.035f, 0.04f, 0.052f, 1f));
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        TextMeshProUGUI title = CreateText("Title", root, "THE TOWER", 42f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -30f);
        titleRect.sizeDelta = new Vector2(0f, 58f);

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

        contentRoot = CreatePanel("Content", root, new Color(0.075f, 0.084f, 0.105f, 0.98f));
        contentRoot.anchorMin = new Vector2(0.5f, 0.5f);
        contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        contentRoot.pivot = new Vector2(0.5f, 0.5f);
        contentRoot.anchoredPosition = new Vector2(0f, -12f);
        contentRoot.sizeDelta = new Vector2(640f, 350f);

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
        SetTabState(true);
        ClearContent();

        int stage = Mathf.Max(1, PlayerPrefs.GetInt(CurrentStageKey, 1));
        TextMeshProUGUI heading = CreateText("Heading", contentRoot, "HOME", 26f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(heading.rectTransform, -34f, 36f);

        TextMeshProUGUI stageText = CreateText("Stage", contentRoot, $"STAGE {stage}", 42f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceCenter(stageText.rectTransform, 0f, 76f, 320f, 60f);

        TextMeshProUGUI detail = CreateText("Detail", contentRoot, "15 waves. Boss waves at 5, 10 and 15.", 17f, FontStyles.Normal, TextAlignmentOptions.Center);
        PlaceCenter(detail.rectTransform, 0f, 8f, 460f, 34f);

        Button playButton = CreateButton("PlayButton", contentRoot, "PLAY", StartGame);
        RectTransform playRect = playButton.GetComponent<RectTransform>();
        PlaceCenter(playRect, 0f, -104f, 240f, 58f);
    }

    void ShowTowers()
    {
        SetTabState(false);
        ClearContent();

        TextMeshProUGUI heading = CreateText("Heading", contentRoot, "TOWERS", 26f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(heading.rectTransform, -34f, 36f);

        RectTransform card = CreatePanel("CannonTowerCard", contentRoot, new Color(0.105f, 0.12f, 0.15f, 1f));
        PlaceCenter(card, 0f, -10f, 420f, 198f);

        TextMeshProUGUI name = CreateText("Name", card, "CANNON TOWER", 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        PlaceTop(name.rectTransform, -18f, 34f);

        TextMeshProUGUI description = CreateText("Description", card, "Single-target bullets with Fire Rate, Range and Damage upgrades.", 16f, FontStyles.Normal, TextAlignmentOptions.Center);
        PlaceCenter(description.rectTransform, 0f, 18f, 320f, 54f);

        Button equipButton = CreateButton("EquipButton", card, "EQUIPPED", () =>
        {
            PlayerPrefs.SetString(SelectedTowerKey, CannonTowerId);
            PlayerPrefs.Save();
            ShowTowers();
        });
        RectTransform equipRect = equipButton.GetComponent<RectTransform>();
        PlaceCenter(equipRect, 0f, -54f, 170f, 42f);
        equipButton.interactable = false;
    }

    void SetTabState(bool homeActive)
    {
        if (homeTabLabel != null)
            homeTabLabel.color = homeActive ? Color.white : new Color(0.68f, 0.73f, 0.8f, 1f);
        if (towersTabLabel != null)
            towersTabLabel.color = homeActive ? new Color(0.68f, 0.73f, 0.8f, 1f) : Color.white;
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
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.25f, 0.62f, 0.88f, 1f);
        colors.pressedColor = new Color(0.12f, 0.36f, 0.56f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.16f, 0.18f, 0.22f, 1f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
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
        obj.GetComponent<Image>().color = color;
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
        label.color = new Color(0.91f, 0.94f, 0.98f, 1f);
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
}
