using UnityEngine;
using System.Collections;


namespace CellexalVR.Interaction
{

    public class MoveObjectOneAxis : MonoBehaviour
    {
        public Transform objectToMove;

        private InteractableObjectOneAxis interactableObject;
        private Vector3 startPosition;
        private int axisToMove;

        private void Start()
        {
            interactableObject = GetComponent<InteractableObjectOneAxis>();
            startPosition = new Vector3(transform.position.x , transform.position.y, transform.position.z);
            axisToMove = (int)interactableObject.movableAxis;
        }

        private void Update()
        {
            if (interactableObject.isGrabbed)
            {
                MoveObject();
            }
        }

        private void MoveObject()
        {
            //Vector3 diff = transform.position - startPosition;
            float diff = transform.position[axisToMove] - startPosition[axisToMove];
            print($"diff : {diff}");
            Vector3 pos = objectToMove.transform.localPosition;
            pos[axisToMove] = diff;
            objectToMove.transform.localPosition = pos;
        }
    }

}