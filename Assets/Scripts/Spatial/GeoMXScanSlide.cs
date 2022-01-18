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

        public override void Detach()
        {
            base.Detach();
        }

        public override void Reattach()
        {
            base.Reattach();
            Vector3 targetPos;
            int i = SlideScroller.mod(index - imageHandler.slideScroller.currentSlide[0], imageHandler.slideScroller.currentScanIDs.Length);
            if (i > 6)
            {
                targetPos = imageHandler.inactivePosRight;
                Fade(false);
            }
            else if (i < 0)
            {
                targetPos = imageHandler.inactivePosLeft;
                Fade(false);
            }
            else
            {
                targetPos = imageHandler.sliceCirclePositions[i];
                if (imageHandler.selectedScan != null)
                {
                    targetPos.y += 1.1f;
                }
                if (imageHandler.selectedROI != null)
                {
                    targetPos.y += 1.1f;
                }
            }
            Move(targetPos);
        }

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(GeoMXScanSlide))]
    public class GeoMXScanSlideEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GeoMXScanSlide myTarget = (GeoMXScanSlide)target;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select"))
            {
                myTarget.Select();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reattach"))
            {
                myTarget.Reattach();
            }
            GUILayout.EndHorizontal();
            DrawDefaultInspector();
        }
    }
#endif
}