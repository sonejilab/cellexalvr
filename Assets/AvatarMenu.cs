using UnityEngine;
using System.Collections;

public class AvatarMenu : Photon.MonoBehaviour
{
    #region PUBLIC PROPERTIES
    //public float DirectionDampTime = 5f;
    public Transform target;
    public Transform menuPos;
    public GameObject mainMenu;


    #endregion


    #region Private Variables
    private ReferenceManager referenceManager;
    private GameObject menu;
    #endregion

    #region MONOBEHAVIOUR MESSAGES


    // Use this for initialization
    void Start()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        menuPos = referenceManager.mainMenu.transform;
        target = GetComponent<Transform>();
        //mainMenu.SetActive(false);
        if (!target)
        {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }

        //Renderer[] meshList = this.transform.GetComponentsInChildren<Renderer>();
        //foreach (Renderer r in meshList)
        //{
        //    r.enabled = false;
        //}
           
    }


    // Update is called once per frame
    void Update()
    {
        if (menuPos == null)
        {
            menuPos = GameObject.Find("Main Menu").GetComponent<Transform>();

        }
        if (photonView.isMine == false)
        {
            Debug.Log("NOT MY MENU");
            if (referenceManager.gameManager.avatarMenuActive)
            {
                Debug.Log("TOGGLE ON");
                mainMenu.SetActive(true);
                //if (menu == null)
                //{
                //    menu = Instantiate(mainMenu, Vector3.zero, Quaternion.identity);
                //}
                //Renderer[] meshList = this.transform.GetComponentsInChildren<Renderer>();
                //foreach (Renderer r in meshList)
                //{
                //    r.enabled = true;
                //}
            }
            if (!referenceManager.gameManager.avatarMenuActive)
            {
                Debug.Log("TOGGLE OFF");
                mainMenu.SetActive(false);
                //if (menu != null)
                //{
                //    Destroy(menu);
                //}
                //Renderer[] meshList = this.transform.GetComponentsInChildren<Renderer>();
                //foreach (Renderer r in meshList)
                //{
                //    r.enabled = false;
                //}
            }
        }

        if (photonView.isMine == false && PhotonNetwork.connected == true)
        {
            return;
        }
        //if (!target)
        //{
        //    target = GetComponent<Transform>();
        //    return;
        //}

        mainMenu.transform.position = menuPos.position;
        mainMenu.transform.rotation = menuPos.rotation;
        //target.Rotate(0, 0, 0);
        // deal with Jumping

        // only allow jumping if we are running.

    }


    #endregion
}

