using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CellexalVR.Spatial
{

    public class GeoMXScanSlide : GeoMXSlide
    {
        public string scanID;
        public string[] rois;


        private bool selected;




        public override void Select()
        {
            if (selected)
            {
                selected = false;
                imageHandler.UnSelectScan(scanID, true);
                UnHighlight();

            }
            else
            {
                selected = true;
                imageHandler.SpawnROIImages(scanID, rois);
                Highlight();
            }
        }

        public override void SelectCells(int group)
        {
            throw new System.NotImplementedException();
        }

        public override void Detach()
        {
            base.Detach();
        }

        public override void Reattach()
        {
            base.Reattach();

        }



    }


    //#if UNITY_EDITOR
    //    [CustomEditor(typeof(GeoMXScanSlide))]
    //    public class GeoMXScanSlideEditor : Editor
    //    {
    //        public override void OnInspectorGUI()
    //        {
    //            GeoMXScanSlide myTarget = (GeoMXScanSlide)target;
    //            GUILayout.BeginHorizontal();
    //            if (GUILayout.Button("Select"))
    //            {
    //                myTarget.Select();
    //            }
    //            GUILayout.EndHorizontal();
    //            GUILayout.BeginHorizontal();
    //            if (GUILayout.Button("Reattach"))
    //            {
    //                myTarget.Reattach();
    //            }
    //            GUILayout.EndHorizontal();
    //            DrawDefaultInspector();
    //        }
    //    }
    //#endif
}