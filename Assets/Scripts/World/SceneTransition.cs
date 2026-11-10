using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string targetScene;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogWarning("SceneTransition has no target scene.");
            return;
        }

        SceneManager.LoadScene(targetScene);
    }
}