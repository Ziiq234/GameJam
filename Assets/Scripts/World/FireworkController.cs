using UnityEngine;

public class FireworkController : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] fireworks;

    public void PlayFireworks()
    {
        foreach (ParticleSystem firework in fireworks)
        {
            if (firework != null)
            {
                firework.Play();
            }
        }
    }
}