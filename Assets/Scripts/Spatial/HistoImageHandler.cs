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

        private void Awake()
        {
            instance = this;
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                int prevSliceNr = 0;
                for (int i = 0; i < images.Count - 1; i++)
                {
                    bool moveX = images[i].sliceNr < prevSliceNr;
                    AllignImages(images[i], images[i + 1], moveX);
                    prevSliceNr = images[i].sliceNr;
                }

            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                foreach (HistoImage hi in images)
                {
                    hi.CropToTissue2();
                }
            }
        }


        public void AddImage(HistoImage image)
        {
            images.Add(image);
        }

        public void AllignImages(HistoImage image1, HistoImage image2, bool moveX = false)
        {
            HistoImage hi1 = image1.transform.position.z > image2.transform.position.z ? image1 : image2;
            HistoImage hi2 = image1.transform.position.z > image2.transform.position.z ? image2 : image1;

            hi1.transform.parent = transform;
            hi2.transform.parent = transform;

            float diffX = hi1.scaledMaxValues.x - hi2.scaledMaxValues.x;
            float diffY = hi1.scaledMaxValues.y - hi2.scaledMaxValues.y;
            float diffX2 = hi1.scaledMinValues.x - hi2.scaledMinValues.x;
            float diffY2 = hi1.scaledMinValues.y - hi2.scaledMinValues.y;

            diffX = (diffX + diffX2) / 2;
            diffY = (diffY + diffY2) / 2;

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
