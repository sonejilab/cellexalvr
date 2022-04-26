using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace CellexalVR.Spatial
{
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

        public void Scroll(int val)
        {
            if (aoiSlides.Count > 0)
            {
                currentSlide.Select();
            }
            currentSlide.Move(new Vector3(0, -1, 0));
            currentSlide.Fade(false);
            currentSlideCount = SlideScroller.mod(currentSlideCount + val, count);
            GeoMXSlide newSlide = slides[currentSlideCount];
            newSlide.gameObject.SetActive(true);
            newSlide.Fade(true);
            newSlide.Move(Vector3.zero);
            currentSlide = newSlide;
        }

        public void SpawnAOIImages(string scanID, string roiID, string[] aoiIDs)
        {
            StartCoroutine(SpawnAOIImagesCoroutine(scanID, roiID, aoiIDs));
        }

        private IEnumerator SpawnAOIImagesCoroutine(string scanID, string roiID, string[] aoiIDs)
        {
            for (int i = 0; i < aoiIDs.Length; i++)
            {
                string path = $"{GeoMXImageHandler.imagePath}\\{scanID}\\{aoiIDs[i]}.png";
                if (File.Exists(path))
                {
                    using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture($"file://{path}"))
                    {
                        uwr.downloadHandler = new DownloadHandlerTexture(true);
                        yield return uwr.SendWebRequest();
                        if (uwr.result != UnityWebRequest.Result.Success)
                        {
                            print(uwr.error);
                        }
                        else
                        {
                            GeoMXAOISlide aoi = Instantiate(GeoMXImageHandler.instance.aoiPrefab, transform);
                            aoi.imageHandler = GeoMXImageHandler.instance;
                            Texture2D aoiTexture = DownloadHandlerTexture.GetContent(uwr);
                            aoi.SetLowQ(aoiTexture);
                            //aoi.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", aoiTexture);
                            float ratio = (float)aoiTexture.width / (float)aoiTexture.height;
                            aoi.transform.localScale = new Vector3(1f * ratio, 1f, 1f);
                            aoi.originalScale = aoi.transform.localScale;
                            aoi.transform.localPosition = new Vector3(1.2f * i, 1.2f, 0);
                            aoiSlides.Add(aoi);
                            aoi.index = i;
                            aoi.displayName = aoiIDs[i];
                            aoi.type = 2;
                            aoi.aoiID = aoiIDs[i];
                        }
                    }
                }
                else
                {
                    print($"Could not find image {path}");
                }
            }

        }
        public void UnSelectROI(string roiID)
        {
            foreach(GeoMXAOISlide slide in aoiSlides)
            {
                Destroy(slide.gameObject);
            }
            aoiSlides.Clear();
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