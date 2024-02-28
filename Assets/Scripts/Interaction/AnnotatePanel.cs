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
            if (!handler.Text().Equals(string.Empty))
            {
                handler.OnAnnotate.Invoke(handler.Text());
                handler.Clear(true);
            }

        }
    }
}