using System.Collections; // Required for IEnumerator
using UnityEngine;
using TMPro;

public class TowerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    public GameObject floatingTextPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        ShowFloatingDamage(damage);
        UpdateColor();

        if (currentHealth <= 0)
        {
            Debug.Log("Tower Destroyed! Game Over.");
        }
    }

    void UpdateColor()
    {
        float healthPercentage = (float)currentHealth / maxHealth;
        spriteRenderer.color = Color.Lerp(Color.red, Color.green, healthPercentage);
    }

    void ShowFloatingDamage(int damage)
    {
        if (floatingTextPrefab != null)
        {
            GameObject damageTextInstance = Instantiate(
                floatingTextPrefab,
                transform.position + new Vector3(0, 1.5f, 0),
                Quaternion.identity
            );
            TextMeshPro tmp = damageTextInstance.GetComponent<TextMeshPro>();
            tmp.text = "-" + damage.ToString();
            StartCoroutine(FadeAndMoveText(damageTextInstance));
            Destroy(damageTextInstance, 1.5f);
        }
    }
    
    IEnumerator FadeAndMoveText(GameObject textObj)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = textObj.transform.position;
        Vector3 targetPos = startPos + new Vector3(0, 1f, 0); // Move up
        TextMeshPro tmp = textObj.GetComponent<TextMeshPro>();
        Color startColor = tmp.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            textObj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);
            yield return null;
        }
    }
}