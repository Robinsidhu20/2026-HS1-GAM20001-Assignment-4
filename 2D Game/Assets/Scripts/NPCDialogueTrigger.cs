using UnityEngine;
using UnityEngine.InputSystem;

public class NPCDialogueTrigger : MonoBehaviour
{
    public enum NPCType { Hunter, Merchant, Miners }

    [SerializeField] private NPCType npcType;

    private bool playerInRange = false;
    private bool hasSpoken = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive)
            {
                StartDialogue();
            }
        }
    }

    private void StartDialogue()
    {
        string[] speakers;
        string[] lines;

        switch (npcType)
        {
            case NPCType.Hunter:
                speakers = new string[]
                {
                    "Tammy",
                    "Hunter",
                    "Hunter",
                    "Tammy"
                };
                lines = new string[]
                {
                    "Excuse me... have you seen my cat? She's small, and—",
                    "<i>*humph?*</i>",
                    "Yer cat? Ain't seen no cat, ain't even seen no small critter for hours...\n<i>*descends into strange mumblings*</i>",
                    "Oh ok..."
                };
                break;

            case NPCType.Merchant:
                speakers = new string[]
                {
                    "Tammy",
                    "Merchant",
                    "Merchant",
                    "Merchant",
                    "Merchant",
                    "Tammy"
                };
                lines = new string[]
                {
                    "Hello! I'm looking for my cat. Have you seen her anywhere?",
                    "You're the smallest customer I've had yet!",
                    "A cat! Where!? Mountain lions are selling at a high price this time of year.",
                    "Oh, a <i>house cat</i>... well nothing gets past these hunters, too high-strung, blasting off at the slightest rustle.",
                    "Welp if you're not getting anything, move along!",
                    "Oh ok..."
                };
                break;

            case NPCType.Miners:
                speakers = new string[]
                {
                    "Tammy",
                    "",
                    "",
                    "Miner 1",
                    "Miner 2",
                    "Miner 1",
                    "Miner 2",
                    "Miner 1",
                    "Miner 2",
                    "Miner 1",
                    "Miner 2",
                    "Tammy"
                };
                lines = new string[]
                {
                    "Um... have either of you seen a cat wander through here?",
                    "<i>*Miner 1 glares at Tammy, grumbling*</i>",
                    "<i>*Miner 2 burps, its wet and bubbly*</i>",
                    "I ain't ever liked them creatures.",
                    "<i>*hick-*</i>",
                    "You know they see dead people?",
                    "Yer... all nine lives.",
                    "Maybe its in there, lotta dead people in them tunnels.",
                    "Jerry died last week... <i>*bletch*</i>",
                    "Go have a gander if ya want. He'll be back in ten.",
                    "Do ya have another drink?",
                    "Oh ok..."
                };
                break;

            default:
                speakers = new string[] { "???" };
                lines = new string[] { "..." };
                break;
        }

        DialogueManager.Instance.StartDialogue(speakers, lines);
    }
}
