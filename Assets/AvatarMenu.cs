using UnityEngine;
using System.Collections;


namespace Com.MyCompany.MyGame
{
    public class AvatarMenu : Photon.MonoBehaviour
    {
        #region PUBLIC PROPERTIES
        //public float DirectionDampTime = 5f;
        public Transform target;
        public Transform menuPos;


        #endregion


        #region Private Variables
        private ReferenceManager referenceManager;
        #endregion

        #region MONOBEHAVIOUR MESSAGES


        // Use this for initialization
        void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            menuPos = referenceManager.mainMenu.transform;
            target = GetComponent<Transform>();
            if (!target)
            {
                Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
            }

            if (photonView.isMine)
            {
                Renderer[] meshList = this.transform.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in meshList)
                {
                    r.enabled = false;
                }
            }
        }


        // Update is called once per frame
        void Update()
        {

            if (photonView.isMine == false && PhotonNetwork.connected == true)
            {
                return;
            }
            if (!target)
            {
                target = GetComponent<Transform>();
                return;
            }

            if (menuPos == null)
            {
                menuPos = GameObject.Find("Main Menu").GetComponent<Transform>();

            }
            if (!photonView.isMine)
            {
                if (referenceManager.menuToggler.MenuActive)
                {
                    Renderer[] meshList = this.transform.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in meshList)
                    {
                        r.enabled = true;
                    }
                }
                if (!referenceManager.menuToggler.MenuActive)
                {
                    Renderer[] meshList = this.transform.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in meshList)
                    {
                        r.enabled = false;
                    }
                }
            }


            target.position = menuPos.position;
            target.rotation = menuPos.rotation;
            //target.Rotate(0, 0, 0);
            // deal with Jumping

            // only allow jumping if we are running.

        }


        #endregion
    }
}
