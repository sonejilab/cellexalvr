using CellexalVR.General;
using System.Linq;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Keyboard for web browser. Enter key sends output to navigate function in web browser.
    /// </summary>
    public class WebManager : MonoBehaviour
    {
        public SimpleWebBrowser.WebBrowser webBrowser;
        public TMPro.TextMeshPro output;
        public bool isVisible;
        public ReferenceManager referenceManager;

        private bool firstActivated;
        private InteractableObjectBasic interactableObjectBasic;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }


        // Use this for initialization
        private void Start()
        {
            SetVisible(false);
            interactableObjectBasic = GetComponent<InteractableObjectBasic>();
        }

        private void Update()
        {
            if (interactableObjectBasic.isGrabbed)
            {
                referenceManager.multiuserMessageSender.SendMessageMoveBrowser(transform.localPosition, transform.localRotation, transform.localScale);
            }
        }

        public void EnterKey()
        {
            print("Navigate to - " + output.text);
            // If url field does not contain '.' then may not be a url so google the output instead
            if (!output.text.Contains('.'))
            {
                output.text = "www.google.com/search?q=" + output.text;
            }
            webBrowser.OnNavigate(output.text);
            referenceManager.multiuserMessageSender.SendMessageBrowserEnter();
        }

        public void SetBrowserActive(bool active)
        {
            if (!firstActivated && !webBrowser.gameObject.activeInHierarchy)
            {
                webBrowser.gameObject.SetActive(active);
                GetComponentInChildren<ReportListGenerator>().GenerateList();

                firstActivated = true;
            }
            SetVisible(active);
        }

        public void SetVisible(bool visible)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = visible;
            }
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = visible;
            }
            isVisible = visible;
            //webBrowser.enabled = visible;
        }

    }
}