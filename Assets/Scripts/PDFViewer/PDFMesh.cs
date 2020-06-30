using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Remoting;
using System.Windows.Forms.VisualStyles;
using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;
using Patagames.Pdf;
using Patagames.Pdf.Enums;
using Patagames.Pdf.Net;
using TMPro;
using Color = System.Drawing.Color;


namespace CellexalVR.PDFViewer
{
    public class PDFMesh : MonoBehaviour
    {
        public GameObject meshObjPrefab;
        public Transform pageParent;
        public TextMeshPro nrOfPagesText;
        public TextMeshPro currentPagesText;
        public VRSlider curvatureSlider;
        public VRSlider radiusSlider;
        public VRSlider scaleXSliderStationary;
        public VRSlider scaleYSliderStationary;
        public VRSlider scaleXSliderPocket;
        public VRSlider scaleYSliderPocket;
        public GameObject settingsHandlerCurved;
        public GameObject settingsHandlerPocket;
        [HideInInspector] public int currentPage;
        public Texture2D texture;
        [HideInInspector] public int totalNrOfpages;
        public int currentNrOfPages;

        public enum ViewingMode
        {
            PocketMovable,
            CurvedStationary
        }

        [SerializeField] private int nrOfPages = 1;
        private ViewingMode currentViewingMode;
        private CurvedMeshGenerator curvedMeshGenerator;
        private Vector3 settingsHandlerStartScale;
        private MeshDeformer pageMesh;

        private void Start()
        {
            currentPage = 1;
            totalNrOfpages = 0;
            curvedMeshGenerator = GetComponentInChildren<CurvedMeshGenerator>();
            currentViewingMode = ViewingMode.CurvedStationary;

            CellexalEvents.GraphsLoaded.AddListener(CreatePage);
            CellexalEvents.GraphsLoaded.AddListener(ShowMultiplePages);
        }

        private void CreatePage()
        {
            if (pageMesh != null) Destroy(pageMesh.gameObject);
            GameObject obj = Instantiate(meshObjPrefab, pageParent.transform);
            pageMesh = obj.GetComponent<MeshDeformer>();
            obj.GetComponent<CurvedMeshGenerator>().GenerateNodes(currentViewingMode, radiusSlider.value,
                curvatureSlider.value);
            SetSettingsHandle(currentViewingMode);
        }


        // private void Update()
        // {
        //     if (currentViewingMode == ViewingMode.PocketMovable && pageMesh != null)
        //     {
        // settingsHandlerPocket.transform.localScale = settingsHandlerStartScale;
        // settingsHandlerPocket.transform.position = pageMesh.settingsHandlerTransform.position;
        // settingsHandlerPocket.transform.rotation = pageMesh.transform.rotation;
        // settingsHandlerPocket.transform.Rotate(0, 0, 90);
        // Vector3 pagePos = pageMesh.transform.position;
        // settingsHandlerPocket.transform.position = pageMesh.transform.position;
        //     }
        //     
        // }

        public void ReadPDF(string path)
        {
            int i = 0;
            string[] files = Directory.GetFiles(path, "*.pdf");
            if (files.Length == 0) return;
            string pdfPath = files[0];
            using (PdfDocument doc = PdfDocument.Load(pdfPath))
            {
                foreach (PdfPage page in doc.Pages)
                {
                    int width = (int) (page.Width / 72.0f * 96);
                    int height = (int) (page.Height / 72.0f * 96);
                    using (PdfBitmap bitmap = new PdfBitmap(width, height, true))
                    {
                        bitmap.FillRect(0, 0, width, height, FS_COLOR.White);
                        page.Render(bitmap, 0, 0, width, height, PageRotate.Normal, RenderFlags.FPDF_LCD_TEXT);
                        string savePath = $"{CellexalUser.UserSpecificFolder}//page{++i}.png";
                        bitmap.Image.Save(savePath, ImageFormat.Png);
                    }

                    totalNrOfpages++;
                }

                // float w = doc.Pages[0].Width / 72.0f * 96;
                // float h = doc.Pages[0].Height / 72.0f * 96;
            }
        }


