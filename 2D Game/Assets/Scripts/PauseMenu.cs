using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

// Esc opens/closes a simple pause menu. Works via clickable buttons AND
// keyboard shortcuts (Esc = resume, M = main menu, Q = quit) so it's reliable
// regardless of EventSystem setup. Put on one GameObject in each gameplay scene.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MAIN MENU";

    private GameObject menuRoot;
    private bool isPaused = false;

    private void Start()
    {
        EnsureEventSystem();
        BuildMenu();
        menuRoot.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
            return;
        }

        if (!isPaused) return;

        // Keyboard shortcuts while paused
        if (Keyboard.current.mKey.wasPressedThisFrame) GoToMainMenu();
        else if (Keyboard.current.qKey.wasPressedThisFrame) QuitGame();
    }

    private void Pause()
    {
        isPaused = true;
        menuRoot.SetActive(true);
        Time.timeScale = 0f; // freeze the game while paused
    }

    public void Resume()
    {
        isPaused = false;
        menuRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnsureEventSystem()
    {
        // Needed for mouse clicks on the buttons. Keyboard shortcuts work either way.
        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    private void BuildMenu()
    {
        menuRoot = new GameObject("PauseMenuCanvas");
        Canvas canvas = menuRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        menuRoot.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = menuRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Solid background so nothing behind (e.g. the ending text) shows through
        GameObject bg = new GameObject("Dim");
        bg.transform.SetParent(menuRoot.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.04f, 0.09f, 0.98f);
        RectTransform bgrt = bgImg.rectTransform;
        bgrt.anchorMin = Vector2.zero;
        bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = Vector2.zero;
        bgrt.offsetMax = Vector2.zero;

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(menuRoot.transform, false);
        TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = "PAUSED";
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 80f;
        title.fontStyle = FontStyles.Bold;
        title.color = new Color(0.96f, 0.92f, 0.78f);
        RectTransform trt = title.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(800f, 140f);
        trt.anchoredPosition = new Vector2(0f, 240f);

        MakeButton("Resume   (Esc)", new Vector2(0f, 90f), Resume);
        MakeButton("Main Menu   (M)", new Vector2(0f, -10f), GoToMainMenu);
        MakeButton("Quit   (Q)", new Vector2(0f, -110f), QuitGame);
    }

    private void MakeButton(string label, Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label + " Button");
        go.transform.SetParent(menuRoot.transform, false);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.22f, 0.18f, 0.32f, 1f);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.22f, 0.18f, 0.32f, 1f);
        cb.highlightedColor = new Color(0.34f, 0.28f, 0.48f, 1f);
        cb.pressedColor = new Color(0.16f, 0.13f, 0.24f, 1f);
        cb.selectedColor = cb.highlightedColor;
        btn.colors = cb;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(420f, 84f);
        rt.anchoredPosition = anchoredPos;

        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 32f;
        tmp.color = Color.white;
        RectTransform txtrt = tmp.rectTransform;
        txtrt.anchorMin = Vector2.zero;
        txtrt.anchorMax = Vector2.one;
        txtrt.offsetMin = Vector2.zero;
        txtrt.offsetMax = Vector2.zero;

        btn.onClick.AddListener(onClick);
    }
}
