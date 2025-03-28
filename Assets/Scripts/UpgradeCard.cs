using TMPro;
using UnityEngine;

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

    void Start()
    {
        // If not assigned in Inspector, try to find them by name
        if (upgradeText == null)
            upgradeText = transform.Find("Canvas/UpgradeText").GetComponent<TextMeshProUGUI>();
        if (upgradeDescription == null)
            upgradeDescription = transform.Find("Canvas/UpgradeDescription").GetComponent<TextMeshProUGUI>();
        if (upgradeCostText == null)
            upgradeCostText = transform.Find("Canvas/UpgradeCost").GetComponent<TextMeshProUGUI>();

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
        if (upgradeText != null)
        {
            string roman = ConvertToRoman(upgradeLevel);
            upgradeText.text = $"{upgradeType} {roman}";
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
        return number > 10 ? number.ToString() : numerals[number - 1];
    }
}