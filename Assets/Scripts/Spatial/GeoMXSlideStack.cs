using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// This class handls the slide stacks.
    /// A slide stack is created when a selection is made.
    /// The aoi images corresponding to the selections are all put in a stack based on the group it belongs to.
    /// </summary>
    public class GeoMXSlideStack : MonoBehaviour
    {
        private int group;
        public int Group
        {
            get => group;
            set
            {
                group = value;
                groupBar.GetComponent<MeshRenderer>().material.color = CellexalConfig.Config.SelectionToolColors[group];
            }
        }

        [SerializeField] private GameObject groupBar;
        private int count;
        private Dictionary<string, GeoMXSlide> slideDict = new Dictionary<string, GeoMXSlide>();
        private List<GeoMXSlide> slides = new List<GeoMXSlide>();
        private GeoMXSlide currentSlide;
        private int currentSlideCount = 0;
        private List<GeoMXAOISlide> aoiSlides = new List<GeoMXAOISlide>();

        /// <summary>
        /// Add a slide to the stack.
        /// </summary>
        /// <param name="slide"></param>
        public void AddSlide(GeoMXAOISlide slide)
        {
            slides.Add(slide);
            if (count > 0)
            {
                slide.gameObject.SetActive(false);
                slide.transform.localScale = Vector3.zero;
                Vector3 pos = slide.transform.localPosition;
                pos.z -= 0.1f;
                slide.transform.localPosition = pos;
            }
            else
            {
                currentSlide = slide;
            }
            count++;
        }

        /// <summary>
        /// Scroll through the stack images.
        /// </summary>
        /// <param name="direction">Right/Left</param>
        public void Scroll(int direction)
        {
            currentSlide.Move(new Vector3(0, -1, 0));
            currentSlide.Fade(false);
            currentSlideCount = SlideScroller.mod(currentSlideCount + direction, count);
            GeoMXSlide newSlide = slides[currentSlideCount];
            newSlide.gameObject.SetActive(true);
            newSlide.Fade(true);
            newSlide.Move(Vector3.zero);
            currentSlide = newSlide;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GeoMXSlideStack))]
    public class GeoMXSlideStackEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GeoMXSlideStack myTarget = (GeoMXSlideStack)target;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Left"))
            {
                myTarget.Scroll(-1);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Right"))
            {
                myTarget.Scroll(1);
            }
            GUILayout.EndHorizontal();

            DrawDefaultInspector();
        }
    }
#endif
}