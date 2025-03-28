using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public GameObject[] normalUpgradePrefabs; // FireRate, Range, Damage
    public GameObject[] specialUpgradePrefabs; // MultiShot, Homing, Ricochet, etc.

    [HideInInspector] public List<UpgradeCard.UpgradeType> unlockedUpgrades = new List<UpgradeCard.UpgradeType>();
// Stores which special upgrades have been permanently unlocked.

// UI placeholder to spawn the 3 choices
    public Transform specialUpgradeSpawnParent; // A center-of-screen parent or coordinates
    public float specialUpgradeSpacing = 2f;

    public Tower tower;
    private Dictionary<UpgradeCard.UpgradeType, int> upgradeLevels = new Dictionary<UpgradeCard.UpgradeType, int>();
    private Dictionary<UpgradeCard.UpgradeType, int> upgradeCosts = new Dictionary<UpgradeCard.UpgradeType, int>();
    public int bits = 5;
    public TextMeshProUGUI bitsText;

    void Start()
    {
        InitializeUpgrades();
        UpdateBitsText();
        Debug.Log($"specialUpgradePrefabs length: {specialUpgradePrefabs.Length}");
    }

    public void EnemyKilled()
    {
        bits++;
        UpdateBitsText();
    }

    void InitializeUpgrades()
    {
        float spacing = 2.5f;
        float startX = -((normalUpgradePrefabs.Length - 1) * spacing / 2f);
        foreach (UpgradeCard.UpgradeType type in System.Enum.GetValues(typeof(UpgradeCard.UpgradeType)))
        {
            if (!upgradeLevels.ContainsKey(type))
                upgradeLevels[type] = 0;
            if (!upgradeCosts.ContainsKey(type))
                upgradeCosts[type] = GetBaseCost(type);
        }

        for (int i = 0; i < normalUpgradePrefabs.Length; i++)
        {
            UpgradeCard.UpgradeType upgradeType = (UpgradeCard.UpgradeType)i;
            SpawnUpgradeCard(upgradeType, startX + (i * spacing));
        }
    }

    void SpawnUpgradeCard(UpgradeCard.UpgradeType upgradeType, float xPos)
    {
        Vector3 spawnPosition = new Vector3(xPos, -4f, 0);

        // Ensure dictionaries have entries for this type
        if (!upgradeLevels.ContainsKey(upgradeType))
            upgradeLevels[upgradeType] = 0;
        if (!upgradeCosts.ContainsKey(upgradeType))
            upgradeCosts[upgradeType] = GetBaseCost(upgradeType);

        // Determine this upgrade's current level
        int currentLevel = upgradeLevels[upgradeType] + 1;

        // 1) Choose which prefab array to use
        GameObject prefab;
        if ((int)upgradeType < normalUpgradePrefabs.Length)
        {
            // It's one of FireRate=0, Range=1, Damage=2
            prefab = normalUpgradePrefabs[(int)upgradeType];
        }
        else
        {
            // It's a special upgrade, e.g. MultiShot, Homing, Ricochet
            int specialIndex = (int)upgradeType - normalUpgradePrefabs.Length;
            // e.g. MultiShot=3 => specialIndex=0
            // Homing=4 => specialIndex=1, etc.
            prefab = specialUpgradePrefabs[specialIndex];
        }

        // 2) Instantiate the correct prefab
        GameObject upgradeCardObj = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // 3) Set up the UpgradeCard component
        UpgradeCard upgradeCard = upgradeCardObj.GetComponentInChildren<UpgradeCard>();
        if (upgradeCard != null)
        {
            upgradeCard.upgradeType = upgradeType;
            upgradeCard.upgradeLevel = currentLevel;
            upgradeCard.upgradeCost = upgradeCosts[upgradeType];
            upgradeCard.upgradeManager = this;
            upgradeCard.upgradeText = upgradeCardObj.GetComponentInChildren<TextMeshProUGUI>(true);
            upgradeCard.UpdateUpgradeUI();
        }
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
        if (bits >= upgradeCosts[type])
        {
            bits -= upgradeCosts[type];
            upgradeLevels[type]++;
            upgradeCosts[type] += (int)(upgradeCosts[type] * 0.5f);
            ApplyUpgrade(type);
            UpdateBitsText();
        }
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
        UpgradeCard[] allCards = FindObjectsOfType<UpgradeCard>();
        foreach (UpgradeCard card in allCards)
        {
            card.upgradeLevel = upgradeLevels[card.upgradeType] + 1;
            card.upgradeCost = upgradeCosts[card.upgradeType];
            card.UpdateUpgradeUI();
        }
    }

    void UpdateBitsText()
    {
        if (bitsText != null)
        {
            bitsText.text = "Bits: " + bits;
        }
    }

    public void ShowSpecialUpgradeChoice()
    {
        Debug.Log(specialUpgradeSpawnParent.name);
        Debug.Log("Showing Upgrades...");
        List<GameObject> available = new List<GameObject>(specialUpgradePrefabs);

        // Shuffle the list to randomize order (Fisher-Yates)
        for (int i = 0; i < available.Count; i++)
        {
            int rand = Random.Range(i, available.Count);
            var temp = available[i];
            available[i] = available[rand];
            available[rand] = temp;
        }

        // Determine how many upgrades we can actually show
        int numToShow = Mathf.Min(3, available.Count);

        // If there are no upgrades left, resume game and return
        if (numToShow == 0)
        {
            Debug.Log("No special upgrades available!");
            Time.timeScale = 1f;
            return;
        }

        // Instantiate the first 'numToShow' from the shuffled list
        float startX = -specialUpgradeSpacing;
        for (int i = 0; i < numToShow; i++)
        {
            float xPos = startX + i * specialUpgradeSpacing;
            Vector3 spawnPos = specialUpgradeSpawnParent.position + new Vector3(xPos, 0, 0);

            GameObject specialCardObj =
                Instantiate(available[i], spawnPos, Quaternion.identity, specialUpgradeSpawnParent);
                Debug.Log("Spawned special card: " + specialCardObj, specialCardObj);
            // Add your click-to-choose script
            SpecialChoiceCard choiceCard = specialCardObj.AddComponent<SpecialChoiceCard>();
            choiceCard.upgradeManager = this;
        }
    }

    public void UnlockSpecialUpgrade(UpgradeCard.UpgradeType type)
    {
        Debug.Log("Card chosen! Applying upgrade...");
        // 1) Add this special upgrade type to the dictionary if not present
        if (!upgradeLevels.ContainsKey(type))
        {
            upgradeLevels[type] = 0;
            upgradeCosts[type] = GetBaseCost(type);
        }

        // 2) Add to the “unlockedUpgrades” list if you want to track them
        if (!unlockedUpgrades.Contains(type))
            unlockedUpgrades.Add(type);

        // 3) Instantiate it at the bottom row with the other upgrades
        float newXPos = CalculateNextUpgradeXPos(); // A method to place new card to the right
        SpawnUpgradeCard(type, newXPos);
    }

// Example method to find a new X position for the newly unlocked card
    float CalculateNextUpgradeXPos()
    {
        // If you originally placed normal upgrades from -X to +X,
        // find how many total upgrades are unlocked, and space them out.
        // For simplicity:
        int totalUnlocked = FindObjectsOfType<UpgradeCard>().Length;
        float spacing = 2.5f;
        return -((totalUnlocked - 1) * spacing / 2f) + (totalUnlocked - 1) * spacing;
    }
}