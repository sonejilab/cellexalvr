using UnityEngine;
using CellexalVR.General;

namespace CellexalVR.Multiplayer
{

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

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

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
                if (referenceManager.gameManager.avatarMenuActive)
                {
                    mainMenu.SetActive(true);
                }
                if (!referenceManager.gameManager.avatarMenuActive)
                {
                    mainMenu.SetActive(false);
                }
                if (!referenceManager.gameManager.avatarMenuActive)
                {
                    mainMenu.SetActive(false);
                }
            }

            if (!mainMenu)
            {
                return;
            }
            if (!photonView.isMine && PhotonNetwork.connected == true)
            {
                return;
            }
            print("ROTATE");
            mainMenu.transform.position = menuPos.position;
            mainMenu.transform.Rotate(90, 0, 0);
            //mainMenu.transform.localRotation = Quaternion.Euler(90, 0, 0);


        }


        #endregion
    }

}