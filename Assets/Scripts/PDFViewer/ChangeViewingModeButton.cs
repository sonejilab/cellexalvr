using System;
using System.Collections;
using System.Collections.Generic;
using CellexalVR.Menu.Buttons;
using UnityEngine;

namespace CellexalVR.PDFViewer
{
    public class ChangeViewingModeButton : CellexalButton
    {
        private PDFMesh pdfMesh;

        private void Start()
        {
            pdfMesh = GetComponentInParent<PDFMesh>();
        }

        protected override string Description => "Change Viewing Mode";
        
        public override void Click()
        {
            pdfMesh.ChangeViewingMode();
        }
    }
}