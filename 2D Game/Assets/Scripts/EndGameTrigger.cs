using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Place on an empty GameObject with a trigger Collider2D near the pulley.
// When the player reaches it: hides the cat (so it "goes into" the pulley)
// and fades in an end-of-demo message. The player can keep moving around.
[RequireComponent(typeof(Collider2D))]
public class EndGameTrigger : MonoBehaviour
{
    [TextArea(3, 6)]
    [SerializeField]
    private string message =
        "KitKat slips into the old pulley shaft\nand vanishes into the dark...\n\n" +
        "You'll have to follow her down to find her.\n\n" +
        "<size=65%>Demo ends here  -  press Esc for the menu</size>";

    [Tooltip("Optional: the cat object to hide when triggered, so it looks like it went into the pulley.")]
    [SerializeField] private GameObject catObject;

    [SerializeField] private float fadeDuration = 1.2f;

    private bool triggered = false;
    private CanvasGroup group;

    private void Reset()
    {
        ForceTrigger();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ForceTrigger();
    }
#endif

    private void OnEnable()
    {
        ForceTrigger();
    }

    // Make sure this collider can never act as a solid wall
    private void ForceTrigger()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (!collision.CompareTag("Player")) return;

        triggered = true;

        if (catObject != null)
            catObject.SetActive(false);

        BuildUI();
        StartCoroutine(FadeIn());
    }

    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("EndGameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 800;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        group = canvasGO.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        // Dim the screen a little
        GameObject bg = new GameObject("Dim");
        bg.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.72f);
        bgImg.raycastTarget = false;
        RectTransform bgrt = bgImg.rectTransform;
        bgrt.anchorMin = Vector2.zero;
        bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = Vector2.zero;
        bgrt.offsetMax = Vector2.zero;

        // The message
        GameObject txtGO = new GameObject("Message");
        txtGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = message;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 46f;
        tmp.color = new Color(0.96f, 0.92f, 0.78f);
        tmp.raycastTarget = false;
        RectTransform trt = tmp.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(1200f, 520f);
        trt.anchoredPosition = Vector2.zero;
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        group.alpha = 1f;
    }

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
