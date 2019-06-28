using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    public class ColoringOptionsList : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private ColoringOptionsPanel[] panels;

        private void Start()
        {
            panels = gameObject.GetComponentsInChildren<ColoringOptionsPanel>();
        }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        public void SwitchMode(GraphManager.GeneExpressionColoringMethods mode)
        {
            referenceManager.graphManager.GeneExpressionColoringMethod = mode;

            foreach (ColoringOptionsPanel panel in panels)
            {
                if (panel.modeToSwitchTo == mode)
                {
                    panel.text.color = Color.green;
                }
                else
                {
                    panel.text.color = Color.white;
                }
            }
        }
    }
}
