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

        private SelectionManager selectionManager;

        protected override void Start()
        {
            base.Start();
            selectionManager = referenceManager.selectionManager;
        }


        /// <summary>
        /// Click this panel, calling the dump function in selectionmanager.
        /// </summary>
        public override void Click()
        {
            referenceManager.multiuserMessageSender.SendMessageExportAnnotations();
            selectionManager.DumpAnnotatedSelectionToTextFile();
        }
    }
}