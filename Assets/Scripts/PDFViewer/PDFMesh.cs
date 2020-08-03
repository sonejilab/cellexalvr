using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;
using Patagames.Pdf;
using Patagames.Pdf.Enums;
using Patagames.Pdf.Net;
using TMPro;


namespace CellexalVR.PDFViewer
{
    /// <summary>
    /// Main class for the viewing of PDF articles. When loading data it looks for any files ending with .pdf.
    /// First it converts them to images so it can be rendered as textures then it stores them in the user specific folder. 
    /// </summary>
    public class PDFMesh : MonoBehaviour
    {
        public GameObject meshObjPrefab;
        public Transform pageParent;
        public TextMeshPro nrOfPagesText;
        public TextMeshPro currentPagesTextStationary;
        public TextMeshPro currentPagesTextPocket;
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
        private ReferenceManager referenceManager;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            currentPage = 1;
            totalNrOfpages = 0;
            curvedMeshGenerator = GetComponentInChildren<CurvedMeshGenerator>();
            currentViewingMode = ViewingMode.CurvedStationary;

            TogglePDF(false);

            // CellexalEvents.GraphsLoaded.AddListener(CreatePage);
            // CellexalEvents.GraphsLoaded.AddListener(ShowMultiplePages);
            // CellexalEvents.GraphsLoaded.AddListener(() => SetSettingsHandle(currentViewingMode));
        }

