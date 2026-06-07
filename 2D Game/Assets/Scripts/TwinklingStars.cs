using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TwinklingStars : MonoBehaviour
{
    [SerializeField] private int starCount = 80;
    [SerializeField] private float minTwinkleSpeed = 1.5f;
    [SerializeField] private float maxTwinkleSpeed = 5f;
    [SerializeField] private float minSize = 2f;
    [SerializeField] private float maxSize = 7f;

    private RectTransform parentRect;

    private IEnumerator Start()
    {
        parentRect = GetComponent<RectTransform>();

        // Wait a frame so the canvas layout is calculated
        yield return null;

        float width = parentRect.rect.width;
        float height = parentRect.rect.height;

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = new GameObject("Star_" + i);
            star.transform.SetParent(transform, false);

            Image img = star.AddComponent<Image>();
            // Mix of white, warm yellow, and pale blue stars
            float colorRoll = Random.Range(0f, 1f);
            Color starColor;
            if (colorRoll < 0.5f)
                starColor = new Color(1f, 1f, 1f, 1f); // White
            else if (colorRoll < 0.75f)
                starColor = new Color(1f, 0.95f, 0.7f, 1f); // Warm yellow
            else
                starColor = new Color(0.7f, 0.85f, 1f, 1f); // Pale blue

            img.color = starColor;

            RectTransform rect = star.GetComponent<RectTransform>();
            float size = Random.Range(minSize, maxSize);
            rect.sizeDelta = new Vector2(size, size);

            float x = Random.Range(-width / 2f, width / 2f);
            float y = Random.Range(-height / 2f, height / 2f);
            rect.anchoredPosition = new Vector2(x, y);

            StartCoroutine(Twinkle(img));
        }
    }

    private IEnumerator Twinkle(Image star)
    {
        float speed = Random.Range(minTwinkleSpeed, maxTwinkleSpeed);
        float offset = Random.Range(0f, Mathf.PI * 2f);
        Color baseColor = star.color;

        while (true)
        {
            // Strong alpha pulse from nearly invisible to fully bright
            float alpha = Mathf.Lerp(0.05f, 1f, (Mathf.Sin(Time.time * speed + offset) + 1f) / 2f);
            star.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }
    }
}
