using CellexalVR.General;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CellexalVR.Tutorial
{

    /// <summary>
    /// Used in tutorial scene to mark out specific spots of interest.
    /// </summary>
    public class HighlightSpot : MonoBehaviour
    {

        public TutorialManager tutorialManager;

        private bool loadLevel = false;
        private Image screenCanvas;
        private float fadeTime = 4.0f;
        private float elapsedTime = 0.0f;
        private float colorAlpha;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //if (loadLevel)
            //{
            //    if (elapsedTime < fadeTime / 2.0f)
            //    {
            //        print("fade to black");
            //        elapsedTime += Time.deltaTime;
            //        colorAlpha += 0.05f;
            //        screenCanvas.GetComponent<Image>().color = new Color(0, 0, 0, colorAlpha);
            //    }
            //    else if (elapsedTime < fadeTime && elapsedTime > (fadeTime / 2.0f))
            //    {
            //        print("fade back");
            //        elapsedTime += Time.deltaTime;
            //        colorAlpha -= 0.05f;
            //        screenCanvas.GetComponent<Image>().color = new Color(0, 0, 0, colorAlpha);
            //    }
            //    else
            //    {
            //        loadLevel = false;
            //        tutorialManager.screenCanvas.SetActive(false);
            //        CrossSceneInformation.Tutorial = false;
            //        GetComponentInParent<TutorialManager>().referenceManager.loaderController.ResetFolders(true);
            //        transform.parent.gameObject.SetActive(false);
            //    }
            //}
        }

        private void OnTriggerEnter(Collider other)
        {
            if (this.name == "Portal")
            {
                if (other.gameObject.name == "ControllerCollider(Clone)" /*other.transform.parent.name == "[VRTK][AUTOGEN][Controller][CollidersContainer]"*/)
                {
                    tutorialManager.CompleteTutorial();
                }
                //loadLevel = true;
            }
            if (other.tag == "Graph")
            {
                print("Graph entered");
                this.gameObject.GetComponent<Collider>().enabled = false;
                foreach (ParticleSystem sys in this.GetComponentsInChildren<ParticleSystem>())
                {
                    sys.Stop();
                }
                tutorialManager.NextStep();
            }
            //if ((other.tag == "Controller" || other.tag == "Cells") && !this.name.Contains("HighlightSpot"))
            //{
            //    foreach (ParticleSystem sys in this.GetComponentsInChildren<ParticleSystem>())
            //    {
            //        sys.Stop();
            //    }
            //}

        }
    }
}