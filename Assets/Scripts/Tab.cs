using UnityEngine;

public class Tab : MonoBehaviour
{

    public ReferenceManager referenceManager;

    /// <summary>
    /// Show or hides all buttons that this tab contains.
    /// </summary>
    /// <param name="active"> True if this tab should be shown, false if hidden. </param>
    public void SetTabActive(bool active)
    {
        foreach (Transform sibling in transform)
        {
            // We don't want to change the state of the tab buttons, they should always be turned on. 
            if (ReferenceEquals(sibling.gameObject.GetComponent<TabButton>(), null))
            {
                sibling.gameObject.SetActive(active);
            }
        }
    }
}