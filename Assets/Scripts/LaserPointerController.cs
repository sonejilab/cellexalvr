using UnityEngine;
using VRTK;
/// <summary>
/// Turns off a laser pointer when the program starts.
/// </summary>
public class LaserPointerController : MonoBehaviour
{
    private int frame;
    private GameObject tempHit;
    private int layerMaskMenu;
    private int layerMaskKeyboard;
    private int layerMask;
    private bool alwaysActive;
    private ControllerModelSwitcher controllerModelSwitcher;

    public ReferenceManager referenceManager;
    // Use this for initialization
    void Start()
    {
        frame = 0;
        tempHit = null;
        GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
        layerMaskMenu = 1 << LayerMask.NameToLayer("MenuLayer");
        layerMaskKeyboard = 1 << LayerMask.NameToLayer("KeyboardLayer");
        layerMask = layerMaskMenu | layerMaskKeyboard;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;

    }
    private void Update()
    {
        frame++;
        if (frame % 3 == 0)
        {
            Frame5Update();
        }
    }

    // Call every 3rd frame.
    private void Frame5Update()
    {
        RaycastHit hit;
        transform.localRotation = Quaternion.Euler(15f, 0, 0);
        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask);
        if (hit.collider)
        {
            tempHit = hit.collider.gameObject;
            if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Menu)
            {
                //controllerModelSwitcher.TurnOffActiveTool(true);
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
            }
        }
        if (alwaysActive)
        {
            if (!hit.collider || hit.collider.gameObject.CompareTag("Keyboard") || hit.collider.gameObject.CompareTag("PreviousSearchesListNode"))
            {
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Keyboard);
            }
        }
        if (!alwaysActive)
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            // When to switch back to previous model. 
            if (!hit.collider)
            {
                if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
                {
                    controllerModelSwitcher.ActivateDesiredTool();
                }
                GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
                if (tempHit && tempHit.GetComponent<CellexalButton>())
                {
                    tempHit.GetComponent<CellexalButton>().controllerInside = false;
                    tempHit.GetComponent<CellexalButton>().SetHighlighted(false);
                }

            }
        }
    }

    // Toggle Laser from laser button. Laser should then be active until toggled off.
    public void ToggleLaser(bool active)
    {
        alwaysActive = active;
        GetComponent<VRTK_StraightPointerRenderer>().enabled = alwaysActive;
        transform.localRotation = Quaternion.identity;
    }
}
