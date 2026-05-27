using TMPro;
using UnityEngine;

public class FloatingWorldText : MonoBehaviour
{
    TextMeshPro text;
    Vector3 startPosition;
    Vector3 offset = new Vector3(0f, 0.6f, 0f);
    Color startColor = Color.white;
    float duration = 0.75f;
    float elapsed;

    public void Configure(float fadeDuration, Vector3 moveOffset)
    {
        duration = Mathf.Max(0.05f, fadeDuration);
        offset = moveOffset;
    }

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
        startPosition = transform.position;
        if (text != null)
            startColor = text.color;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        transform.position = Vector3.Lerp(startPosition, startPosition + offset, t);

        if (text != null)
            text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

        if (elapsed >= duration)
            Destroy(gameObject);
    }
}
