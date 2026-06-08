using UnityEngine;

// Plays a looping walk sound while the player is moving on the ground.
// Self-contained: lives on the Player and reads the Rigidbody2D velocity,
// so it doesn't require any changes to the movement script.
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip walkClip;

    [Header("Optional grounded check")]
    [Tooltip("Drag the same GroundCheck transform the movement script uses. " +
             "If left empty, footsteps play whenever moving horizontally.")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Tuning")]
    [Tooltip("Minimum horizontal speed before footsteps start.")]
    [SerializeField] private float moveThreshold = 0.5f;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        footstepSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (footstepSource == null) footstepSource = GetComponent<AudioSource>();

        if (footstepSource != null)
        {
            footstepSource.clip = walkClip;
            footstepSource.loop = true;
            footstepSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (rb == null || footstepSource == null || walkClip == null) return;

        bool moving = Mathf.Abs(rb.linearVelocity.x) > moveThreshold;
        bool grounded = IsGrounded();

        bool shouldPlay = moving && grounded;

        // Stay silent while talking (player is frozen during dialogue)
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            shouldPlay = false;

        // Stay silent while paused
        if (Time.timeScale == 0f)
            shouldPlay = false;

        if (shouldPlay)
        {
            if (!footstepSource.isPlaying)
                footstepSource.Play();
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }
    }

    private bool IsGrounded()
    {
        // No ground check assigned -> assume grounded so footsteps still work.
        if (groundCheck == null) return true;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}
