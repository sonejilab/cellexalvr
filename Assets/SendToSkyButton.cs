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

    void Start()
    {
        networkGenerator = GameObject.Find("NetworkGenerator").GetComponent<NetworkGenerator>();
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
    }

    void Update()
    {
        if (rightController == null)
        {
            //Debug.Log("Find right controller");
            rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
           
        }
        var device = SteamVR_Controller.Input((int)rightController.index);
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





}
