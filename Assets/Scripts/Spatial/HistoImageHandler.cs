using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AnalysisLogic;
using CellexalVR.AnalysisLogic;
using CellexalVR.General;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Class to handle the histology images (<see cref="HistoImage"/>).  
    /// </summary>
    public class HistoImageHandler : MonoBehaviour
    {
        public static HistoImageHandler instance;

        public HistoImage slicePrefab;
        public List<HistoImage> images = new List<HistoImage>();
        public Dictionary<string, List<HistoImage>> imageDict;

        public GameObject parentPrefab;

        private void Awake()
        {
            instance = this;
            imageDict = new Dictionary<string, List<HistoImage>>();
        }

        /// <summary>
        /// Create a 3d volume of the slice textures. Probably only relevant if you have enough slices.
        /// </summary>
        private void CreateVolume()
        {
            foreach (KeyValuePair<string, List<HistoImage>> kvp in imageDict)
            {
                Texture2D[] textures = new Texture2D[kvp.Value.Count];
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    textures[i] = kvp.Value[i].texture;
                }
            }
        }

        /// <summary>
        /// Gather your images to original positions.
        /// </summary>
        private void GatherImages()
        {
            foreach (KeyValuePair<string, List<HistoImage>> kvp in imageDict)
            {
                var parent = GameObject.Instantiate(parentPrefab);
                parent.gameObject.name = kvp.Key;
                foreach (HistoImage im in kvp.Value)
                {
                    im.transform.parent = parent.transform;
                }
            }
        }

        /// <summary>
        /// Add new image to the handler.
        /// </summary>
        /// <param name="image"></param>
        public void AddImage(HistoImage image)
        {
            images.Add(image);
        }

        /// <summary>
        /// Allign the images so the tissue in the textures is alligned.
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="image2"></param>
        /// <param name="moveX"></param>
        public void AllignImages(HistoImage image1, HistoImage image2, bool moveX = false)
        {
            HistoImage hi1 = image1.transform.position.z > image2.transform.position.z ? image1 : image2;
            HistoImage hi2 = image1.transform.position.z > image2.transform.position.z ? image2 : image1;

            float diffX = hi1.scaledMaxValues.x - hi2.scaledMaxValues.x;
            float diffY = hi1.scaledMaxValues.y - hi2.scaledMaxValues.y;
            float diffX2 = hi1.scaledMinValues.x - hi2.scaledMinValues.x;
            float diffY2 = hi1.scaledMinValues.y - hi2.scaledMinValues.y;

            diffX = (diffX + diffX2) / 2;
            diffY = (diffY + diffY2) / 2;

            print($"{diffX}, {diffY}");
            Vector3 pos = hi1.transform.localPosition;
            pos.x -= diffY;
            pos.y += diffX;
            if (moveX)
            {
                pos.x += 1;
                pos.z = 0.2f * (hi1.sliceNr - 1);

                Vector3 pos2 = hi2.transform.localPosition;
                pos2.x += 1;
                pos2.z = 0.2f * (hi2.sliceNr - 1);
                hi2.transform.localPosition = pos2;
            }
            hi1.transform.localPosition = pos;
        }
    }
}
