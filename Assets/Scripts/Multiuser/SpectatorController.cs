using CellexalVR.General;
using UnityEngine;
using UnityEngine.XR;

namespace CellexalVR.Multiuser
{
    /// <summary>
    /// Handles movement of the user when in spectator mode. Active e.g. if the user does not have a vr headset and wishes to use the keyboard instead.
    /// </summary>
    public class SpectatorController : MonoBehaviour
    {
        public float speed = 1f;
        public GameObject CtrlsCanvas;
        public GameObject TextCanvas;
        public GameObject avatar;
        private bool active;

        public ReferenceManager referenceManager;
        public Camera spectatorCamera;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            //ToggleSpectator(false);
            //foreach (Canvas c in settingsMenu.GetComponentsInChildren<Canvas>())
            //{
            //    c.renderMode = RenderMode.ScreenSpaceCamera;
            //    c.worldCamera = GetComponentInChildren<Camera>();
            //}
            //foreach (Canvas c in console.GetComponentsInChildren<Canvas>())
            //{
            //    c.renderMode = RenderMode.ScreenSpaceCamera;
            //    c.worldCamera = GetComponentInChildren<Camera>();
            //}

            //Cursor.lockState = CursorLockMode.Locked;
            ReferenceManager.instance.headset.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (referenceManager.consoleManager.consoleGameObject.activeSelf) return;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleSpectator(!active);
            }
            if (!active) return;
            if (Input.anyKey || Input.GetKeyUp(KeyCode.Tab))
            {
                HandleInput();
            }
        }


        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CtrlsCanvas.SetActive(true);
                TextCanvas.SetActive(false);
            }
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                CtrlsCanvas.SetActive(false);
                TextCanvas.SetActive(true);
            }
            
            if (!active) return;
            
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                speed /= 4;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                speed = 1;
            }
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.forward * Time.deltaTime * speed);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(Vector3.back * Time.deltaTime * speed);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * Time.deltaTime * speed);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(Vector3.right * Time.deltaTime * speed);
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Translate(Vector3.up * Time.deltaTime * speed);
            }
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Translate(Vector3.down * Time.deltaTime * speed);
            }
        }

        public void MirrorVRView()
        {
            CtrlsCanvas.SetActive(false);
            TextCanvas.SetActive(false);
            spectatorCamera.enabled = false;
            avatar.SetActive(false);
            ReferenceManager.instance.headset.gameObject.SetActive(true);
        }

        public void ToggleSpectator(bool toggle)
        {
            avatar.SetActive(toggle);
            CtrlsCanvas.SetActive(!toggle);
            TextCanvas.SetActive(toggle);
            spectatorCamera.enabled = toggle;
            spectatorCamera.GetComponent<SpectatorCameraLook>().enabled = toggle;
            active = toggle;

            if (toggle)
            {
                transform.position = new Vector3(0, 1.15f, -2.5f);
                transform.rotation = Quaternion.identity;
            }
        }
    }
}