using System.Collections;
using System.Collections.Generic;
using CellexalVR.General;
using Unity.Mathematics;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.PDFViewer
{
    public class MagnifierCamera : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public Camera magnifyingCamera;
        public GameObject screenQuad;
        public GameObject cubePrefab;

        // private SteamVR_Controller.Device device;
        public SteamVR_Action_Boolean controllerAction = SteamVR_Input.GetBooleanAction("TouchpadPress");
        public Vector2 touchpadPosition;

        private Hand hand;
        private GameObject cube;
        private LayerMask layerMask;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            layerMask = 1 << LayerMask.NameToLayer("PDFLayer");
            hand = Player.instance.rightHand;
        }

        private void Update()
        {
            // int index = (int) referenceManager.rightController.index;
            // device = SteamVR_Controller.Input(index);
            if (controllerAction.GetState(Player.instance.rightHand.handType))
            {
                touchpadPosition = SteamVR_Input.GetVector2("TouchpadPosition", hand.handType);
                if (math.abs(touchpadPosition.y) > 0.5f)
                {
                    Ray ray = new Ray(Player.instance.rightHand.transform.position, Player.instance.rightHand.transform.forward);
                    Magnify(ray);
                }
            }
            
            if (controllerAction.GetStateUp(Player.instance.rightHand.handType))
            {
                magnifyingCamera.gameObject.SetActive(false);
                screenQuad.SetActive(false);
            }

            // Transform rightControllerTransform = referenceManager.rightController.transform;

            // if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            // {
            //     Vector2 touchPad = (device.GetAxis());
            //     if (touchPad.y < 0.3)
            //     {
            //         Ray ray = new Ray(rightControllerTransform.position, rightControllerTransform.forward);
            //         Magnify(ray);
            //     }
            // }
            //
            // if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
            // {
            //     magnifyingCamera.gameObject.SetActive(false);
            //     screenQuad.SetActive(false);
            // }
        }


        private void Magnify(Ray ray)
        {
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
            print("magnify");

            magnifyingCamera.gameObject.SetActive(hit.collider.transform.gameObject.name.Equals("Page(Clone)"));
            screenQuad.SetActive(hit.collider.transform.gameObject.name.Equals("Page(Clone)"));


            float distance = Vector3.Distance(hit.point, ray.origin);

            Vector3 dir = hit.point - ray.origin;

            Vector3 dirNormalized = dir.normalized;

            Vector3 cameraPosition = ray.origin + (0.8f * distance * dirNormalized);

            magnifyingCamera.transform.position = cameraPosition;

            // --- Debug ray ---
            // var lr = GetComponent<LineRenderer>();
            // if (lr == null)
            // {
            //     lr = gameObject.AddComponent<LineRenderer>();
            //     lr.useWorldSpace = true;
            //     lr.positionCount = 2;
            //
            //     lr.startWidth = lr.endWidth = 0.01f;
            // }
            //
            // lr.SetPositions(new Vector3[]
            // {
            //     hit.point,
            //     ray.origin
            // });
            //
            //
            // if (cube == null)
            // {
            //     cube = GameObject.Instantiate(cubePrefab, transform);
            //     cube.SetActive(true);
            //     print("cube created");
            // }
            //
            // cube.transform.position = cameraPosition;


            // Texture2D frame = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false,
            //     true);
            //
            // magnifyingCamera.targetTexture = renderTexture;
        }
    }
}