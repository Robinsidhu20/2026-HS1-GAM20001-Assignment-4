using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// Builds a full-screen black overlay in code, fades IN when a scene loads,
// and fades OUT before loading the next scene. Also moves the player to a
// named spawn point on arrival, so transitions work both directions.
// Put ONE empty GameObject with this script in each gameplay scene.
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] private float fadeDuration = 0.6f;

    // Name of the spawn-point GameObject to place the player at in the next
    // scene. Set by SceneTransitionTrigger just before we load.
    private static string nextSpawnPointName;

    private CanvasGroup fadeGroup;
    private bool isTransitioning = false;

    private void Awake()
    {
        Instance = this;
        BuildOverlay();
    }

    private void BuildOverlay()
    {
        GameObject canvasGO = new GameObject("SceneFadeCanvas");
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // above everything else

        GameObject imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        Image img = imgGO.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeGroup = imgGO.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 1f; // start fully black, then fade in
        fadeGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        // If we arrived through a transition, snap the player to the spawn point
        if (!string.IsNullOrEmpty(nextSpawnPointName))
        {
            GameObject spawn = GameObject.Find(nextSpawnPointName);
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (spawn != null && player != null)
            {
                player.transform.position = spawn.transform.position;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
            nextSpawnPointName = null;
        }

        StartCoroutine(Fade(1f, 0f)); // fade in from black
    }

    public void FadeToScene(string sceneName, string spawnPointName)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        nextSpawnPointName = spawnPointName;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return Fade(0f, 1f); // fade to black
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        fadeGroup.alpha = from;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // unaffected by pause / timeScale
            fadeGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }
}
