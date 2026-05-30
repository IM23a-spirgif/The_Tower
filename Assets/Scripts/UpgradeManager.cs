using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    const string SelectedTowerKey = "SelectedTower";
    const string TeslaTowerId = "tesla";

    enum FreeUpgradeType
    {
        KillBonus,
        HitBonus
    }

    struct CardDefinition
    {
        public UpgradeCard.UpgradeType type;
        public string title;
        public string description;
        public string rarity;
        public int maxStacks;
        public int baseCost;
    }

    struct UpgradeOffer
    {
        public UpgradeCard.UpgradeType paidType;
        public FreeUpgradeType freeType;
        public bool isFree;
        public string title;
        public string description;
        public string footer;
        public string rarity;
    }

    public Tower tower;
    public int bits = 5;
    public TextMeshProUGUI bitsText;

    readonly Dictionary<UpgradeCard.UpgradeType, int> upgradeLevels = new Dictionary<UpgradeCard.UpgradeType, int>();
    readonly Dictionary<UpgradeCard.UpgradeType, int> upgradeCosts = new Dictionary<UpgradeCard.UpgradeType, int>();

    GameObject choiceOverlay;
    RectTransform currentCardRow;
    bool currentGuaranteeEpicOrHigher;
    Action currentChoiceComplete;
    int bonusBitsPerKill;
    float hitBitChance;
    float salvageProgress;
    int rerollCost = 1;
    const float BaseBitsPerKill = 0.5f;
    const float SalvageBonusPerStack = 0.15f;

    static readonly CardDefinition[] CannonCards =
    {
        Card(UpgradeCard.UpgradeType.ReinforcedPowder, "REINFORCED POWDER", "Shell Damage +15%.", "Common", 8, 3),
        Card(UpgradeCard.UpgradeType.ExpandedPayload, "EXPANDED PAYLOAD", "Blast Radius +12%.", "Common", 6, 3),
        Card(UpgradeCard.UpgradeType.AutoLoader, "AUTO-LOADER", "Reload Speed +10%.", "Common", 8, 3),
        Card(UpgradeCard.UpgradeType.HeavyCaliber, "HEAVY CALIBER", "Shells gain +25% damage but Reload Speed -8%.", "Uncommon", 5, 5),
        Card(UpgradeCard.UpgradeType.ShockwaveShells, "SHOCKWAVE SHELLS", "Explosion knockback increased slightly.", "Uncommon", 4, 5),
        Card(UpgradeCard.UpgradeType.ShrapnelRounds, "SHRAPNEL ROUNDS", "Explosion edges deal shrapnel damage equal to 35% shell damage.", "Rare", 5, 7),
        Card(UpgradeCard.UpgradeType.FragmentBurst, "FRAGMENT BURST", "Explosions release 3 metal fragments.", "Rare", 3, 7),
        Card(UpgradeCard.UpgradeType.BurningDebris, "BURNING DEBRIS", "Explosions leave burning ground for 2 seconds.", "Rare", 4, 7),
        Card(UpgradeCard.UpgradeType.ConcussiveImpact, "CONCUSSIVE IMPACT", "Explosion hits slow enemies by 20% for 1 second.", "Rare", 3, 7),
        Card(UpgradeCard.UpgradeType.CraterMaker, "CRATER MAKER", "Direct hits create a lingering slowing crater.", "Epic", 2, 10),
        Card(UpgradeCard.UpgradeType.ChainDetonation, "CHAIN DETONATION", "Enemies killed by explosions detonate for 20% shell damage.", "Epic", 3, 10),
        Card(UpgradeCard.UpgradeType.SiegeShells, "SIEGE SHELLS", "Every 5th shot fires a massive shell. Stacks reduce required shots.", "Epic", 3, 10),
        Card(UpgradeCard.UpgradeType.ArmorPiercingCore, "ARMOR-PIERCING CORE", "Direct-hit damage pierces defenses, increasing direct damage.", "Rare", 4, 7),
        Card(UpgradeCard.UpgradeType.DoubleCharge, "DOUBLE CHARGE", "Every reload has a 20% chance to instantly load a second shell.", "Rare", 3, 7),
        Card(UpgradeCard.UpgradeType.SiegePlatform, "SIEGE PLATFORM", "Stable firing increases damage by 35%.", "Epic", 2, 10),
        Card(UpgradeCard.UpgradeType.ExecutionShell, "EXECUTION SHELL", "Shells deal bonus damage to enemies above 80% health.", "Uncommon", 4, 5),
        Card(UpgradeCard.UpgradeType.FinisherPayload, "FINISHER PAYLOAD", "Shells deal bonus damage to enemies below 30% health.", "Uncommon", 4, 5),
        Card(UpgradeCard.UpgradeType.StabilizedBarrel, "STABILIZED BARREL", "Projectile speed +30%.", "Common", 4, 3),
        Card(UpgradeCard.UpgradeType.SmartTargeting, "SMART TARGETING", "Prioritizes enemies near the center tower.", "Rare", 1, 7),
        Card(UpgradeCard.UpgradeType.RangefinderOptics, "RANGEFINDER OPTICS", "Tower range +18%.", "Common", 5, 3),
        Card(UpgradeCard.UpgradeType.DelayedFuse, "DELAYED FUSE", "Shells explode slightly later for deeper penetration and radius.", "Rare", 2, 7),
        Card(UpgradeCard.UpgradeType.OverloadedChamber, "OVERLOADED CHAMBER", "Damage +40%, Reload Speed -15%.", "Rare", 3, 7),
        Card(UpgradeCard.UpgradeType.VolatileMunitions, "VOLATILE MUNITIONS", "Critical kills create secondary explosions.", "Epic", 2, 10),
        Card(UpgradeCard.UpgradeType.GlassCannonDesign, "GLASS CANNON DESIGN", "Massive damage, but pauses after every 10 shots to cool down.", "Legendary", 1, 14),
        Card(UpgradeCard.UpgradeType.ApocalypseRound, "APOCALYPSE ROUND", "Every 20th shot fires an enormous extreme-damage shell.", "Legendary", 1, 14),
    };

    static readonly CardDefinition[] TeslaCards =
    {
        Card(UpgradeCard.UpgradeType.HighVoltageCoils, "HIGH VOLTAGE COILS", "Lightning Damage +12%.", "Common", 8, 3),
        Card(UpgradeCard.UpgradeType.RapidCapacitors, "RAPID CAPACITORS", "Attack Speed +10%.", "Common", 8, 3),
        Card(UpgradeCard.UpgradeType.ConductiveReach, "CONDUCTIVE REACH", "Tower Range +15%.", "Common", 5, 3),
        Card(UpgradeCard.UpgradeType.ArcStability, "ARC STABILITY", "Chain damage falloff reduced.", "Uncommon", 5, 5),
        Card(UpgradeCard.UpgradeType.OverclockedWiring, "OVERCLOCKED WIRING", "Attack Speed +18%, Range -8%.", "Uncommon", 4, 5),
        Card(UpgradeCard.UpgradeType.ExtendedArc, "EXTENDED ARC", "Lightning chains to +1 additional enemy.", "Rare", 6, 7),
        Card(UpgradeCard.UpgradeType.ForkedLightning, "FORKED LIGHTNING", "Each chain has a 20% chance to split into another nearby enemy.", "Rare", 4, 7),
        Card(UpgradeCard.UpgradeType.ArcBounce, "ARC BOUNCE", "Chains can bounce back to previously hit enemies for reduced damage.", "Epic", 2, 10),
        Card(UpgradeCard.UpgradeType.StormCurrent, "STORM CURRENT", "Chain distance increased significantly.", "Uncommon", 4, 5),
        Card(UpgradeCard.UpgradeType.Superconductor, "SUPERCONDUCTOR", "If only one enemy is hit, damage massively increases.", "Epic", 3, 10),
        Card(UpgradeCard.UpgradeType.StaticCharge, "STATIC CHARGE", "Hit enemies are slowed by 10%.", "Common", 5, 3),
        Card(UpgradeCard.UpgradeType.ParalysisField, "PARALYSIS FIELD", "Repeated hits briefly stun enemies.", "Epic", 2, 10),
        Card(UpgradeCard.UpgradeType.IonizedArmor, "IONIZED ARMOR", "Shocked enemies take increased damage from all sources.", "Rare", 4, 7),
        Card(UpgradeCard.UpgradeType.ResidualCurrent, "RESIDUAL CURRENT", "Enemies continue taking electrical damage over time after being shocked.", "Rare", 5, 7),
        Card(UpgradeCard.UpgradeType.EmpSurge, "EMP SURGE", "Every 20 attacks emits a large pulse damaging all nearby enemies.", "Epic", 3, 10),
        Card(UpgradeCard.UpgradeType.CapacitorBanks, "CAPACITOR BANKS", "Consecutive attacks without interruption gain stacking damage.", "Rare", 4, 7),
        Card(UpgradeCard.UpgradeType.EnergyOverflow, "ENERGY OVERFLOW", "Excess damage from kills transfers into the next chain.", "Rare", 3, 7),
        Card(UpgradeCard.UpgradeType.FeedbackLoop, "FEEDBACK LOOP", "Each chained enemy slightly reduces reload time for the next shot.", "Rare", 5, 7),
        Card(UpgradeCard.UpgradeType.UnstablePlasma, "UNSTABLE PLASMA", "Critical strikes create mini lightning explosions.", "Epic", 3, 10),
        Card(UpgradeCard.UpgradeType.TeslaField, "TESLA FIELD", "Creates a passive electric field around the tower damaging nearby enemies.", "Epic", 3, 10),
        Card(UpgradeCard.UpgradeType.MagneticStorm, "MAGNETIC STORM", "Shocked enemies slightly pull nearby enemies toward them.", "Epic", 2, 10),
        Card(UpgradeCard.UpgradeType.VoltageCollapse, "VOLTAGE COLLAPSE", "Enemies killed by lightning explode in a small electrical burst.", "Rare", 4, 7),
        Card(UpgradeCard.UpgradeType.MeltdownCore, "MELTDOWN CORE", "Massive damage increase, but occasional self-stun after firing.", "Legendary", 2, 14),
        Card(UpgradeCard.UpgradeType.InfiniteArc, "INFINITE ARC", "Chains no longer have a maximum target count while damage keeps decaying.", "Legendary", 1, 14),
        Card(UpgradeCard.UpgradeType.StormbringerProtocol, "STORMBRINGER PROTOCOL", "During high enemy density, attack speed ramps up dramatically.", "Legendary", 1, 14),
    };

    void Start()
    {
        InitializeUpgrades();
        EnsureBitsText();
        UpdateBitsText();
    }

    public void EnemyKilled()
    {
        salvageProgress += BaseBitsPerKill + bonusBitsPerKill * SalvageBonusPerStack;
        while (salvageProgress >= 1f)
        {
            bits++;
            salvageProgress -= 1f;
        }
        UpdateBitsText();
    }

    public void EnemyHitByBullet()
    {
        if (hitBitChance <= 0f)
            return;

        if (UnityEngine.Random.value <= hitBitChance)
        {
            bits++;
            UpdateBitsText();
        }
    }

    public void ShowWaveUpgradeChoice(Action onChoiceComplete, bool guaranteeEpicOrHigher = false)
    {
        if (choiceOverlay != null)
            return;

        InitializeUpgrades();
        EnsureBitsText();
        currentChoiceComplete = onChoiceComplete;
        currentGuaranteeEpicOrHigher = guaranteeEpicOrHigher;
        rerollCost = 1;

        List<UpgradeOffer> offers = BuildOffers(guaranteeEpicOrHigher);
        if (offers.Count == 0)
        {
            CompleteChoice();
            return;
        }

        Time.timeScale = 0f;
        choiceOverlay = CreateOverlay(offers[0].isFree);
        currentCardRow = CreateChoiceRow(choiceOverlay.transform);
        PopulateChoiceCards(offers);
        CreateRerollButton(choiceOverlay.transform);
    }

    public void ShowSpecialUpgradeChoice()
    {
        ShowWaveUpgradeChoice(null);
    }

    public void TryUpgrade(UpgradeCard.UpgradeType type)
    {
        if (!upgradeCosts.ContainsKey(type) || bits < upgradeCosts[type] || IsAtMaxStacks(type))
            return;

        BuyPaidUpgrade(type);
    }

    void InitializeUpgrades()
    {
        foreach (CardDefinition card in GetActiveCards())
        {
            if (!upgradeLevels.ContainsKey(card.type))
                upgradeLevels[card.type] = 0;
            if (!upgradeCosts.ContainsKey(card.type))
                upgradeCosts[card.type] = Mathf.CeilToInt(card.baseCost * 1.2f);
        }
    }

    List<UpgradeOffer> BuildOffers(bool guaranteeEpicOrHigher)
    {
        List<UpgradeOffer> paidOffers = new List<UpgradeOffer>();
        foreach (CardDefinition card in GetActiveCards())
        {
            int cost = upgradeCosts[card.type];
            if (bits < cost || IsAtMaxStacks(card.type))
                continue;

            int nextLevel = upgradeLevels[card.type] + 1;
            paidOffers.Add(new UpgradeOffer
            {
                paidType = card.type,
                isFree = false,
                title = card.maxStacks == 1 ? card.title : $"{card.title} {ToRoman(nextLevel)}",
                description = card.description,
                footer = $"Cost: {cost} Bits",
                rarity = card.rarity
            });
        }

        if (paidOffers.Count > 0)
            return PickRandomWeighted(paidOffers, 3, guaranteeEpicOrHigher);

        return new List<UpgradeOffer>
        {
            new UpgradeOffer
            {
                freeType = FreeUpgradeType.KillBonus,
                isFree = true,
                title = $"SALVAGE BONUS {ToRoman(bonusBitsPerKill + 1)}",
                description = "Enemies contribute +15% more salvage toward Bits.",
                footer = "Free",
                rarity = "Supply"
            },
            new UpgradeOffer
            {
                freeType = FreeUpgradeType.HitBonus,
                isFree = true,
                title = $"SCRAP ROUNDS {ToRoman(Mathf.RoundToInt(hitBitChance / 0.15f) + 1)}",
                description = "Bullet hits gain a +15% chance to give 1 bit.",
                footer = "Free",
                rarity = "Supply"
            }
        };
    }

    static List<UpgradeOffer> PickRandomWeighted(List<UpgradeOffer> source, int count, bool guaranteeEpicOrHigher)
    {
        List<UpgradeOffer> pool = new List<UpgradeOffer>(source);
        List<UpgradeOffer> result = new List<UpgradeOffer>();

        if (guaranteeEpicOrHigher)
        {
            List<UpgradeOffer> highRarity = new List<UpgradeOffer>();
            foreach (UpgradeOffer offer in pool)
            {
                if (IsEpicOrHigher(offer.rarity))
                    highRarity.Add(offer);
            }

            if (highRarity.Count > 0)
            {
                UpgradeOffer guaranteed = highRarity[UnityEngine.Random.Range(0, highRarity.Count)];
                result.Add(guaranteed);
                pool.Remove(guaranteed);
            }
        }

        while (pool.Count > 0 && result.Count < count)
        {
            float totalWeight = 0f;
            foreach (UpgradeOffer offer in pool)
                totalWeight += GetRarityWeight(offer.rarity);

            float roll = UnityEngine.Random.value * totalWeight;
            int pickedIndex = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                roll -= GetRarityWeight(pool[i].rarity);
                if (roll <= 0f)
                {
                    pickedIndex = i;
                    break;
                }
            }

            result.Add(pool[pickedIndex]);
            pool.RemoveAt(pickedIndex);
        }

        return result;
    }

    static bool IsEpicOrHigher(string rarity)
    {
        return rarity == "Epic" || rarity == "Legendary";
    }

    static float GetRarityWeight(string rarity)
    {
        switch (rarity)
        {
            case "Common": return 10f;
            case "Uncommon": return 6f;
            case "Rare": return 3.5f;
            case "Epic": return 1.5f;
            case "Legendary": return 0.55f;
            default: return 1f;
        }
    }

    void CreateChoiceCard(UpgradeOffer offer, Transform parent)
    {
        GameObject cardObject = new GameObject(offer.title + "Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.SetParent(parent, false);

        LayoutElement layout = cardObject.AddComponent<LayoutElement>();
        layout.preferredWidth = 212f;
        layout.preferredHeight = 198f;
        layout.minWidth = layout.preferredWidth;
        layout.minHeight = layout.preferredHeight;

        Image image = cardObject.GetComponent<Image>();
        image.color = offer.isFree ? new Color(0.11f, 0.17f, 0.13f, 0.98f) : GetRarityColor(offer.rarity);

        Outline outline = cardObject.AddComponent<Outline>();
        outline.effectDistance = new Vector2(3f, -3f);
        outline.effectColor = GetRarityBorderColor(offer.rarity);
        RarityPulse pulse = cardObject.AddComponent<RarityPulse>();
        pulse.Configure(outline, GetRarityBorderColor(offer.rarity));
        UITheme.AddTopAccent(cardRect, GetRarityBorderColor(offer.rarity), 4f);

        Button button = cardObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = image.color + new Color(0.04f, 0.05f, 0.06f, 0f);
        colors.pressedColor = new Color(0.06f, 0.28f, 0.36f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.08f, 0.08f, 0.09f, 0.78f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        button.onClick.AddListener(() => SelectOffer(offer));

        TextMeshProUGUI rarity = CreateText("Rarity", cardRect, offer.rarity.ToUpperInvariant(), 11f, FontStyles.Bold, TextAlignmentOptions.Center);
        rarity.color = GetRarityBorderColor(offer.rarity);
        TextMeshProUGUI title = CreateText("Title", cardRect, offer.title, 16f, FontStyles.Bold, TextAlignmentOptions.Center);
        TextMeshProUGUI description = CreateText("Description", cardRect, offer.description, 12f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        TextMeshProUGUI footer = CreateText("Footer", cardRect, offer.footer, 14f, FontStyles.Bold, TextAlignmentOptions.Center);
        footer.color = offer.isFree ? new Color(0.56f, 0.95f, 0.58f, 1f) : new Color(0.98f, 0.87f, 0.32f, 1f);

        PlaceCardText(rarity.rectTransform, 12f, -8f, -12f, 18f, true);
        PlaceCardText(title.rectTransform, 12f, -30f, -12f, 46f, true);
        PlaceCardText(description.rectTransform, 16f, -82f, -16f, 72f, true);
        PlaceCardText(footer.rectTransform, 12f, 12f, -12f, 28f, false);
    }

    RectTransform CreateChoiceRow(Transform parent)
    {
        RectTransform row = CreateRect("UpgradeChoices", parent);
        row.anchorMin = new Vector2(0.5f, 0.5f);
        row.anchorMax = new Vector2(0.5f, 0.5f);
        row.pivot = new Vector2(0.5f, 0.5f);
        row.anchoredPosition = new Vector2(0f, -12f);
        row.sizeDelta = new Vector2(700f, 212f);

        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return row;
    }

    void PopulateChoiceCards(List<UpgradeOffer> offers)
    {
        if (currentCardRow == null)
            return;

        for (int i = currentCardRow.childCount - 1; i >= 0; i--)
            Destroy(currentCardRow.GetChild(i).gameObject);

        foreach (UpgradeOffer offer in offers)
            CreateChoiceCard(offer, currentCardRow);
    }

    void CreateRerollButton(Transform parent)
    {
        Button rerollButton = CreateButton("RerollButton", parent, GetRerollLabel(), RerollOffers);
        RectTransform rect = rerollButton.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -156f);
        rect.sizeDelta = new Vector2(190f, 42f);
        rerollButton.interactable = bits >= rerollCost;
    }

    void RerollOffers()
    {
        if (bits < rerollCost || currentCardRow == null)
            return;

        bits -= rerollCost;
        rerollCost++;
        UpdateBitsText();
        PopulateChoiceCards(BuildOffers(currentGuaranteeEpicOrHigher));

        Transform rerollTransform = choiceOverlay != null ? choiceOverlay.transform.Find("RerollButton") : null;
        if (rerollTransform != null)
        {
            TextMeshProUGUI label = rerollTransform.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = GetRerollLabel();

            Button button = rerollTransform.GetComponent<Button>();
            if (button != null)
                button.interactable = bits >= rerollCost;
        }
    }

    string GetRerollLabel()
    {
        return $"REROLL ({rerollCost} BIT{(rerollCost == 1 ? "" : "S")})";
    }

    static Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "Common": return new Color(0.1f, 0.12f, 0.14f, 0.98f);
            case "Uncommon": return new Color(0.09f, 0.16f, 0.12f, 0.98f);
            case "Rare": return new Color(0.08f, 0.13f, 0.2f, 0.98f);
            case "Epic": return new Color(0.15f, 0.1f, 0.19f, 0.98f);
            case "Legendary": return new Color(0.2f, 0.15f, 0.07f, 0.98f);
            default: return new Color(0.1f, 0.11f, 0.14f, 0.98f);
        }
    }

    static Color GetRarityBorderColor(string rarity)
    {
        switch (rarity)
        {
            case "Common": return new Color(0.62f, 0.64f, 0.66f, 0.85f);
            case "Uncommon": return new Color(0.22f, 0.9f, 0.34f, 0.9f);
            case "Rare": return new Color(0.22f, 0.48f, 1f, 0.92f);
            case "Epic": return new Color(0.68f, 0.25f, 1f, 0.95f);
            case "Legendary": return new Color(1f, 0.55f, 0.08f, 0.98f);
            default: return new Color(0.62f, 0.64f, 0.66f, 0.85f);
        }
    }

    void SelectOffer(UpgradeOffer offer)
    {
        if (offer.isFree)
            ApplyFreeUpgrade(offer.freeType);
        else
            BuyPaidUpgrade(offer.paidType);

        CompleteChoice();
    }

    void BuyPaidUpgrade(UpgradeCard.UpgradeType type)
    {
        int cost = upgradeCosts[type];
        if (bits < cost || IsAtMaxStacks(type))
            return;

        bits -= cost;
        upgradeLevels[type]++;
        upgradeCosts[type] += Mathf.Max(1, Mathf.RoundToInt(cost * 0.45f));
        ApplyTowerUpgrade(type);
        UpdateBitsText();
    }

    void ApplyTowerUpgrade(UpgradeCard.UpgradeType type)
    {
        if (tower == null)
            tower = FindAnyObjectByType<Tower>();

        if (tower == null)
            return;

        switch (type)
        {
            case UpgradeCard.UpgradeType.ReinforcedPowder:
                tower.shellDamageMultiplier *= 1.15f;
                break;
            case UpgradeCard.UpgradeType.ExpandedPayload:
                tower.splashRadius *= 1.12f;
                break;
            case UpgradeCard.UpgradeType.AutoLoader:
                tower.fireRate *= 0.9f;
                break;
            case UpgradeCard.UpgradeType.HeavyCaliber:
                tower.shellDamageMultiplier *= 1.25f;
                tower.fireRate *= 1.08f;
                break;
            case UpgradeCard.UpgradeType.ShockwaveShells:
                tower.explosionKnockback += 0.12f;
                break;
            case UpgradeCard.UpgradeType.ShrapnelRounds:
                tower.shrapnelEdgeDamageMultiplier += 0.35f;
                break;
            case UpgradeCard.UpgradeType.FragmentBurst:
                tower.fragmentCount += 3;
                break;
            case UpgradeCard.UpgradeType.BurningDebris:
                tower.burningGroundDuration += 2f;
                break;
            case UpgradeCard.UpgradeType.ConcussiveImpact:
                tower.concussiveSlowMultiplier = Mathf.Min(tower.concussiveSlowMultiplier, Mathf.Max(0.35f, 0.8f - 0.06f * upgradeLevels[type]));
                tower.concussiveSlowDuration += 1f;
                break;
            case UpgradeCard.UpgradeType.CraterMaker:
                tower.craterDuration += 2.5f;
                tower.craterSlowMultiplier = Mathf.Min(tower.craterSlowMultiplier, 0.7f);
                break;
            case UpgradeCard.UpgradeType.ChainDetonation:
                tower.chainDetonationMultiplier += 0.2f;
                break;
            case UpgradeCard.UpgradeType.SiegeShells:
                tower.siegeShellStacks++;
                break;
            case UpgradeCard.UpgradeType.ArmorPiercingCore:
                tower.directHitDamageMultiplier *= 1.25f;
                break;
            case UpgradeCard.UpgradeType.DoubleCharge:
                tower.doubleChargeChance += 0.2f;
                break;
            case UpgradeCard.UpgradeType.SiegePlatform:
                tower.siegePlatformStacks++;
                break;
            case UpgradeCard.UpgradeType.ExecutionShell:
                tower.executionDamageMultiplier += 0.25f;
                break;
            case UpgradeCard.UpgradeType.FinisherPayload:
                tower.finisherDamageMultiplier += 0.25f;
                break;
            case UpgradeCard.UpgradeType.StabilizedBarrel:
                tower.projectileSpeedMultiplier *= 1.3f;
                break;
            case UpgradeCard.UpgradeType.SmartTargeting:
                tower.smartTargeting = true;
                break;
            case UpgradeCard.UpgradeType.RangefinderOptics:
                tower.attackRange *= 1.18f;
                break;
            case UpgradeCard.UpgradeType.DelayedFuse:
                tower.delayedFuseSeconds += 0.08f;
                tower.delayedFuseRadiusMultiplier *= 1.16f;
                break;
            case UpgradeCard.UpgradeType.OverloadedChamber:
                tower.shellDamageMultiplier *= 1.4f;
                tower.fireRate *= 1.15f;
                break;
            case UpgradeCard.UpgradeType.VolatileMunitions:
                tower.volatileSecondaryExplosionMultiplier += 0.35f;
                break;
            case UpgradeCard.UpgradeType.GlassCannonDesign:
                tower.glassCannonDesign = true;
                tower.shellDamageMultiplier *= 2.25f;
                break;
            case UpgradeCard.UpgradeType.ApocalypseRound:
                tower.apocalypseRound = true;
                break;
            case UpgradeCard.UpgradeType.HighVoltageCoils:
                tower.teslaDamageMultiplier *= 1.12f;
                break;
            case UpgradeCard.UpgradeType.RapidCapacitors:
                tower.fireRate *= 0.9f;
                break;
            case UpgradeCard.UpgradeType.ConductiveReach:
                tower.attackRange *= 1.15f;
                break;
            case UpgradeCard.UpgradeType.ArcStability:
                tower.teslaChainFalloff = Mathf.Min(0.94f, tower.teslaChainFalloff + 0.045f);
                break;
            case UpgradeCard.UpgradeType.OverclockedWiring:
                tower.fireRate *= 0.82f;
                tower.attackRange *= 0.92f;
                break;
            case UpgradeCard.UpgradeType.ExtendedArc:
                tower.teslaChainCount++;
                break;
            case UpgradeCard.UpgradeType.ForkedLightning:
                tower.teslaForkChance += 0.2f;
                break;
            case UpgradeCard.UpgradeType.ArcBounce:
                tower.teslaArcBounce = true;
                tower.teslaChainFalloff = Mathf.Min(0.96f, tower.teslaChainFalloff + 0.04f);
                break;
            case UpgradeCard.UpgradeType.StormCurrent:
                tower.teslaChainRange *= 1.28f;
                break;
            case UpgradeCard.UpgradeType.Superconductor:
                tower.teslaSuperconductorStacks++;
                break;
            case UpgradeCard.UpgradeType.StaticCharge:
                tower.teslaSlowMultiplier = Mathf.Min(tower.teslaSlowMultiplier, Mathf.Max(0.45f, 0.9f - 0.06f * upgradeLevels[type]));
                tower.teslaSlowDuration = Mathf.Max(tower.teslaSlowDuration, 1.5f);
                break;
            case UpgradeCard.UpgradeType.ParalysisField:
                tower.teslaParalysisStacks++;
                break;
            case UpgradeCard.UpgradeType.IonizedArmor:
                tower.teslaIonizedBonus += 0.12f;
                break;
            case UpgradeCard.UpgradeType.ResidualCurrent:
                tower.teslaResidualDamageMultiplier += 0.14f;
                break;
            case UpgradeCard.UpgradeType.EmpSurge:
                tower.teslaEmpSurgeStacks++;
                break;
            case UpgradeCard.UpgradeType.CapacitorBanks:
                tower.teslaCapacitorBankStacks++;
                break;
            case UpgradeCard.UpgradeType.EnergyOverflow:
                tower.teslaEnergyOverflow = true;
                tower.teslaDamageMultiplier *= 1.06f;
                break;
            case UpgradeCard.UpgradeType.FeedbackLoop:
                tower.teslaFeedbackLoopMultiplier += 0.025f;
                break;
            case UpgradeCard.UpgradeType.UnstablePlasma:
                tower.teslaCriticalExplosionChance += 0.12f;
                break;
            case UpgradeCard.UpgradeType.TeslaField:
                tower.teslaFieldStacks++;
                break;
            case UpgradeCard.UpgradeType.MagneticStorm:
                tower.teslaMagneticPull += 0.08f;
                break;
            case UpgradeCard.UpgradeType.VoltageCollapse:
                tower.teslaVoltageCollapseMultiplier += 0.22f;
                break;
            case UpgradeCard.UpgradeType.MeltdownCore:
                tower.teslaMeltdownStacks++;
                tower.teslaDamageMultiplier *= 1.55f;
                break;
            case UpgradeCard.UpgradeType.InfiniteArc:
                tower.teslaInfiniteArc = true;
                break;
            case UpgradeCard.UpgradeType.StormbringerProtocol:
                tower.teslaStormbringerProtocol = true;
                break;
        }
    }

    void ApplyFreeUpgrade(FreeUpgradeType type)
    {
        switch (type)
        {
            case FreeUpgradeType.KillBonus:
                bonusBitsPerKill++;
                break;
            case FreeUpgradeType.HitBonus:
                hitBitChance += 0.15f;
                break;
        }

        UpdateBitsText();
    }

    void CompleteChoice()
    {
        if (choiceOverlay != null)
            Destroy(choiceOverlay);

        choiceOverlay = null;
        currentCardRow = null;
        rerollCost = 1;
        Time.timeScale = 1f;

        Action callback = currentChoiceComplete;
        currentChoiceComplete = null;
        callback?.Invoke();
    }

    bool IsAtMaxStacks(UpgradeCard.UpgradeType type)
    {
        CardDefinition card = GetCard(type);
        return card.maxStacks > 0 && upgradeLevels.TryGetValue(type, out int level) && level >= card.maxStacks;
    }

    void EnsureBitsText()
    {
        if (bitsText != null)
        {
            StyleBitsText();
            return;
        }

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

        Transform existing = canvas.transform.Find("BitsCounter");
        if (existing != null)
            bitsText = existing.GetComponent<TextMeshProUGUI>();

        if (bitsText == null)
        {
            GameObject textObject = new GameObject("BitsCounter", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(canvas.transform, false);
            bitsText = textObject.GetComponent<TextMeshProUGUI>();
        }

        StyleBitsText();
    }

    void UpdateBitsText()
    {
        EnsureBitsText();
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
            rect.sizeDelta = new Vector2(170f, 42f);
            EnsureBitsPanel(rect);
        }

        bitsText.alignment = TextAlignmentOptions.Center;
        bitsText.fontSize = 21f;
        bitsText.fontStyle = FontStyles.Bold;
        bitsText.color = UITheme.Amber;
        bitsText.raycastTarget = false;

        Outline outline = bitsText.GetComponent<Outline>();
        if (outline == null)
            outline = bitsText.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.72f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }

    static void EnsureBitsPanel(RectTransform bitsRect)
    {
        Transform parent = bitsRect.parent;
        if (parent == null || parent.Find("BitsCounterPanel") != null)
            return;

        GameObject panelObject = new GameObject("BitsCounterPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.SetParent(parent, false);
        panel.anchorMin = bitsRect.anchorMin;
        panel.anchorMax = bitsRect.anchorMax;
        panel.pivot = bitsRect.pivot;
        panel.anchoredPosition = bitsRect.anchoredPosition;
        panel.sizeDelta = bitsRect.sizeDelta;
        panel.SetSiblingIndex(bitsRect.GetSiblingIndex());

        UITheme.StylePanel(panelObject.GetComponent<Image>(), UITheme.Panel, new Color(0.66f, 0.5f, 0.16f, 0.85f));
        UITheme.AddTopAccent(panel, UITheme.Amber, 3f);
    }

    GameObject CreateOverlay(bool freeFallback)
    {
        Canvas canvas = bitsText.GetComponentInParent<Canvas>();
        GameObject overlay = new GameObject("WaveUpgradeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        Image image = overlay.GetComponent<Image>();
        image.color = new Color(0.005f, 0.012f, 0.022f, 0.88f);
        image.raycastTarget = true;

        string titleText = freeFallback ? "LOW BITS: CHOOSE SUPPLY BOOST" : IsTeslaTowerSelected() ? "CHOOSE TESLA UPGRADE" : "CHOOSE CANNON UPGRADE";
        TextMeshProUGUI title = CreateText("Title", rect, titleText, 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        title.color = IsTeslaTowerSelected() ? UITheme.CyanBright : UITheme.Amber;
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 138f);
        titleRect.sizeDelta = new Vector2(640f, 42f);

        TextMeshProUGUI bitsLabel = CreateText("Bits", rect, "BITS " + bits, 18f, FontStyles.Bold, TextAlignmentOptions.Center);
        bitsLabel.color = new Color(0.98f, 0.87f, 0.32f, 1f);
        RectTransform bitsRect = bitsLabel.rectTransform;
        bitsRect.anchorMin = new Vector2(0.5f, 0.5f);
        bitsRect.anchorMax = new Vector2(0.5f, 0.5f);
        bitsRect.pivot = new Vector2(0.5f, 0.5f);
        bitsRect.anchoredPosition = new Vector2(0f, 104f);
        bitsRect.sizeDelta = new Vector2(220f, 30f);

        RectTransform headerLine = UITheme.AddAccent(rect, "HeaderLine", new Color(0.18f, 0.56f, 0.72f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260f, 88f), new Vector2(260f, 90f));
        headerLine.SetAsFirstSibling();

        return overlay;
    }

    static CardDefinition Card(UpgradeCard.UpgradeType type, string title, string description, string rarity, int maxStacks, int baseCost)
    {
        return new CardDefinition
        {
            type = type,
            title = title,
            description = description,
            rarity = rarity,
            maxStacks = maxStacks,
            baseCost = baseCost
        };
    }

    static CardDefinition GetCard(UpgradeCard.UpgradeType type)
    {
        foreach (CardDefinition card in CannonCards)
        {
            if (card.type == type)
                return card;
        }

        foreach (CardDefinition card in TeslaCards)
        {
            if (card.type == type)
                return card;
        }

        return CannonCards[0];
    }

    static CardDefinition[] GetActiveCards()
    {
        return IsTeslaTowerSelected() ? TeslaCards : CannonCards;
    }

    static bool IsTeslaTowerSelected()
    {
        return PlayerPrefs.GetString(SelectedTowerKey, "cannon") == TeslaTowerId;
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    static Button CreateButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        Button button = obj.GetComponent<Button>();
        UITheme.StyleButton(button, true);
        button.onClick.AddListener(onClick);

        TextMeshProUGUI text = CreateText("Text", rect, label, 16f, FontStyles.Bold, TextAlignmentOptions.Center);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
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

    static void PlaceCardText(RectTransform rect, float left, float top, float right, float height, bool fromTop)
    {
        rect.anchorMin = fromTop ? new Vector2(0f, 1f) : new Vector2(0f, 0f);
        rect.anchorMax = fromTop ? new Vector2(1f, 1f) : new Vector2(1f, 0f);
        rect.pivot = fromTop ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(left, fromTop ? -height : top);
        rect.offsetMax = new Vector2(right, fromTop ? top : top + height);
    }

    static string ToRoman(int number)
    {
        string[] numerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
        return number < 1 || number > 10 ? number.ToString() : numerals[number - 1];
    }
}
