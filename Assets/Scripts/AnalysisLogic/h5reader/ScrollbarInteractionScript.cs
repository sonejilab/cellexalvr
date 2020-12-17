using CellexalVR.General;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.AnalysisLogic.H5reader
{
    public class ScrollbarInteractionScript : MonoBehaviour
    {
        private bool controllerInside;
        public RectTransform handleRect;
        public RectTransform slidingArea;
        public Scrollbar scrollbar;
        public BoxCollider boxCollider;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                controllerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                controllerInside = false;
            }
        }

        private void Update()
        {
            if (controllerInside && Player.instance.rightHand.grabPinchAction.GetStateDown(Player.instance.rightHand.handType))
            {
                float temp;
                //Horizontal
                if (slidingArea.rect.height > slidingArea.rect.width)
                {
                    float y = 0;// slidingArea.transform.InverseTransformPoint(device.transform.pos).y;
                    float height = slidingArea.rect.height;
                    temp = Mathf.Clamp01(y / height + 0.5f);
                }
                else
                {
                    float x = 0;// slidingArea.transform.InverseTransformPoint(device.transform.pos).x;
                    float width = slidingArea.rect.width;
                    temp = Mathf.Clamp01(x / width + 0.5f);
                }


                scrollbar.value = temp;
            }
            Vector3 size = handleRect.rect.size;
            size.z = 10;
            boxCollider.size = size;


        }

    }
}


