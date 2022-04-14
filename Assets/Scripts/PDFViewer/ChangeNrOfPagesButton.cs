using System.Collections;
using System.Collections.Generic;
using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using UnityEngine;

namespace CellexalVR.PDFViewer
{
    public class ChangeNrOfPagesButton : CellexalButton
    {
        public int dir;
        
        private CurvedMeshGenerator curvedMeshGenerator;
        private PDFMesh pdfMesh;
        // Start is called before the first frame update
        private  void Start()
        {
            curvedMeshGenerator = GetComponentInParent<CurvedMeshGenerator>();
            pdfMesh = GetComponentInParent<PDFMesh>();
        }

        // Update is called once per frame
        protected override string Description => "Change nr of pages displayed";

        public override void Click()
        {
            pdfMesh.ChangeNrOfPages(dir);
        }
    }
}