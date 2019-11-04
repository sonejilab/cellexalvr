using UnityEngine;
using CellexalVR.General;


namespace CellexalVR.Multiuser
{
    public class AvatarRightArm : Photon.MonoBehaviour
    {
        #region PUBLIC PROPERTIES
        //public float DirectionDampTime = 5f;
        public Transform target;
        public Transform rightControllerPos;
        #endregion


        #region Private Variables

        #endregion

        #region MONOBEHAVIOUR MESSAGES


        // Use this for initialization
        void Start()
        {
            //rightControllerPos = GameObject.Find("InputReader").GetComponent<ReferenceManager>().rightController.transform;
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

            if (rightControllerPos == null)
            {
                rightControllerPos = GameObject.Find("Controller (right)")?.GetComponent<Transform>();
                return;
            }
            if (!target)
            {
                target = GetComponent<Transform>();
                return;
            }
            target.position = rightControllerPos.position;
            target.rotation = rightControllerPos.rotation;
            target.Rotate(-90, 0, 0);
            // deal with Jumping

            // only allow jumping if we are running.

        }



        #endregion
    }
}
