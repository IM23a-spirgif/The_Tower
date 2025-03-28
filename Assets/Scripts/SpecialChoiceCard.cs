using UnityEngine;

public class SpecialChoiceCard : MonoBehaviour
{
    public UpgradeManager upgradeManager;

    void OnMouseDown()
    {
        // Identify which UpgradeType this card has
        UpgradeCard card = GetComponent<UpgradeCard>();
        if (card != null)
        {
            upgradeManager.UnlockSpecialUpgrade(card.upgradeType);
        }

        // Destroy all other special cards
        SpecialChoiceCard[] allChoices = FindObjectsOfType<SpecialChoiceCard>();
        foreach (var choice in allChoices)
        {
            Destroy(choice.gameObject);
        }

        Time.timeScale = 1f; // Resume game
    }
}