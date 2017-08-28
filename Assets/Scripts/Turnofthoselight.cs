using UnityEngine;

/// <summary>
/// This class turns of those pesky lights that are useful while in the editor.
/// </summary>
public class Turnofthoselight : MonoBehaviour
{

    private void Start()
    {
        gameObject.SetActive(false);
    }
}