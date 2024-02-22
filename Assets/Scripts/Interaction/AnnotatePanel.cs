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

        /// <summary>
        /// Click this panel, changing the mode of coloring used.
        /// </summary>
        public override void Click()
        {
            base.Click();
            if (!keyboardHandler.Text().Equals(string.Empty))
            {
                keyboardHandler.OnAnnotate.Invoke(keyboardHandler.Text());
                keyboardHandler.Clear(true);
            }

        }
    }
}