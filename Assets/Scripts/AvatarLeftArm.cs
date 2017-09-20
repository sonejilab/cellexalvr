using UnityEngine;
using System.Collections;


namespace Com.MyCompany.MyGame
{
    public class AvatarLeftArm : Photon.MonoBehaviour
    {
        #region PUBLIC PROPERTIES
        //public float DirectionDampTime = 5f;
        public Transform target;
        public Transform leftControllerPos;
        
        
        #endregion


        #region Private Variables

        #endregion

        #region MONOBEHAVIOUR MESSAGES


        // Use this for initialization
        void Start()
        {
            leftControllerPos = GameObject.Find("Controller (left)").GetComponent<Transform>();
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
                leftControllerPos = GameObject.Find("Controller (left)").GetComponent<Transform>();

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
