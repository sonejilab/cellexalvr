using UnityEngine;
using UnityEngine.UI;

class DesktopMenu : MonoBehaviour
{

    public GameObject menu;

    private void Start()
    {
        menu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menu.SetActive(!menu.activeSelf);

        }
    }

    /// <summary>
    /// Makes the menu go away.
    /// </summary>
    public void DeactivateMenu()
    {
        menu.SetActive(false);
    }

    /// <summary>
    /// Quits the program.
    /// </summary>
    public void Quit()
    {
        CellExAlLog.Log("Quit button pressed");
        CellExAlLog.LogBacklog();
        // Application.Quit() does not work in the unity editor, only in standalone builds.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

}
