using UnityEngine;
using System.Collections;


///<summary>
/// This class represents a button that rotates when pressed, activated or deactivated.
///</summary>
public abstract class RotatableButton : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public TextMesh descriptionText;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    public SpriteRenderer frontsideRenderer;
    public SpriteRenderer backsideRenderer;
    // all buttons must override this variable's get property
    abstract protected string Description
    {
        get;
    }
    protected SteamVR_Controller.Device device;
    protected bool controllerInside;
    protected bool isRotating = false;

    private Collider buttonCollider;
    protected SteamVR_TrackedObject rightController;
    private bool isActivated = true;
    // the amount of degrees this button has rotated so far when rotating over multiple frames
    private float rotatedTotal;

    protected virtual void Start()
    {
        rightController = referenceManager.rightController;
        device = SteamVR_Controller.Input((int)rightController.index);
        frontsideRenderer.sprite = standardTexture;
        buttonCollider = gameObject.GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            descriptionText.text = Description;
            frontsideRenderer.sprite = highlightedTexture;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            // if the controller has moved directly to another button without
            // exiting this button's collider the other button will have changed the
            // text and we shouldn't mess with it
            if (descriptionText.text == Description)
            {
                descriptionText.text = "";
            }
            frontsideRenderer.sprite = standardTexture;
            controllerInside = false;
        }
    }

    private void Activate(bool activate)
    {
        if (activate)
        {
            backsideRenderer.enabled = false;
            frontsideRenderer.enabled = true;
            buttonCollider.enabled = true;
            frontsideRenderer.sprite = standardTexture;
        }
        else
        {
            backsideRenderer.enabled = true;
            frontsideRenderer.enabled = false;
            buttonCollider.enabled = false;
        }
    }

    public void SetButtonState(bool active)
    {
        if (isActivated != active)
        {
            isActivated = active;
            if (active)
            {
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(FlipButtonRoutine(180f, 0.15f, active));
                }
                else
                {
                    // if the button is not activated we must not start a coroutine, so we just set the values directly
                    transform.Rotate(0, 180f, 0);
                    Activate(true);
                }
            }
            else
            {
                if (gameObject.activeInHierarchy)
                {
                    // no need to keep the description up if we are deactivating the button
                    descriptionText.text = "";
                    StartCoroutine(FlipButtonRoutine(-180f, 0.15f, active));
                }
                else
                {
                    transform.Rotate(0, -180f, 0);
                    Activate(false);
                }
            }
        }
    }

    // if the button is deactivated when rotating, the coroutine will be killed
    void OnDisable()
    {
        if (isRotating)
        {
            if (rotatedTotal < 0)
            {
                transform.Rotate(0, -180 - rotatedTotal, 0);
                Activate(false);
            }
            else
            {
                transform.Rotate(0, 180 - rotatedTotal, 0);
                Activate(true);
            }
            isRotating = false;
        }
    }

    IEnumerator FlipButtonRoutine(float yAngles, float inTime, bool active)
    {
        controllerInside = false;
        buttonCollider.enabled = false;
        frontsideRenderer.enabled = true;
        backsideRenderer.enabled = true;
        isRotating = true;
        // how much we have rotated so far
        rotatedTotal = 0;
        // the absolute value of the rotation angle
        float yAnglesAbs = Mathf.Abs(yAngles);
        // how much we should rotate each frame
        float rotationPerFrame = yAngles / (yAnglesAbs * inTime);

        while (rotatedTotal < yAnglesAbs && rotatedTotal > -yAnglesAbs)
        {
            rotatedTotal += rotationPerFrame;
            // if we are about to rotate it too far
            if (rotatedTotal > yAnglesAbs || rotatedTotal < -yAnglesAbs)
            {
                // only rotate the menu as much as there is left to rotate
                transform.Rotate(0, rotationPerFrame - (rotatedTotal - yAngles), 0);

            }
            else
            {
                transform.Rotate(0, rotationPerFrame, 0);
            }
            yield return null;
        }

        isRotating = false;
        Activate(active);
    }
}
