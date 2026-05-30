using UnityEngine;

public class ShockwaveEffect : MonoBehaviour
{
    LineRenderer line;
    float duration = 0.28f;
    float elapsed;
    Vector3 startScale;
    Color startColor;
    Color endColor;

    public void Configure(float effectDuration)
    {
        duration = Mathf.Max(0.05f, effectDuration);
    }

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        startScale = transform.localScale * 0.35f;
        transform.localScale = startScale;
        if (line != null)
        {
            startColor = line.startColor;
            endColor = line.endColor;
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        transform.localScale = Vector3.Lerp(startScale, Vector3.one, t);

        if (line != null)
        {
            line.startColor = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
            line.endColor = new Color(endColor.r, endColor.g, endColor.b, Mathf.Lerp(endColor.a, 0f, t));
        }

        if (elapsed >= duration)
            Destroy(gameObject);
    }
}
