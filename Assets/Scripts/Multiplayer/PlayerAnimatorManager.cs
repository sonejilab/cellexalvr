using UnityEngine;
using System.Collections;


namespace Com.MyCompany.MyGame
{
    public class PlayerAnimatorManager : Photon.MonoBehaviour
    {
        #region PUBLIC PROPERTIES
        //public float DirectionDampTime = 5f;
        public Transform target;
        public Transform cameraPos;
        public Transform menu;
        public Transform menuTarget;

        private GraphPoint selectedPoint;
        #endregion


        #region Private Variables

        #endregion

        #region MONOBEHAVIOUR MESSAGES


        // Use this for initialization
        void Start()
        {
            cameraPos = GameObject.Find("Camera (eye)").GetComponent<Transform>();
            target = GetComponent<Transform>();
            menu = GameObject.Find("Main Menu").GetComponent<Transform>();

            if (!target)
            {
                Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
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
                return;
            }

			if (cameraPos == null) {
				cameraPos = GameObject.Find ("Camera (eye)").GetComponent<Transform> ();

			}

            target.position = cameraPos.position;
            target.rotation = cameraPos.rotation;
            target.Rotate(90, 0, 0);

            //menuTarget.position = menu.position;
            //menuTarget.rotation = menu.rotation;
            //menuTarget.Rotate(90, 0, 0);

        }


        #endregion
    }
}
