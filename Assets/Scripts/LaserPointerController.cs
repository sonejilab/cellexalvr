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

    // Use this for initialization
    void Start()
    {
        frame = 0;
        tempHit = null;
        GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
        layerMaskMenu = 1 << 10;
        layerMaskKeyboard = 1 << 16;
        layerMask = layerMaskMenu | layerMaskKeyboard;
    }
    private void Update()
    {
        frame++;
        if (frame % 5 == 0)
        {
            Frame5Update();
        }
    }

    // Call every 5th frame.
    private void Frame5Update()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask);
        if (hit.collider && (hit.collider.gameObject.CompareTag("Menu Controller Collider") || hit.collider.gameObject.CompareTag("Controller")))
        {
            GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
            transform.localRotation = Quaternion.Euler(15f, 0, 0);
            tempHit = hit.collider.gameObject;
        }
        if (alwaysActive)
        {
            if (!hit.collider || hit.collider.gameObject.CompareTag("Keyboard") || hit.collider.gameObject.CompareTag("PreviousSearchesListNode"))
            {
                GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                transform.localRotation = Quaternion.identity;
            }
        }
        if (!alwaysActive)
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            // When to activate laser. When hitting menu or keyboard.
            if (hit.collider)
            {
                if (hit.collider.gameObject.CompareTag("Keyboard") || hit.collider.gameObject.CompareTag("PreviousSearchesListNode"))
                {
                    GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                    transform.localRotation = Quaternion.identity;
                }
                // When to shut off the laser. Laser still needs to be active even though it hits tool collider layers on controller.
                if (!(hit.collider.gameObject.CompareTag("Menu Controller Collider") || hit.collider.gameObject.CompareTag("Controller")
                    || hit.collider.gameObject.CompareTag("PreviousSearchesListNode") || hit.collider.gameObject.CompareTag("Keyboard")))
                {
                    GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
                }
            }
            if (!hit.collider)
            {
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
