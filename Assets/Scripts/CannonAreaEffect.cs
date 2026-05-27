using System.Collections.Generic;
using UnityEngine;

public class CannonAreaEffect : MonoBehaviour
{
    readonly Dictionary<EnemyHealth, float> nextDamageTick = new Dictionary<EnemyHealth, float>();

    float duration;
    int damagePerSecond;
    float slowMultiplier = 1f;
    float expiresAt;
    SpriteRenderer visual;

    public void Configure(float effectDuration, int burnDamagePerSecond, float craterSlowMultiplier, float radius)
    {
        duration = effectDuration;
        damagePerSecond = burnDamagePerSecond;
        slowMultiplier = Mathf.Clamp(craterSlowMultiplier, 0.1f, 1f);
        expiresAt = Time.time + duration;
        CreateVisual(radius);
        Destroy(gameObject, duration);
    }

    void Update()
    {
        if (visual == null || duration <= 0f)
            return;

        float remaining = Mathf.Clamp01((expiresAt - Time.time) / duration);
        Color color = visual.color;
        color.a = Mathf.Lerp(0f, color.a, remaining);
        visual.color = color;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy"))
            return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && slowMultiplier < 1f)
            enemy.ApplySlow(slowMultiplier, 0.18f);

        if (damagePerSecond <= 0)
            return;

        EnemyHealth health = other.GetComponent<EnemyHealth>();
        if (health == null)
            return;

        if (!nextDamageTick.TryGetValue(health, out float nextTick))
            nextTick = 0f;

        if (Time.time < nextTick || Time.time >= expiresAt)
            return;

        health.TakeDamage(Mathf.Max(1, damagePerSecond));
        nextDamageTick[health] = Time.time + 1f;
    }

    void CreateVisual(float radius)
    {
        GameObject visualObject = new GameObject("Visual", typeof(SpriteRenderer));
        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localScale = Vector3.one * radius * 2f;

        visual = visualObject.GetComponent<SpriteRenderer>();
        visual.sprite = CreateCircleSprite();
        visual.sortingOrder = -1;

        if (damagePerSecond > 0 && slowMultiplier < 1f)
            visual.color = new Color(0.95f, 0.33f, 0.12f, 0.34f);
        else if (damagePerSecond > 0)
            visual.color = new Color(1f, 0.32f, 0.08f, 0.34f);
        else
            visual.color = new Color(0.34f, 0.24f, 0.16f, 0.38f);
    }

    static Sprite CreateCircleSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.46f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((radius - distance) / 5f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
