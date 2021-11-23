using CellexalVR.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.General
{
    /// <summary>
    /// A static class for showing users an error message.
    /// </summary>
    public static class CellexalError
    {
        private static ReferenceManager referenceManager;
        public static ReferenceManager ReferenceManager
        {
            get { return referenceManager; }
            set
            {
                referenceManager = value;
                cameraTransform = referenceManager.headset.transform;
            }
        }
        public static GameObject errorPrefab;

        private static Transform cameraTransform;

        /// <summary>
        /// Instantiates an error in front of the user. An error consists of a short title and a longer message.
        /// </summary>
        /// <param name="position">The errors position.</param>
        /// <param name="title">A short title describing the error, e.g "couldn't load data"</param>
        /// <param name="message">A longer message that explains what went wrong, e.g "no data was found, make sure you placed in correctly" and so on</param>
        public static void SpawnError(string title, string message)
        {
            if (CrossSceneInformation.Spectator)
            {
                return;
            }
            if (!cameraTransform)
            {
                if (!referenceManager)
                {
                    referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
                }
                cameraTransform = referenceManager.headset.transform;
            }
            SpawnError(cameraTransform.position + cameraTransform.forward * 0.7f, title, message);
        }

        /// <summary>
        /// Instantiates an error at a specified position. An error consists of a short title and a longer message.
        /// </summary>
        /// <param name="position">The errors position.</param>
        /// <param name="title">A short title describing the error, e.g "couldn't load data"</param>
        /// <param name="message">A longer message that explains what went wrong, e.g "no data was found, make sure you placed in correctly" and so on</param>
        public static void SpawnError(Vector3 position, string title, string message)
        {
            var errorMessage = Object.Instantiate(errorPrefab, position, Quaternion.identity);
            errorMessage.GetComponent<ErrorMessage>().SetErrorMessage(title, message);
            CellexalLog.Log(string.Format("ERROR: {0}", message));
        }
    }

    /// <summary>
    /// An error message that displays a short title, and can be clicked to show a longer error message.
    /// Use <see cref="CellexalError.SpawnError(string, string)"/> to create an error message.
    /// </summary>
    public class ErrorMessage : MonoBehaviour
    {

        public ReferenceManager referenceManager;

        public GameObject errorMessageGameObject;
        public GameObject errorMessageBackground;
        public TextMeshPro errorTitle;
        public TextMeshPro errorMessage;

        private ActionBasedController rightController;
        private UnityEngine.XR.InputDevice device;
        private bool controllerInside;
        private bool errorMessageShown = false;
        private bool backgroundQuadInitialised = false;
        private Animator errorMessageAnimator;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            rightController = referenceManager.rightController;
            transform.LookAt(referenceManager.headset.transform.position);
            transform.Rotate(0, 90, 90);
            errorMessageAnimator = errorMessageGameObject.GetComponent<Animator>();

            CellexalEvents.RightTriggerClick.AddListener(OnTriggerPressed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = false;
            }
        }

        // Open XR
        private void OnTriggerPressed()
        {
            if (controllerInside)
            {
                if (errorMessageShown)
                {
                    HideErrorMessage();
                }
                else
                {
                    ShowErrorMessage();
                }
            }
        }

        /// <summary>
        /// Sets the error message and title.
        /// </summary>
        public void SetErrorMessage(string title, string message)
        {
            errorTitle.text = "Click for more information\n" + title;
            errorMessage.text = message;
            backgroundQuadInitialised = false;
            //ShowErrorMessage();
        }

        /// <summary>
        /// Shows the error message and not just the title.
        /// </summary>
        public void ShowErrorMessage()
        {

            errorMessageShown = true;
            errorMessageGameObject.SetActive(true);
            if (!backgroundQuadInitialised)
            {
                backgroundQuadInitialised = true;
                errorMessage.ForceMeshUpdate(false);

                var fontSize = errorMessage.fontSize;
                var textHeight = errorMessage.renderedHeight / fontSize;
                var scale = new Vector3(0.72f, textHeight / 2f, 1f);
                var position = new Vector3(0f, 0.315f - textHeight / 4f, 0f);
                errorMessageBackground.transform.localPosition = position;
                errorMessageBackground.transform.localScale = scale;
            }
            //errorMessageAnimator.ResetTrigger("Close");
            errorMessageAnimator.SetTrigger("Open");
        }

        /// <summary>
        /// Hides the error message and shows just the title.
        /// </summary>
        public void HideErrorMessage()
        {
            //errorMessageAnimator.ResetTrigger("Open");
            errorMessageAnimator.SetTrigger("Close");
        }

        /// <summary>
        /// Waits until the close error message animation has finished and then deactivates the error message game object
        /// </summary>
        public void DeactivateErrorMessage()
        {
            errorMessageShown = false;
            errorMessageGameObject.SetActive(false);
        }

        /// <summary>
        /// Destroy the gameobject that represents the error message.
        /// </summary>
        private void RemoveErrorMessage()
        {
            Destroy(gameObject);
        }
    }
}