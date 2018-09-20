using UnityEngine;

/// <summary>
/// This class turns off the keyboard.
/// </summary>
public class KeyboardSwitch : MonoBehaviour
{

    public bool KeyboardActive { get; set; }

    void Start()
    {
        SetKeyboardVisible(true);
    }

    /// <summary>
    /// Sets the keyboard to be either visible or invisible.
    /// </summary>
    /// <param name="visible">True if the keyboard should be visible, false for invisible.</param>
    public void SetKeyboardVisible(bool visible)
    {
        KeyboardActive = visible;
        foreach (Transform t in transform)
        {
            if (t.gameObject.GetComponent<AutoCompleteList>())
            {
                foreach (Transform tt in t)
                {
                    tt.gameObject.SetActive(visible);
                }
            }
            else
            {
                t.gameObject.SetActive(visible);
            }
        }
    }

}
