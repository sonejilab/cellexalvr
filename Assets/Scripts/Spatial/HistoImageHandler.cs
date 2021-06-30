using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AnalysisLogic;
using DefaultNamespace;
using CellexalVR.General;

namespace CellexalVR.Spatial
{
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


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                foreach (KeyValuePair<string, List<HistoImage>> kvp in imageDict)
                {
                    for (int i = 0; i < kvp.Value.Count - 1; i++)
                    {
                        print($"{kvp.Key}, {kvp.Value[i].gameObject.name}, {kvp.Value[i+1].gameObject.name}");
                        AllignImages(kvp.Value[i], kvp.Value[i + 1], false);
                    }
                    //bool moveX = images[i].sliceNr < prevSliceNr;
                }

            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                foreach (HistoImage hi in images)
                {
                    hi.CropToTissue2();
                }
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                //GatherImages();
                CreateVolume();
            }
        }




        private void CreateVolume()
        {
            foreach (KeyValuePair<string, List<HistoImage>> kvp in imageDict)
            {
                Texture2D[] textures = new Texture2D[kvp.Value.Count];
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    textures[i] = kvp.Value[i].texture;
                }
                ImageStack.instance.CreateVolume(textures);
            }
        }

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

        private void StichImages()
        {
            //foreach (KeyValuePair<string, List<HistoImage>> kvp in imageDict)
            //{
            //    foreach (HistoImage im in kvp.Value)
            //    {

            //    }
            //    List<UnityEngine.Color[]> allTexturePixels = new List<UnityEngine.Color[]>();
            //    int width = 0;
            //    int height = 0;
            //    for (int i = startPage; i < startPage + nrOfPages; i++)
            //    {
            //        string path = $"{CellexalUser.UserSpecificFolder}\\PDFImages\\page{i}.png";
            //        byte[] imageByteArray = File.ReadAllBytes(path);
            //        Texture2D imageTexture = new Texture2D(2, 2);
            //        imageTexture.LoadImage(imageByteArray);
            //        UnityEngine.Color[] imageColors = imageTexture.GetPixels(0, 0, imageTexture.width, imageTexture.height);

            //        allTexturePixels.Add(imageColors);

            //        width = imageTexture.width;
            //        height = imageTexture.height;
            //    }

            //    List<UnityEngine.Color> mergedTexturePixels = new List<UnityEngine.Color>();

            //    for (int y = 0; y < height; y++)
            //    {
            //        for (int i = 0; i < nrOfPages; i++)
            //        {
            //            for (int x = 0; x < width; x++)
            //            {
            //                int c = x + (width * y);

            //                mergedTexturePixels.Add(allTexturePixels[i][c]);
            //            }
            //        }
            //    }
            //}
        }



        public void AddImage(HistoImage image)
        {
            images.Add(image);
        }

        public void AllignImages(HistoImage image1, HistoImage image2, bool moveX = false)
        {
            HistoImage hi1 = image1.transform.position.z > image2.transform.position.z ? image1 : image2;
            HistoImage hi2 = image1.transform.position.z > image2.transform.position.z ? image2 : image1;

            //hi1.transform.parent = transform;
            //hi2.transform.parent = transform;

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

            //Vector3 pos = hi1.image.transform.localPosition;
            //pos.x -= diffX;
            //pos.y -= diffY;
            //hi1.image.transform.localPosition = pos;

            //pos = hi1.visualEffect.transform.localPosition;
            //pos.x -= diffX;
            //pos.y -= diffY;
            //hi1.visualEffect.transform.localPosition = pos;
        }
    }
}
