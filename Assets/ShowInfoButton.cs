using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

public class ShowInfoButton : VRTK_InteractableObject
{

    public SpriteRenderer spriteRenderer;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    //public Sprite gray;
    //public Sprite original;
    public GameObject canvas;
    private SteamVR_Controller.Device device;
    private bool controllerInside;

    void Start()
    {
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)rightController.index);
    }

    /*void Update()
    {
        if (rightController == null)
        {
            Debug.Log("Find right controller");
            rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();

        }
        if (device == null)
        {
            device = SteamVR_Controller.Input((int)rightController.index);
        }

        // handle input
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            controllerInside = false;
            canvas.SetActive(!canvas.activeSelf);
            Debug.Log("Show canvas");
        }

        
    }*/

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            spriteRenderer.sprite = highlightedTexture;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
        }
    }
    protected override void Awake()
    {
        base.Awake();
    }

    public override void StartTouching(GameObject currentTouchingObject)
    {
        this.spriteRenderer.sprite = highlightedTexture;
        //Debug.Log("TOUCHING INFO");
        base.StartTouching(currentTouchingObject);
    }

    public override void StopTouching(GameObject previousTouchingObject)
    {
        base.StopTouching(previousTouchingObject);
        this.spriteRenderer.sprite = standardTexture;
    }

    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);
        canvas.SetActive(!canvas.activeSelf);
        //Debug.Log("USE INFO");
        //print("using " + node.Label);
    }




}
