using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI continuePrompt;

    [Header("Settings")]
    [SerializeField] private float typeSpeed = 0.03f;

    [Header("Audio")]
    [SerializeField] private AudioSource dialogueAudioSource;

    [Header("Panel Styling")]
    [Tooltip("Background colour of the dialogue box. Applied on Play so it always stands out from the dark ground.")]
    [SerializeField] private Color panelColor = new Color(0.16f, 0.12f, 0.28f, 0.96f); // deep indigo
    [SerializeField] private Color continuePromptColor = new Color(0.96f, 0.90f, 0.70f, 1f); // soft cream

    private Player_Movement playerMovement;
    private Rigidbody2D playerRb;
    private int dialogueStartFrame;

    private string[] currentLines;
    private string[] currentSpeakers;
    private int currentLineIndex;
    private bool isTyping = false;
    private bool skipTyping = false;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private AudioClip typingClip;
    private AudioClip interactionClip;

    public bool IsDialogueActive => isDialogueActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Generate a soft dialogue tick sound
        int sampleRate = 44100;
        int sampleLength = (int)(0.025f * sampleRate);
        typingClip = AudioClip.Create("DialogueTick", sampleLength, 1, sampleRate, false);
        float[] samples = new float[sampleLength];
        for (int i = 0; i < sampleLength; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 250f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * 600f * t) * envelope * 0.12f;
        }
        typingClip.SetData(samples, 0);

        // Generate a classic two-tone "menu select" blip (square wave = 8-bit style)
        float clickDuration = 0.14f;
        int clickLength = (int)(clickDuration * sampleRate);
        interactionClip = AudioClip.Create("MenuSelect", clickLength, 1, sampleRate, false);
        float[] click = new float[clickLength];
        for (int i = 0; i < clickLength; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            if (t < 0.055f) // first note (lower)
            {
                float e = Mathf.Exp(-t * 16f);
                sample = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 700f * t)) * e;
            }
            else if (t >= 0.065f) // short gap, then second note (higher)
            {
                float lt = t - 0.065f;
                float e = Mathf.Exp(-lt * 12f);
                sample = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 1050f * t)) * e;
            }

            click[i] = sample * 0.6f; // louder
        }
        interactionClip.SetData(click, 0);

        // Make sure we always have an AudioSource to play through
        // (typing ticks + the NPC interaction sound)
        if (dialogueAudioSource == null)
            dialogueAudioSource = gameObject.AddComponent<AudioSource>();
        dialogueAudioSource.playOnAwake = false;

        // Make the panel clearly stand out from the dark ground
        Image panelImage = dialoguePanel.GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = panelColor;

        // Guarantee a visible "Press E to continue" prompt
        SetupContinuePrompt();

        dialoguePanel.SetActive(false);
    }

    // Creates the continue prompt if it's missing, and forces it into a
    // known-good, on-screen position with a bright colour so it's always readable.
    private void SetupContinuePrompt()
    {
        if (continuePrompt == null)
        {
            GameObject go = new GameObject("ContinuePrompt (Auto)");
            go.transform.SetParent(dialoguePanel.transform, false);
            continuePrompt = go.AddComponent<TextMeshProUGUI>();
        }

        RectTransform rt = continuePrompt.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.offsetMin = new Vector2(20f, 8f);   // left + bottom margin
        rt.offsetMax = new Vector2(-20f, 40f); // right margin, ~32px tall band

        continuePrompt.alignment = TextAlignmentOptions.BottomRight;
        continuePrompt.fontSize = 22f;
        continuePrompt.color = continuePromptColor;
        continuePrompt.raycastTarget = false;
        continuePrompt.text = "";
    }

    public void StartDialogue(string[] speakers, string[] lines)
    {
        if (isDialogueActive) return;

        currentSpeakers = speakers;
        currentLines = lines;
        currentLineIndex = 0;
        isDialogueActive = true;
        dialogueStartFrame = Time.frameCount;

        FreezePlayer(true);

        // Play the NPC interaction sound once when the conversation opens
        if (dialogueAudioSource != null && interactionClip != null)
        {
            dialogueAudioSource.pitch = 1f;
            dialogueAudioSource.PlayOneShot(interactionClip, 1f);
        }

        dialoguePanel.SetActive(true);
        if (continuePrompt != null)
            continuePrompt.text = "";

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        speakerNameText.text = currentSpeakers[currentLineIndex];

        // Color the speaker name based on who is speaking
        string speaker = currentSpeakers[currentLineIndex].ToLower();
        if (speaker.Contains("tammy"))
            speakerNameText.color = new Color(0.91f, 0.84f, 0.64f); // Golden
        else if (speaker.Contains("hunter"))
            speakerNameText.color = new Color(0.6f, 0.75f, 0.55f); // Forest green
        else if (speaker.Contains("merchant"))
            speakerNameText.color = new Color(0.85f, 0.55f, 0.55f); // Warm red
        else if (speaker.Contains("miner"))
            speakerNameText.color = new Color(0.65f, 0.65f, 0.75f); // Slate blue
        else
            speakerNameText.color = Color.white;

        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentLineIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        skipTyping = false;
        dialogueText.text = "";

        if (continuePrompt != null)
            continuePrompt.text = "";

        int i = 0;
        while (i < line.Length)
        {
            if (skipTyping)
            {
                dialogueText.text = line;
                break;
            }

            // Skip rich text tags instantly
            if (line[i] == '<')
            {
                int closingBracket = line.IndexOf('>', i);
                if (closingBracket != -1)
                {
                    dialogueText.text += line.Substring(i, closingBracket - i + 1);
                    i = closingBracket + 1;
                    continue;
                }
            }

            dialogueText.text += line[i];

            if (line[i] != ' ' && line[i] != '\n' && dialogueAudioSource != null)
            {
                dialogueAudioSource.pitch = Random.Range(0.9f, 1.1f);
                dialogueAudioSource.PlayOneShot(typingClip);
            }

            i++;

            // Pause longer on punctuation
            if (line[i - 1] == '.' || line[i - 1] == '!' || line[i - 1] == '?')
                yield return new WaitForSeconds(typeSpeed * 5f);
            else if (line[i - 1] == ',')
                yield return new WaitForSeconds(typeSpeed * 3f);
            else
                yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;

        if (continuePrompt != null)
        {
            if (currentLineIndex < currentLines.Length - 1)
                continuePrompt.text = "Press E to continue...";
            else
                continuePrompt.text = "Press E to close";
        }
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        // Ignore the same E press that opened the dialogue this frame
        if (Time.frameCount == dialogueStartFrame) return;

        bool advancePressed = (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (advancePressed)
        {
            if (isTyping)
            {
                // Skip to end of current line
                skipTyping = true;
            }
            else
            {
                // Advance to next line or close
                currentLineIndex++;
                if (currentLineIndex < currentLines.Length)
                {
                    ShowCurrentLine();
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        currentLines = null;
        currentSpeakers = null;

        FreezePlayer(false);
    }

    // Stops the player from moving while talking. Disables the movement
    // component (which also disables its input actions) and zeroes velocity.
    private void FreezePlayer(bool freeze)
    {
        if (freeze && playerMovement == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<Player_Movement>();
                playerRb = player.GetComponent<Rigidbody2D>();
            }
        }

        if (playerMovement != null)
            playerMovement.enabled = !freeze;

        if (playerRb != null && freeze)
            playerRb.linearVelocity = Vector2.zero;
    }
}
