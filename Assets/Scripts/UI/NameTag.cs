using UnityEngine;

// Shows a small label above an object, like "Forest Dungeon" over a cave.
public class NameTag : MonoBehaviour
{
    [SerializeField] private string label = "Name";

    private void OnGUI()
    {
        UIHelper.DrawNameTag(transform.position, label);
    }
}
