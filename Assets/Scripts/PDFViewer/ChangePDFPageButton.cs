using System.Collections;
using System.Collections.Generic;
using CellexalVR.Menu.Buttons;
using UnityEngine;

namespace CellexalVR.PDFViewer
{
    public class ChangePDFPageButton : CellexalButton
    {
        public int dir;

        private CurvedMeshGenerator curvedMeshGenerator;

        private PDFMesh pdfMesh;
        // Start is called before the first frame update
        private void Start()
        {
            curvedMeshGenerator = GetComponentInParent<CurvedMeshGenerator>();
            pdfMesh = GetComponentInParent<PDFMesh>();

        }

        // Update is called once per frame
        protected override string Description => "Change Page(s)";

        public override void Click()
        {
            pdfMesh.ChangePage(dir);
        }
    }
}