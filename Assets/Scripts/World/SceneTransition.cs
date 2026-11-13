using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string targetScene;

    [Tooltip("Name of the object to appear next to in the new scene (leave empty for the default spot)")]
    [SerializeField] private string spawnPointName;

    // remembered between scenes so GameManager knows where to put the player
    public static string NextSpawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogWarning("SceneTransition has no target scene.");
            return;
        }

        NextSpawnPoint = spawnPointName;
        SceneManager.LoadScene(targetScene);
    }
}
