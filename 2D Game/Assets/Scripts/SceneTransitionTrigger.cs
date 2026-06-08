using UnityEngine;
using UnityEngine.SceneManagement;

// Place on an empty GameObject with a trigger Collider2D at a scene edge.
// When the player walks in, it fades out and loads the target scene, then
// places the player at the named spawn point in that scene.
[RequireComponent(typeof(Collider2D))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetScene = "SCENE 2 - INSIDE MINE";

    [Tooltip("Name of the spawn-point GameObject in the TARGET scene to drop the player at.")]
    [SerializeField] private string targetSpawnPointName = "";

    [Tooltip("Ignore the player for this long after the scene loads, so we don't " +
             "instantly re-trigger when the player spawns on top of a trigger.")]
    [SerializeField] private float armDelay = 0.4f;

    private float armedAt;
    private bool used = false;

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
        armedAt = Time.unscaledTime + armDelay;
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
        if (used) return;
        if (Time.unscaledTime < armedAt) return;      // still settling after spawn
        if (!collision.CompareTag("Player")) return;

        used = true;

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(targetScene, targetSpawnPointName);
        else
            SceneManager.LoadScene(targetScene); // fallback if no fader present
    }

    // Draw the trigger area in the editor so it's easy to place
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.35f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
