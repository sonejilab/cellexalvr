using UnityEngine;
using System.Collections;
using CellexalVR.General;

namespace CellexalVR.Interaction
{
    public class ClearAnnotationsPanel : ClickablePanel
    {
        public TMPro.TextMeshPro text;
        private AnnotationManager annotationManager;

        protected override void Start()
        {
            base.Start();
            annotationManager = referenceManager.annotationManager;
        }


        /// <summary>
        /// Click this panel, reset graphs but keep selection groups.
        /// </summary>
        public override void Click()
        {
            annotationManager.ClearAllAnnotations();
        }
    }
}