        public void ShowPage(int pageNr)
        {
            string path = $"{CellexalUser.UserSpecificFolder}\\page{pageNr}.png";
            currentPage = pageNr;
            texture.LoadImage(File.ReadAllBytes(path));
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject.GetComponent<MeshDeformer>() == null) continue;
                r.material.SetTexture("_MainTex", texture);
            }
        }

        private IEnumerator ShowMultiplePagesCoroutine(int start, int nrOfPages)
        {
            List<UnityEngine.Color[]> allTexturePixels = new List<UnityEngine.Color[]>();
            int width = 0;
            int height = 0;
            for (int i = start; i < start + nrOfPages; i++)
            {
                string path = $"{CellexalUser.UserSpecificFolder}\\page{i}.png";
                byte[] imageByteArray = File.ReadAllBytes(path);
                Texture2D imageTexture = new Texture2D(2, 2);
                imageTexture.LoadImage(imageByteArray);
                UnityEngine.Color[] imageColors = imageTexture.GetPixels(0, 0, imageTexture.width, imageTexture.height);

                allTexturePixels.Add(imageColors);

                width = imageTexture.width;
                height = imageTexture.height;
                yield return null;
            }

            List<UnityEngine.Color> mergedTexturePixels = new List<UnityEngine.Color>();

            for (int y = 0; y < height; y++)
            {
                for (int i = 0; i < nrOfPages; i++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int c = x + (width * y);

                        mergedTexturePixels.Add(allTexturePixels[i][c]);
                    }
                }
            }

            Texture2D mergedTexture = new Texture2D(width * nrOfPages, height, TextureFormat.ARGB32, false);
            mergedTexture.SetPixels(mergedTexturePixels.ToArray());
            mergedTexture.Apply();
            texture = mergedTexture;

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject.GetComponent<MeshDeformer>() == null) continue;
                r.material.SetTexture("_MainTex", texture);
            }

            currentPagesText.text = $"{currentPage} - {currentPage + nrOfPages - 1}";
        }

        public void ChangeNrOfPages(int i)
        {
            if ((nrOfPages == 1 && i < 0) || (currentPage + nrOfPages + i - 1) > totalNrOfpages) return;
            nrOfPages += i;
            nrOfPagesText.text = nrOfPages.ToString();
            currentPagesText.text = $"{currentPage} - {currentPage + nrOfPages - 1}";
            CreatePage();
            ShowMultiplePages();
            // curvedMeshGenerator.GenerateNodes(currentViewingMode);
            // curvedMeshGenerator.GenerateMeshes();
            // StartCoroutine(ShowMultiplePagesCoroutine(currentPage, nrOfPages));
        }

        public void ChangeCurvature()
        {
            // curvatureX = curvatureSlider.value;
            // curvedMeshGenerator.curvatureX = curvatureSlider.value;
            CreatePage();
            // curvedMeshGenerator.GenerateNodes(currentViewingMode);
            // curvedMeshGenerator.GenerateMeshes();
        }

        public void ChangeRadius()
        {
            // curvedMeshGenerator.radius = radiusSlider.value;
            CreatePage();
            // curvedMeshGenerator.GenerateNodes(currentViewingMode);
            // curvedMeshGenerator.GenerateMeshes();
        }

        public void ShowMultiplePages()
        {
            StartCoroutine(ShowMultiplePagesCoroutine(currentPage, nrOfPages));
        }

        public void ChangePage(int i)
        {
            if ((currentPage == 1 && i < 0) || (currentPage + nrOfPages + i - 1) > totalNrOfpages) return;
            currentPage += i;
            currentPagesText.text = $"{currentPage} - {currentPage + nrOfPages - 1}";
            StartCoroutine(ShowMultiplePagesCoroutine(currentPage, nrOfPages));
        }


        public void ScaleX()
        {
            Vector3 scale = pageParent.transform.localScale;
            VRSlider currentXSlider = currentViewingMode == ViewingMode.PocketMovable
                ? scaleXSliderPocket
                : scaleXSliderStationary;
            scale.x = currentXSlider.value;
            scale.z = scale.x;
            pageParent.transform.localScale = scale;
        }

        public void ScaleY()
        {
            Vector3 scale = pageParent.transform.localScale;
            VRSlider currentYSlider = currentViewingMode == ViewingMode.PocketMovable
                ? scaleYSliderPocket
                : scaleYSliderStationary;
            scale.y = currentYSlider.value;
            pageParent.transform.localScale = scale;
        }


        public void ChangeViewingMode()
        {
            switch (currentViewingMode)
            {
                case ViewingMode.CurvedStationary:
                    currentViewingMode = ViewingMode.PocketMovable;
                    pageParent.localScale = Vector3.one;
                    SetSettingsHandle(ViewingMode.PocketMovable);
                    StartCoroutine(ShowMultiplePagesCoroutine(1, 1));
                    CreatePage();
                    break;
                case ViewingMode.PocketMovable:
                    pageParent.localScale = new Vector3(0.5f, 0.25f, 0.5f);
                    currentViewingMode = ViewingMode.CurvedStationary;
                    SetSettingsHandle(ViewingMode.CurvedStationary);
                    CreatePage();
                    ShowMultiplePages();
                    ScaleX();
                    ScaleY();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetSettingsHandle(ViewingMode viewingMode)
        {
            if (viewingMode == ViewingMode.PocketMovable)
            {
                settingsHandlerPocket.SetActive(true);
                settingsHandlerCurved.SetActive(false);
                settingsHandlerPocket.transform.parent = pageMesh.transform;
                settingsHandlerPocket.transform.localScale = new Vector3(0.12f, 0.3f, 0.02f);
                settingsHandlerPocket.transform.localPosition = new Vector3(0.3f, -0.1f, 0);
                settingsHandlerPocket.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }

            else
            {
                settingsHandlerPocket.SetActive(false);
                settingsHandlerCurved.SetActive(true);
                settingsHandlerPocket.transform.parent = transform;
            }
        }

        //private Apitron.PDF.Rasterizer.Document _document = new Document(new FileStream("path", FileMode.Create));
    }
}