using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    private Transform player;
    private SpriteRenderer speechBubbleRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speechBubbleRenderer = GetComponent<SpriteRenderer>();
        speechBubbleRenderer.enabled = false; // Hide the speech bubble initially
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Speech bubble appears when player enters the trigger area
            speechBubbleRenderer.enabled = true;

            //Find the player transform
            player = collision.gameObject.GetComponent<Transform>();

            //check to see where the player is and then turn towards them
            if (player.position.x > transform.position.x && transform.parent.localScale.x < 0)
            {
                Flip();
            }

            else if (player.position.x < transform.position.x && transform.parent.localScale.x > 0)   
            {
                Flip();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Hide the speech bubble when the player exits the trigger area
            speechBubbleRenderer.enabled = false;
        }
    }

    private void Flip()
    {
        Vector3 currentScale = transform.parent.localScale;
        currentScale.x *= -1; // Flip the x scale to mirror the sprite
        transform.parent.localScale = currentScale;
    }
}
