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
    [SerializeField] private float textSpeed = 0.03f;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "SCENE 1 - CABIN TO MINE";

    [TextArea(10, 20)]
    [SerializeField] private string prologueContent =
        "Tammy is a tenacious and independent young girl. " +
        "She lives with her cat, KitKat, in the lodge she grew up in the Blackridge Forest.\n\n" +
        "KitKat has been by her side since she was a little girl. " +
        "Tammy would do anything for KitKat.\n\n" +
        "One night, mysteriously on the same night her parents went missing a year ago, " +
        "she awoke to find KitKat missing.\n\n" +
        "Afraid but determined to find her best friend, " +
        "Tammy is preparing to venture out of her home into the deep woods.\n\n" +
        "She remembers the story her parents told her of Blackridge Forest...\n\n" +
        "A dark and mystic space, creatures ready to chase.\n" +
        "Few friends to find in the dark, only the strong-willed should embark.\n" +
        "Enter at your own pace, for there are dangers within this place.";

    private bool isTyping = false;
    private bool skipTyping = false;

    private void Start()
    {
        continueButton.gameObject.SetActive(false);
        continueButton.onClick.AddListener(LoadNextScene);
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        prologueText.text = "";

        foreach (char c in prologueContent)
        {
            if (skipTyping)
            {
                prologueText.text = prologueContent;
                break;
            }

            prologueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        continueButton.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (isTyping && (Input.GetMouseButtonDown(0) || Input.anyKeyDown))
        {
            skipTyping = true;
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
