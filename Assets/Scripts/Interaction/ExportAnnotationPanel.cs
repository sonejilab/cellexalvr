using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// 
    /// </summary>
    public class ExportAnnotationPanel : ClickablePanel
    {
        public TMPro.TextMeshPro text;

        protected override void Start()
        {
            base.Start();
        }


        /// <summary>
        /// Click this panel, calling the dump function in the annotation manager which will export the
        /// cell names together with annotations to a text file.
        /// </summary>
        public override void Click()
        {
            base.Click();
            referenceManager.multiuserMessageSender.SendMessageExportAnnotations();
            referenceManager.annotationManager.DumpAnnotatedSelectionToTextFile();
        }
    }
}