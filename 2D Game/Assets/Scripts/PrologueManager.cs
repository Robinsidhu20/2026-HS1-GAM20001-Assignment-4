using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PrologueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI prologueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private CanvasGroup textCanvasGroup;

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private float pauseBetweenParagraphs = 1.5f;
    [SerializeField] private float fadeInDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioSource backgroundMusicSource;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "SCENE 1 - CABIN TO MINE";

    private string[] paragraphs = new string[]
    {
        "<color=#E8D5A3>Tammy</color> is a tenacious and independent young girl. She lives with her cat, <color=#E8D5A3>KitKat</color>, in the lodge she grew up in the <color=#7BA7C9>Blackridge Forest</color>. KitKat has been by her side since she was a little girl. Tammy would do anything for KitKat.",
        "One night, mysteriously on the same night her parents went missing a year ago, she awoke to find <color=#E8D5A3>KitKat</color> missing. Afraid but determined to find her best friend, <color=#E8D5A3>Tammy</color> is preparing to venture out of her home into the deep woods.",
        "She remembers the story her parents told her of <color=#7BA7C9>Blackridge Forest</color>...",
        "<i><size=110%><color=#C9B8E8>A dark and mystic space, creatures ready to chase.\nFew friends to find in the dark, only the strong-willed should embark.\nEnter at your own pace, for there are dangers within this place.</color></size></i>"
    };

    private bool isTyping = false;
    private bool skipRequested = false;
    private bool skipAllRequested = false;
    private AudioClip generatedTypingClip;
    private Coroutine prologueCoroutine;

    private void Awake()
    {
        // Generate a soft, gentle typing tick sound
        int sampleRate = 44100;
        int sampleLength = (int)(0.02f * sampleRate); // 20ms
        generatedTypingClip = AudioClip.Create("TypingTick", sampleLength, 1, sampleRate, false);
        float[] samples = new float[sampleLength];
        for (int i = 0; i < sampleLength; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 300f); // Quick fade out
            samples[i] = Mathf.Sin(2f * Mathf.PI * 800f * t) * envelope * 0.15f;
        }
        generatedTypingClip.SetData(samples, 0);
    }

    private void Start()
    {
        continueButton.gameObject.SetActive(false);
        prologueText.text = "";

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 0f;

        prologueCoroutine = StartCoroutine(PlayPrologue());

        // Wire up the continue button via code so it always works
        continueButton.onClick.AddListener(LoadNextScene);
    }

    private IEnumerator PlayPrologue()
    {
        // Fade in the text area
        if (textCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                textCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            textCanvasGroup.alpha = 1f;
        }

        // Type each paragraph
        for (int i = 0; i < paragraphs.Length; i++)
        {
            if (skipAllRequested)
            {
                // Show all remaining paragraphs instantly
                string remaining = "";
                for (int j = i; j < paragraphs.Length; j++)
                {
                    if (j > i) remaining += "\n\n";
                    remaining += paragraphs[j];
                }
                prologueText.text += remaining;
                break;
            }

            yield return StartCoroutine(TypeParagraph(paragraphs[i]));

            if (i < paragraphs.Length - 1)
            {
                prologueText.text += "\n\n";
                yield return new WaitForSeconds(pauseBetweenParagraphs);
            }
        }

        // Show continue button with fade
        yield return new WaitForSeconds(1f);
        continueButton.gameObject.SetActive(true);
        StartCoroutine(FadeInButton());
    }

    private IEnumerator TypeParagraph(string paragraph)
    {
        isTyping = true;
        skipRequested = false;

        int startIndex = prologueText.text.Length;
        int i = 0;

        while (i < paragraph.Length)
        {
            if (skipRequested)
            {
                prologueText.text = prologueText.text.Substring(0, startIndex) + paragraph;
                break;
            }

            // If we hit a rich text tag, add the entire tag instantly
            if (paragraph[i] == '<')
            {
                int closingBracket = paragraph.IndexOf('>', i);
                if (closingBracket != -1)
                {
                    prologueText.text += paragraph.Substring(i, closingBracket - i + 1);
                    i = closingBracket + 1;
                    continue;
                }
            }

            prologueText.text += paragraph[i];

            // Play soft typing tick for visible characters
            if (paragraph[i] != ' ' && paragraph[i] != '\n' && typingAudioSource != null)
            {
                typingAudioSource.pitch = Random.Range(0.95f, 1.05f);
                typingAudioSource.PlayOneShot(generatedTypingClip);
            }

            i++;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        skipRequested = false;
    }

    private IEnumerator FadeInButton()
    {
        CanvasGroup btnGroup = continueButton.GetComponent<CanvasGroup>();
        if (btnGroup == null)
            btnGroup = continueButton.gameObject.AddComponent<CanvasGroup>();

        btnGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            btnGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 1f);
            yield return null;
        }
        btnGroup.alpha = 1f;
    }

    private void Update()
    {
        // Enter/Return shows all text instantly and shows the continue button
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            skipRequested = true;
            skipAllRequested = true;

            // Stop the prologue coroutine and show everything
            if (prologueCoroutine != null)
            {
                StopCoroutine(prologueCoroutine);
                prologueCoroutine = null;
            }

            // Show all text
            string fullText = string.Join("\n\n", paragraphs);
            prologueText.text = fullText;

            if (textCanvasGroup != null)
                textCanvasGroup.alpha = 1f;

            // Show the continue button
            continueButton.gameObject.SetActive(true);
            CanvasGroup btnGroup = continueButton.GetComponent<CanvasGroup>();
            if (btnGroup != null) btnGroup.alpha = 1f;

            isTyping = false;
        }
        // Click or any other key skips current paragraph
        else if (isTyping && (Input.GetMouseButtonDown(0) || Input.anyKeyDown))
        {
            skipRequested = true;
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
