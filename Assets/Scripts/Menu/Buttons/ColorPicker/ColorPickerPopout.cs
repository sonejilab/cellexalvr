using CellexalVR.General;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Menu.Buttons.ColorPicker
{
    public class ColorPickerPopout : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject satvalBox;
        public GameObject satvalMarker;
        public GameObject hueSlider;
        public GameObject hueMarker;

        private GameObject rightController;
        private Vector3 satValBoxLowerLeft = new Vector3(-0.46f, 0f, -0.14f);
        private Vector3 satValBoxUpperRight = new Vector3(0.14f, 0f, 0.46f);
        private Vector3 huesliderBoxLowerLeft = new Vector3(0.16f, 0f, -0.14f);
        private Vector3 huesliderBoxUpperRight = new Vector3(0.24f, 0f, 0.46f);



        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            rightController = referenceManager.rightController.gameObject.gameObject;
        }

        private void Update()
        {
            Raycast();
        }

        public void Raycast()
        {
            RaycastHit hit;
            if (Physics.Raycast(rightController.transform.position, rightController.transform.forward, out hit, 25f))
            {
                // the raycast hit in local coordinates
                Vector3 hitCoord = transform.InverseTransformPoint(hit.point);
                if (CoordInsideBox(hitCoord, satValBoxLowerLeft, satValBoxUpperRight))
                {
                    // hit sat/val box
                    satvalMarker.transform.localPosition = hitCoord;
                }
                else if (CoordInsideBox(hitCoord, huesliderBoxLowerLeft, huesliderBoxUpperRight))
                {
                    // hit hue slider
                    hueMarker.transform.localPosition = hitCoord;
                }
            }
        }

        private bool CoordInsideBox(Vector3 coord, Vector3 lowerLeft, Vector3 upperRight)
        {
            return coord.x >= lowerLeft.x && coord.y >= lowerLeft.y && coord.x <= upperRight.x && coord.y <= upperRight.y;
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

    }
}
