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

        //private IEnumerator ReadSlices()
        //{
        //    //string path = Directory.GetCurrentDirectory() + "//Data//" + file;
        //    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + "//Data//seuratSlices//", "*.mds");
        //    string[] imageFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "//Data//seuratSlices//", "*.png");
        //    for (int i = 0; i < files.Length; i++)
        //    {
        //        int lineCount = 0;
        //        string imageFile = imageFiles[i];
        //        byte[] imageData = File.ReadAllBytes(imageFile);
        //        Texture2D texture = new Texture2D(2, 2);
        //        texture.LoadImage(imageData);
        //        string name = files[i].Split('.')[0];
        //        HistoImage hi = Instantiate(slicePrefab);
        //        PointCloud pc = hi.GetComponent<PointCloud>();
        //        pc.Initialize(i);
        //        hi.gameObject.name = name;
        //        hi.texture = texture;
        //        hi.transform.position = new Vector3(0f, 1f, (float)i / 5f);
        //        using (StreamReader sr = new StreamReader(files[i]))
        //        {
        //            string header = sr.ReadLine();
        //            while (!sr.EndOfStream)
        //            {
        //                string line = sr.ReadLine();
        //                string[] words = line.Split(',');
        //                float.TryParse(words[0], out float x);
        //                float.TryParse(words[1], out float y);
        //                int xCoord = (int)x;
        //                int yCoord = (int)y;
        //                PointCloudGenerator.instance.AddGraphPoint(lineCount.ToString(), xCoord, yCoord);
        //                if (lineCount % 100 == 0) yield return null;
        //                //hi.imgCoords.Add(new Vector2Int(xCoord, yCoord));
        //                int textureX = lineCount % PointCloudGenerator.textureWidth;
        //                int textureY = (lineCount / PointCloudGenerator.textureWidth);
        //                TextureHandler.instance.textureCoordDict[lineCount.ToString()] = new Vector2Int(textureX, textureY);
        //                lineCount++;
        //            }
        //        }
        //        //hi.Initialize();
        //        hi.image.GetComponent<MeshRenderer>().material.mainTexture = texture;
        //        PointCloudGenerator.instance.SpawnPoints(hi, par);
        //        images.Add(hi);
        //    }

        //    StartCoroutine(PointCloudGenerator.instance.CreateColorTextureMap());

        //    CellexalEvents.GraphsLoaded.Invoke();
        //}

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                AllignImages(images[0], images[1]);
                AllignImages(images[2], images[3]);
            }

            //if (Input.GetKeyDown(KeyCode.T))
            //{
            //    StartCoroutine(ReadSlices());
            //}
        }

        public void AddImage(HistoImage image)
        {
            images.Add(image);
        }

        public void AllignImages(HistoImage image1, HistoImage image2)
        {
            HistoImage hi1 = image1.transform.position.z > image2.transform.position.z ? image1 : image2;
            HistoImage hi2 = image1.transform.position.z > image2.transform.position.z ? image2 : image1;

            float diffX = hi1.scaledMaxValues.x - hi2.scaledMaxValues.x;
            float diffY = hi1.scaledMaxValues.y - hi2.scaledMaxValues.y;
            float diffX2 = hi1.scaledMinValues.x - hi2.scaledMinValues.x;
            float diffY2 = hi1.scaledMinValues.y - hi2.scaledMinValues.y;

            diffX = (diffX + diffX2) / 2;
            diffY = (diffY + diffY2) / 2;


            Vector3 pos = hi1.image.transform.position;
            pos.x -= diffX;
            pos.y += diffY;
            hi1.image.transform.position = pos;

            pos = hi1.visualEffect.transform.position;
            pos.x -= diffX;
            pos.y += diffY;
            hi1.visualEffect.transform.position = pos;
        }
    }
}
