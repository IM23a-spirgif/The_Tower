using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
    public enum UpgradeType
    {
        ReinforcedPowder = 0,
        ExpandedPayload = 1,
        AutoLoader = 2,
        HeavyCaliber = 3,
        ShockwaveShells = 4,
        ShrapnelRounds = 5,
        FragmentBurst = 6,
        BurningDebris = 7,
        ConcussiveImpact = 8,
        CraterMaker = 9,
        ChainDetonation = 10,
        SiegeShells = 11,
        ArmorPiercingCore = 12,
        DoubleCharge = 13,
        SiegePlatform = 14,
        ExecutionShell = 15,
        FinisherPayload = 16,
        StabilizedBarrel = 17,
        SmartTargeting = 18,
        RangefinderOptics = 19,
        DelayedFuse = 20,
        OverloadedChamber = 21,
        VolatileMunitions = 22,
        GlassCannonDesign = 23,
        ApocalypseRound = 24,
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
            case UpgradeType.ReinforcedPowder: return "REINFORCED POWDER";
            case UpgradeType.ExpandedPayload: return "EXPANDED PAYLOAD";
            case UpgradeType.AutoLoader: return "AUTO-LOADER";
            case UpgradeType.HeavyCaliber: return "HEAVY CALIBER";
            case UpgradeType.ShockwaveShells: return "SHOCKWAVE SHELLS";
            case UpgradeType.ShrapnelRounds: return "SHRAPNEL ROUNDS";
            case UpgradeType.FragmentBurst: return "FRAGMENT BURST";
            case UpgradeType.BurningDebris: return "BURNING DEBRIS";
            case UpgradeType.ConcussiveImpact: return "CONCUSSIVE IMPACT";
            case UpgradeType.CraterMaker: return "CRATER MAKER";
            case UpgradeType.ChainDetonation: return "CHAIN DETONATION";
            case UpgradeType.SiegeShells: return "SIEGE SHELLS";
            case UpgradeType.ArmorPiercingCore: return "ARMOR-PIERCING CORE";
            case UpgradeType.DoubleCharge: return "DOUBLE CHARGE";
            case UpgradeType.SiegePlatform: return "SIEGE PLATFORM";
            case UpgradeType.ExecutionShell: return "EXECUTION SHELL";
            case UpgradeType.FinisherPayload: return "FINISHER PAYLOAD";
            case UpgradeType.StabilizedBarrel: return "STABILIZED BARREL";
            case UpgradeType.SmartTargeting: return "SMART TARGETING";
            case UpgradeType.RangefinderOptics: return "RANGEFINDER OPTICS";
            case UpgradeType.DelayedFuse: return "DELAYED FUSE";
            case UpgradeType.OverloadedChamber: return "OVERLOADED CHAMBER";
            case UpgradeType.VolatileMunitions: return "VOLATILE MUNITIONS";
            case UpgradeType.GlassCannonDesign: return "GLASS CANNON DESIGN";
            case UpgradeType.ApocalypseRound: return "APOCALYPSE ROUND";
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
