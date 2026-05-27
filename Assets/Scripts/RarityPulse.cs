using UnityEngine;
using UnityEngine.UI;

public class RarityPulse : MonoBehaviour
{
    Outline outline;
    Color baseColor = Color.white;

    public void Configure(Outline targetOutline, Color color)
    {
        outline = targetOutline;
        baseColor = color;
    }

    void Awake()
    {
        if (outline == null)
            outline = GetComponent<Outline>();
    }

    void Update()
    {
        if (outline == null)
            return;

        float pulse = 0.55f + Mathf.Sin(Time.unscaledTime * 4.5f) * 0.25f;
        Color color = baseColor;
        color.a = Mathf.Clamp01(pulse);
        outline.effectColor = color;
        float distance = 2.5f + Mathf.Sin(Time.unscaledTime * 4.5f) * 0.8f;
        outline.effectDistance = new Vector2(distance, -distance);
    }
}
