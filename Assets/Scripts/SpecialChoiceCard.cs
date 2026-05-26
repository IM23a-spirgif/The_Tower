using UnityEngine;

public class SpecialChoiceCard : MonoBehaviour
{
    public UpgradeManager upgradeManager;

    void OnMouseDown()
    {
        UpgradeCard card = GetComponent<UpgradeCard>();
        if (card != null && upgradeManager != null)
        {
            upgradeManager.TryUpgrade(card.upgradeType);
        }

        SpecialChoiceCard[] allChoices = FindObjectsByType<SpecialChoiceCard>();
        foreach (var choice in allChoices)
        {
            Destroy(choice.gameObject);
        }

        Time.timeScale = 1f;
    }
}
