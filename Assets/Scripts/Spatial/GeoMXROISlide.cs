using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CellexalVR.Spatial
{

    public class GeoMXROISlide : GeoMXSlide
    {
        public string scanID;
        public string roiID;
        public string[] aoiIDs;

        private bool selected;

        public override void Select()
        {
            GeoMXSlideStack stack = GetComponentInParent<GeoMXSlideStack>();
            if (selected)
            {
                selected = false;
                if (stack)
                {
                    stack.UnSelectROI(roiID);
                }
                else
                {
                    imageHandler.UnSelectROI(roiID, true);
                }
                UnHighlight();
            }
            else
            {
                selected = true;
                if (stack)
                {
                    stack.SpawnAOIImages(scanID, roiID, aoiIDs);
                }
                else
                {
                    imageHandler.SpawnAOIImages(scanID, aoiIDs, roiID);
                }
                Highlight();
            }
        }

        public override void SelectCells(int group)
        {

        }

        public override void Detach()
        {
            base.Detach();
        }

        public override void Reattach()
        {
            base.Reattach();
        }

        public override void OnRaycastHit()
        {
            //imageHandler.HighlightCells(roiID);
        }
    }

//#if UNITY_EDITOR
//    [CustomEditor(typeof(GeoMXROISlide))]
//    public class GeoMXROISlideEditor : Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            GeoMXROISlide myTarget = (GeoMXROISlide)target;
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