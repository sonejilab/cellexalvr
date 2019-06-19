using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// 
    /// </summary>
    public class AnnotatePanel : ClickablePanel
    {
        public TMPro.TextMeshPro text;

        private KeyboardHandler keyboardHandler;

        protected override void Start()
        {
            base.Start();
            keyboardHandler = referenceManager.keyboardHandler;
        }


        /// <summary>
        /// Click this panel, changing the mode of coloring used.
        /// </summary>
        public override void Click()
        {
            if (!keyboardHandler.Text().Equals(string.Empty))
            {
                keyboardHandler.OnAnnotate.Invoke(keyboardHandler.Text());
                keyboardHandler.Clear();
            }
        }
    }
}