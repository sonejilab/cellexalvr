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
    private int layerMaskGraph;
    private int layerMaskNetwork;
    private int layerMaskOther;
    private bool alwaysActive;
    private bool keyboardActive;
    private ControllerModelSwitcher controllerModelSwitcher;

    public ReferenceManager referenceManager;
    public GameObject panel;
    // Use this for initialization
    void Start()
    {
        frame = 0;
        tempHit = null;
        GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
        layerMaskMenu = 1 << LayerMask.NameToLayer("MenuLayer");
        layerMaskKeyboard = 1 << LayerMask.NameToLayer("KeyboardLayer");
        layerMaskGraph = 1 << LayerMask.NameToLayer("GraphLayer");
        layerMaskNetwork = 1 << LayerMask.NameToLayer("NetworkLayer");
        //layerMaskOther = layerMaskKeyboard | layerMaskSel;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;

    }
    private void Update()
    {
        frame++;
        if (frame % 5 == 0)
        {
            Frame5Update();
        }
    }

    // Call every 3rd frame.
    private void Frame5Update()
    {
        RaycastHit hit;
        transform.localRotation = Quaternion.Euler(15f, 0, 0);
        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10, layerMaskMenu);
        if (hit.collider)
        {
            tempHit = hit.collider.gameObject;
            if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Menu)
            {
                //controllerModelSwitcher.TurnOffActiveTool(true);
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
                panel.transform.localPosition = new Vector3(0.0f, 0.0f, 0.01f);
                panel.transform.localRotation = Quaternion.Euler(-15, 5, 1);
            }
        }
        if (!hit.collider && alwaysActive)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10, layerMaskKeyboard);
            if (hit.collider && (hit.collider.gameObject.CompareTag("Keyboard") || hit.collider.gameObject.CompareTag("PreviousSearchesListNode")))
            {
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Keyboard);
            }
            else
            {
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.TwoLasers);
                panel.transform.localPosition = new Vector3(0, 0, 0);
                panel.transform.localRotation = Quaternion.Euler(0, 5, 1);
            }
        }
        if (!alwaysActive)
        {
            // When to switch back to previous model. 
            if (!hit.collider)
            {
                if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
                {
                    controllerModelSwitcher.ActivateDesiredTool();
                }
                GetComponent<VRTK_StraightPointerRenderer>().enabled = false;


            }
        }
    }

    // Toggle Laser from laser button. Laser should then be active until toggled off.
    public void ToggleLaser(bool active)
    {
        alwaysActive = active;
        GetComponent<VRTK_StraightPointerRenderer>().enabled = alwaysActive;
        transform.localRotation = Quaternion.identity;
        //if (alwaysActive)
        //{
        //    layerMask = layerMaskMenu | layerMaskKeyboard | layerMaskGraph | layerMaskNetwork;
        //}
        //if (!alwaysActive)
        //{
        //    layerMask = layerMaskMenu;
        //}
    }
}
