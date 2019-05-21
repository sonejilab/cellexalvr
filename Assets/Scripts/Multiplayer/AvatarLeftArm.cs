using UnityEngine;
using System.Collections;
using CellexalVR.General;

namespace CellexalVR.Multiplayer
{
    public class AvatarLeftArm : Photon.MonoBehaviour
    {
        #region PUBLIC PROPERTIES
        //public float DirectionDampTime = 5f;
        public Transform target;
        public Transform leftControllerPos;


        #endregion


        #region Private Variables
        public ReferenceManager referenceManager;
        #endregion

        #region MONOBEHAVIOUR MESSAGES


        // Use this for initialization
        void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            leftControllerPos = referenceManager.leftController.transform;
            target = GetComponent<Transform>();
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
                target = GetComponent<Transform>();
                return;
            }

            if (leftControllerPos == null)
            {
                leftControllerPos = referenceManager.leftController.transform;

            }

            target.position = leftControllerPos.position;
            target.rotation = leftControllerPos.rotation;
            target.Rotate(-90, 0, 0);
            // deal with Jumping

            // only allow jumping if we are running.

        }


        #endregion
    }
}
