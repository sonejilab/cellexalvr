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

        public override void Select()
        {
            imageHandler.SpawnAOIImages(scanID, aoiIDs, roiID);
            Highlight();
        }

        public override void Detach()
        {
            base.Detach();
        }

        public override void Reattach()
        {
            base.Reattach();
            Vector3 targetPos;
            if (index > imageHandler.slideScroller.currentSlide[1] + 5)
            {
                targetPos = imageHandler.inactivePosRight;
                Fade(false);
            }
            else if (index < imageHandler.slideScroller.currentSlide[1])
            {
                targetPos = imageHandler.inactivePosLeft;
                Fade(false);
            }
            else
            {
                int i = SlideScroller.mod(index-imageHandler.slideScroller.currentSlide[1], imageHandler.slideScroller.currentROIIDs.Length);
                targetPos = imageHandler.sliceCirclePositions[i];
                if (imageHandler.selectedROI != null)
                {
                    targetPos.y += 1.1f;
                }
            }
            Move(targetPos);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GeoMXROISlide))]
    public class GeoMXROISlideEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GeoMXROISlide myTarget = (GeoMXROISlide)target;
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