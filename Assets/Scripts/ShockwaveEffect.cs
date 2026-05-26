using UnityEngine;

public class ShockwaveEffect : MonoBehaviour
{
    LineRenderer line;
    float duration = 0.28f;
    float elapsed;
    Vector3 startScale;

    public void Configure(float effectDuration)
    {
        duration = Mathf.Max(0.05f, effectDuration);
    }

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        startScale = transform.localScale * 0.35f;
        transform.localScale = startScale;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        transform.localScale = Vector3.Lerp(startScale, Vector3.one, t);

        if (line != null)
        {
            Color color = Color.Lerp(new Color(0.95f, 0.82f, 0.45f, 0.8f), new Color(0.95f, 0.82f, 0.45f, 0f), t);
            line.startColor = color;
            line.endColor = color;
        }

        if (elapsed >= duration)
            Destroy(gameObject);
    }
}
