using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
    public enum UpgradeType
    {
        FireRate = 0,
        Range    = 1,
        Damage   = 2,
        MultiShot= 3,
        Homing   = 4,
        Ricochet  = 5,
    }



    public UpgradeType upgradeType;
    public int upgradeLevel = 1;
    public int upgradeCost;
    public UpgradeManager upgradeManager;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI upgradeDescription; 
    public TextMeshProUGUI upgradeCostText;
    [SerializeField] private float titleFontSize = 17f;
    [SerializeField] private float detailFontSize = 11f;
    [SerializeField] private float worldCanvasDynamicPixelsPerUnit = 64f;

    void Start()
    {
        // If not assigned in Inspector, try to find them by name
        if (upgradeText == null)
            upgradeText = FindText("Canvas/UpgradeText");
        if (upgradeDescription == null)
            upgradeDescription = FindText("Canvas/UpgradeDescription");
        if (upgradeCostText == null)
            upgradeCostText = FindText("Canvas/UpgradeCost");

        ImproveTextRendering();
        // Update all UI fields
        UpdateUpgradeUI();
    }

    void OnMouseDown()
    {
        // When clicked, attempt to buy/upgrade
        upgradeManager.TryUpgrade(upgradeType);
    }
    
    public void UpdateUpgradeUI()
    {
        ImproveTextRendering();

        if (upgradeText != null)
        {
            string roman = ConvertToRoman(upgradeLevel);
            upgradeText.text = $"{GetDisplayName(upgradeType)} {roman}";
        }

        // 2) Description is presumably already set in the prefab, 
        //    but if you want to set it dynamically, you can do so here.
        //    e.g.: upgradeDescription.text = "This does X, Y, Z..."

        // 3) Cost text, e.g. "Cost: 5 Bits"
        if (upgradeCostText != null)
        {
            upgradeCostText.text = $"Cost: {upgradeCost} Bits";
        }
    }

    string ConvertToRoman(int number)
    {
        string[] numerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
        return number < 1 || number > 10 ? number.ToString() : numerals[number - 1];
    }

    static string GetDisplayName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.FireRate: return "FIRE RATE";
            case UpgradeType.Range: return "RANGE";
            case UpgradeType.Damage: return "DAMAGE";
            case UpgradeType.MultiShot: return "MULTI SHOT";
            case UpgradeType.Homing: return "HOMING";
            case UpgradeType.Ricochet: return "RICOCHET";
            default: return type.ToString().ToUpperInvariant();
        }
    }

    TextMeshProUGUI FindText(string path)
    {
        Transform textTransform = transform.Find(path);
        return textTransform != null ? textTransform.GetComponent<TextMeshProUGUI>() : null;
    }

    void ImproveTextRendering()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            canvas.pixelPerfect = true;
        }

        CanvasScaler canvasScaler = GetComponentInChildren<CanvasScaler>(true);
        if (canvasScaler != null)
        {
            canvasScaler.dynamicPixelsPerUnit = worldCanvasDynamicPixelsPerUnit;
        }

        ConfigureText(upgradeText, titleFontSize);
        ConfigureText(upgradeDescription, detailFontSize);
        ConfigureText(upgradeCostText, detailFontSize);
    }

    static void ConfigureText(TextMeshProUGUI text, float fontSize)
    {
        if (text == null)
            return;

        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        text.extraPadding = true;
        text.isTextObjectScaleStatic = false;
    }
}
