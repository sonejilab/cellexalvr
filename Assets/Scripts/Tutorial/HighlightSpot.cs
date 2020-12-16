using UnityEngine;
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


        private void OnTriggerEnter(Collider other)
        {
            if (this.name == "Portal")
            {
                if (other.CompareTag("Player")) 
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