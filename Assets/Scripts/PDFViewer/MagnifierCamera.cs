using CellexalVR.General;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CellexalVR.PDFViewer
{
    public class MagnifierCamera : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public Camera magnifyingCamera;
        public GameObject screenQuad;
        public GameObject cubePrefab;
        private RenderTexture renderTexture;

        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private InputActionReference click;
        [SerializeField] private InputActionReference touchPadPos;

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
            layerMask = 1 << LayerMask.NameToLayer("EnvironmentButtonLayer");
            click.action.performed += OnClick;
            click.action.canceled += OnRelease;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            print($"click {math.abs(touchPadPos.action.ReadValue<Vector2>().y) > 0.5f}");
            if (math.abs(touchPadPos.action.ReadValue<Vector2>().y) > 0.5f)
            {
                Ray ray = new Ray(ReferenceManager.instance.rightController.transform.position, ReferenceManager.instance.rightController.transform.forward);
                Magnify(ray);
            }
        }

        private void OnRelease(InputAction.CallbackContext context)
        {
            magnifyingCamera.gameObject.SetActive(false);
            screenQuad.SetActive(false);
        }



        private void Magnify(Ray ray)
        {
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;

            //magnifyingCamera.gameObject.SetActive(hit.collider.transform.gameObject.name.Equals("Page(Clone)"));
            //screenQuad.SetActive(hit.collider.transform.gameObject.name.Equals("Page(Clone)"));
            bool hitImage = hit.collider.CompareTag("GeoMXImage");
            magnifyingCamera.gameObject.SetActive(hitImage);
            screenQuad.SetActive(hitImage);

            float distance = Vector3.Distance(hit.point, ray.origin);

            Vector3 dir = hit.point - ray.origin;

            Vector3 dirNormalized = dir.normalized;

            Vector3 cameraPosition = ray.origin + (0.9f * distance * dirNormalized);

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


            //Texture2D frame = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false,
            //    true);


        }
    }
}