        /// <summary>
        /// Generating the mesh to show the image textures on.
        /// </summary>
        private void CreatePage()
        {
            if (pageMesh != null) Destroy(pageMesh.gameObject);
            GameObject obj = Instantiate(meshObjPrefab, pageParent.transform);
            pageMesh = obj.GetComponent<MeshDeformer>();
            obj.GetComponent<CurvedMeshGenerator>().GenerateNodes(currentViewingMode, radiusSlider.Value,
                curvatureSlider.Value);
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

        /// <summary>
        /// Reads the pdf file and converts to images to be able to render the pdf as textures.
        /// Each page becomes a separate image.
        /// </summary>
        /// <param name="path"></param>
        public void ReadPDF(string path)
        {
            int i = 0;
            string[] files = Directory.GetFiles(path, "*.pdf");
            if (files.Length == 0)
            {
                CellexalLog.Log("Tried to find article pdf in data folder but none were found.");
                // CellexalEvents.GraphsLoaded.RemoveListener(CreatePage);
                // CellexalEvents.GraphsLoaded.RemoveListener(ShowMultiplePages);
                // CellexalEvents.GraphsLoaded.RemoveListener(() => SetSettingsHandle(currentViewingMode));
                return;
            }

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

            CellexalEvents.PDFArticleRead.Invoke();
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

        /// <summary>
        /// Renders the page images as textures on top of the generated mesh.
        /// If multiple pages are to be displayed,
        /// the pages(images) are stitched together to one big texture that is then rendered.
        /// </summary>
        /// <param name="startPage"></param>
        /// <param name="nrOfPages"></param>
        /// <returns></returnst
        private IEnumerator ShowPagesCoroutine(int startPage, int nrOfPages)
        {
            List<UnityEngine.Color[]> allTexturePixels = new List<UnityEngine.Color[]>();
            int width = 0;
            int height = 0;
            for (int i = startPage; i < startPage + nrOfPages; i++)
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

            currentPagesTextStationary.text = $"{currentPage} - {currentPage + nrOfPages - 1}";
            currentPagesTextPocket.text = currentPage.ToString();
            nrOfPagesText.text = $"{nrOfPages}";
        }

        public void ChangeNrOfPages(int i)
        {
            if ((nrOfPages == 1 && i < 0) || (currentPage + nrOfPages + i - 1) > totalNrOfpages) return;
            nrOfPages += i;
            nrOfPagesText.text = nrOfPages.ToString();
            CreatePage();
            ShowMultiplePages();
        }

        public void ChangeCurvature(float value)
        {
            CreatePage();
        }

        public void ChangeRadius(float value)
        {
            CreatePage();
        }

        public void ShowMultiplePages()
        {
            StartCoroutine(ShowPagesCoroutine(currentPage, nrOfPages));
        }

        public void ChangePage(int i)
        {
            if ((currentPage == 1 && i < 0) || (currentPage + nrOfPages + i - 1) > totalNrOfpages) return;
            currentPage += i;
            StartCoroutine(ShowPagesCoroutine(currentPage, nrOfPages));
        }


        public void ScaleX(float value)
        {
            Vector3 scale = pageParent.transform.localScale;
            scale.x = scale.z = value;
            pageParent.transform.localScale = scale;
        }

        public void ScaleY(float value)
        {
            Vector3 scale = pageParent.transform.localScale;
            scale.y = value;
            pageParent.transform.localScale = scale;
        }


        /// <summary>
        /// Change between two modes. Stationary where the mesh can be curved around the user
        /// and multiple pages can be displayed at the same time as the mesh can be much wider but the mesh can not be
        /// grabbed and moved around with your hands.
        /// Pocket where one page is displayed at a time. This mesh can be moved around using your hands.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ChangeViewingMode()
        {
            switch (currentViewingMode)
            {
                case ViewingMode.CurvedStationary:
                    currentViewingMode = ViewingMode.PocketMovable;
                    pageParent.localScale = Vector3.one;
                    CreatePage();
                    SetSettingsHandle(ViewingMode.PocketMovable);
                    StartCoroutine(ShowPagesCoroutine(1, 1));
                    break;
                case ViewingMode.PocketMovable:
                    pageParent.localScale = new Vector3(0.5f, 0.25f, 0.5f);
                    currentViewingMode = ViewingMode.CurvedStationary;
                    SetSettingsHandle(ViewingMode.CurvedStationary);
                    CreatePage();
                    ShowMultiplePages();
                    ScaleX(scaleXSliderStationary.Value);
                    ScaleY(scaleYSliderStationary.Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// The two different modes have different settings that can be changed. So when changing mode the
        /// settings handle is also changed.
        /// </summary>
        /// <param name="viewingMode"></param>
        private void SetSettingsHandle(ViewingMode viewingMode)
        {
            if (viewingMode == ViewingMode.PocketMovable)
            {
                settingsHandlerPocket.SetActive(true);
                settingsHandlerCurved.SetActive(false);
                settingsHandlerPocket.transform.parent = pageMesh.transform;
                settingsHandlerPocket.transform.localScale = new Vector3(0.3f, 0.12f, 0.02f);
                settingsHandlerPocket.transform.localPosition = new Vector3(0.3f, -0.1f, 0);
                settingsHandlerPocket.transform.localRotation = Quaternion.identity;

                Transform cameraTransform = referenceManager.headset.transform;
                pageMesh.transform.position = cameraTransform.position + cameraTransform.forward * 0.7f;
                pageMesh.transform.LookAt(referenceManager.headset.transform.position);
                pageMesh.transform.Rotate(0, 180, 0);
            }

            else
            {
                settingsHandlerPocket.SetActive(false);
                settingsHandlerCurved.SetActive(true);
                pageParent.transform.localRotation = Quaternion.Euler(0, 90, 0);
                pageParent.transform.localPosition = new Vector3(0, 0.5f, 0);
                settingsHandlerPocket.transform.parent = transform;

                Transform cameraTransform = referenceManager.headset.transform;
                settingsHandlerCurved.transform.position = cameraTransform.position + cameraTransform.forward * 0.7f;
                settingsHandlerCurved.transform.LookAt(referenceManager.headset.transform.position);
                settingsHandlerCurved.transform.Rotate(0, 180, 0);
            }
        }


        /// <summary>
        /// Used to show or hide the pdf mesh and the settings handle.
        /// </summary>
        /// <param name="toggle"></param>
        public void TogglePDF(bool toggle)
        {
            pageParent.gameObject.SetActive(toggle);

            if (!toggle) return;
            CreatePage();
            ShowMultiplePages();
            SetSettingsHandle(currentViewingMode);

        }

        //private Apitron.PDF.Rasterizer.Document _document = new Document(new FileStream("path", FileMode.Create));
    }
}