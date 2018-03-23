using UnityEngine;

/// <summary>
/// This class turns off the keyboard.
/// </summary>
public class KeyboardSwitch : MonoBehaviour
{

    public bool KeyboardActive { get; set; }

    void Start()
    {
        SetKeyboardVisible(false);
    }

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
