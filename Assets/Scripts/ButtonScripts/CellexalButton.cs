using UnityEngine;
using VRTK;

/// <summary>
/// Abstract general purpose class that represents a button on the menu.
/// </summary>
public abstract class CellexalButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public TextMesh descriptionText;
    public GameObject infoMenu;

    private int frameCount;
    private string laserColliderName = "[RightController]BasePointerRenderer_ObjectInteractor_Collider";
    // all buttons must override this variable's get property
    /// <summary>
    /// A string that briefly explains what this button does.
    /// </summary>
    abstract protected string Description
    {
        get;
    }

    // These are drawn in the inspector through CellexalButtonEditor.cs
    [HideInInspector]
    public Color meshStandardColor = Color.black;
    [HideInInspector]
    public Color meshHighlightColor = Color.white;
    [HideInInspector]
    public Color meshDeactivatedColor = Color.grey;
    [HideInInspector]
    public Sprite standardTexture = null;
    [HideInInspector]
    public Sprite highlightedTexture = null;
    [HideInInspector]
    public Sprite deactivatedTexture = null;
    [HideInInspector]
    public int popupChoice = 0;

    protected SteamVR_TrackedObject rightController;
    protected SteamVR_Controller.Device device;
    protected SpriteRenderer spriteRenderer;
    protected MeshRenderer meshRenderer;
    [HideInInspector]
    public bool buttonActivated = true;
    public bool controllerInside = false;
    private Transform raycastingSource;
    private int layerMaskNetwork;
    private int layerMaskGraph;
    private int layerMaskMenu;
    private int layerMaskKeyboard;
    private int layerMask;


    protected virtual void Awake()
    {
        if (referenceManager == null)
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
        rightController = referenceManager.rightController;
        device = SteamVR_Controller.Input((int)rightController.index);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        //this.tag = "Menu Controller Collider";
        layerMaskNetwork = LayerMask.NameToLayer("NetworkLayer");
        layerMaskKeyboard = 1 << LayerMask.NameToLayer("KeyboardLayer");
        layerMaskMenu = 1 << LayerMask.NameToLayer("MenuLayer");
        layerMask = layerMaskMenu | layerMaskKeyboard;

    }

    protected virtual void Update()
    {
        frameCount++;
        CheckForClick();
        CheckForHit();
    }

    private void CheckForClick()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Click();
        }
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y < 0.5f)
        {
            HelpClick();
        }
    }

    /// <summary>
    /// Button sometimes stays active even though ontriggerexit should have been called.
    /// To deactivate button again check every 10th frame if laser pointer collider is colliding.
    /// </summary>
    private void CheckForHit()
    {
        if (!buttonActivated) return;
        if (frameCount % 10 == 0)
        {
            bool inside = false;
            RaycastHit hit;
            raycastingSource = referenceManager.rightLaser.transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit, 10, layerMask);
            //if (hit.collider) print(hit.collider.transform.gameObject.name);
            if (hit.collider && hit.collider.transform == transform && referenceManager.rightLaser.GetComponent<VRTK_StraightPointerRenderer>().enabled && buttonActivated)
            {
                inside = true;
                frameCount = 0;
                controllerInside = inside;
                SetHighlighted(inside);
                //if (infoMenu) infoMenu.SetActive(inside);
                return;
            }
            if (!(hit.collider || hit.transform == transform))
            {
                inside = false;
                controllerInside = inside;
                SetHighlighted(inside);
                //if (infoMenu) infoMenu.SetActive(inside);
            }
            controllerInside = inside;
            SetHighlighted(inside);
            if (descriptionText.text == Description)
            {
                descriptionText.text = "";
            }
            frameCount = 0;
        }
    }

    protected abstract void Click();

    protected virtual void HelpClick()
    {
        if (!infoMenu) return;

        infoMenu.GetComponent<VideoButton>().StartVideo();
    }

    public virtual void SetButtonActivated(bool activate)
    {
        //print(name + " setbuttonactivated");
        if (!activate)
        {
            descriptionText.text = "";
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = deactivatedTexture;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = meshDeactivatedColor;
            }
        }
        if (activate)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = standardTexture;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = meshStandardColor;
            }
        }
        buttonActivated = activate;
        controllerInside = false;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (!buttonActivated) return;
        //print(name + " ontriggerenter");
        if (other.gameObject.name == laserColliderName)
        {
            descriptionText.text = Description;
            controllerInside = true;
            SetHighlighted(true);
        }
    }

    // In case OnTriggerExit doesnt get called by laser pointer we need to manually do the unhighlighting.
    protected void Exit()
    {
        //print(name + " exit");
        if (descriptionText.text == Description)
        {
            descriptionText.text = "";
        }
        controllerInside = false;
        if (buttonActivated)
        {
            SetHighlighted(false);
        }
        else
        {
            SetButtonActivated(false);
        }
        if (infoMenu)
        {
            infoMenu.SetActive(false);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (!buttonActivated) return;
        //print(name + " ontriggerexit");
        if (other.gameObject.name == laserColliderName)
        {
            if (descriptionText.text == Description)
            {
                descriptionText.text = "";
            }
            controllerInside = false;
            SetHighlighted(false);
            //if (infoMenu && !infoMenu.GetComponent<InfoMenu>().active)
            //{
            //    infoMenu.SetActive(false);
            //}
        }
    }

    public virtual void SetHighlighted(bool highlight)
    {
        if (highlight)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = highlightedTexture;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = meshHighlightColor;
            }
        }
        if (!highlight)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = standardTexture;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = meshStandardColor;
            }
        }
        if (infoMenu)
        {
            infoMenu.SetActive(highlight);
        }
        controllerInside = highlight;
    }
}
