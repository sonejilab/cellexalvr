using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

public class SendToSkyButton : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SendToSky send;
    public SteamVR_TrackedObject rightController;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    public GameObject getBackFromSkyButton;
    //public Sprite gray;
    //public Sprite original;
    private NetworkGenerator networkGenerator;
    private bool controllerInside;
    private SteamVR_Controller.Device device;

    void Start()
    {
        networkGenerator = GameObject.Find("NetworkGenerator").GetComponent<NetworkGenerator>();
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)rightController.index);
    }

    void Update()
    {
        if (rightController == null)
        {

            var controllerGameObject = GameObject.Find("Controller (right)");
            if (controllerGameObject)
                rightController = controllerGameObject.GetComponent<SteamVR_TrackedObject>();
            else
                return;

        }

        device = SteamVR_Controller.Input((int)rightController.index);

        // handle input
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            controllerInside = false;
            Debug.Log("DO SEND TO SKY");
            this.gameObject.SetActive(false);
            getBackFromSkyButton.SetActive(true);
            if (send.GetComponent<Transform>().name == "Enlarged Network")
            {
                send.DoSendToSky(networkGenerator.objectsInSky, 0);
            }
            else
            {
                send.DoSendToSky(networkGenerator.objectsInSky, 1);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            spriteRenderer.sprite = highlightedTexture;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
        }
    }





}
