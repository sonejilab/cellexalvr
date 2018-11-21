using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class GetBackFromSkyButton : VRTK_InteractableObject {

    public SpriteRenderer spriteRenderer;
    public SendToSky send;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    //public Sprite gray;
    //public Sprite original;
    private NetworkGenerator networkGenerator;
    private SteamVR_Controller.Device device;
    private bool controllerInside;
    public GameObject sendToSkyButton;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        networkGenerator = GameObject.Find("NetworkGenerator").GetComponent<NetworkGenerator>();
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)rightController.index);
    }

    public override void StartTouching(GameObject currentTouchingObject)
    {
        // Debug.Log("TOUCHING");
        this.spriteRenderer.sprite = highlightedTexture;
        base.StartTouching(currentTouchingObject);
    }

    public override void StopTouching(GameObject previousTouchingObject)
    {
        base.StopTouching(previousTouchingObject);
        this.spriteRenderer.sprite = standardTexture;
    }

    public override void StartUsing(GameObject currentUsingObject)
    {
        //Debug.Log("GET BACK HERE");
        base.StartUsing(currentUsingObject);
        send.GetBackFromSky();
        sendToSkyButton.SetActive(true);
        this.gameObject.SetActive(false);
        //print("using " + node.Label);
    }
}
