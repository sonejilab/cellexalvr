using UnityEngine;

/// <summary>
/// This class turns off the keyboard.
/// </summary>
public class KeyboardSwitch : MonoBehaviour
{

    void Start()
    {
        gameObject.SetActive(false);
    }

}
