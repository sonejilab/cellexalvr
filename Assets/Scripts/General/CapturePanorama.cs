using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Rendering;
using System.IO;

namespace CellexalVR.General
{
    public class CapturePanorama : MonoBehaviour

    {
        public RenderTexture cubemap1;
        public RenderTexture cubemap2;
        public RenderTexture equirect;
        public bool renderStereo = true;
        public float stereoSeparation = 0.064f;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Capture();
            }
        }
        void Capture()
        {
            Camera cam = GetComponent<Camera>();

            if (cam == null)
            {
                cam = GetComponentInParent<Camera>();
            }

            if (cam == null)
            {
                Debug.Log("stereo 360 capture node has no camera or parent camera");
            }

            if (renderStereo)
            {
                cam.stereoSeparation = stereoSeparation;
                cam.RenderToCubemap(cubemap1, 63, Camera.MonoOrStereoscopicEye.Left);
                cam.RenderToCubemap(cubemap2, 63, Camera.MonoOrStereoscopicEye.Right);
            }
            else
            {
                cam.RenderToCubemap(cubemap1, 63, Camera.MonoOrStereoscopicEye.Mono);
            }

            //optional: convert cubemaps to equirect

            if (equirect == null)
                return;

            cubemap1.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
            cubemap2.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);

            int sqr = 8192;

            Texture2D texture2D = new Texture2D(sqr, sqr / 4, TextureFormat.RGB24, false);
            texture2D.ReadPixels(new Rect(0, 0, sqr, sqr / 4), 0, 0);
            RenderTexture.active = null; //can help avoid errors 
            //virtuCamera.camera.targetTexture = null;
            // consider ... Destroy(tempRT);

            byte[] bytes;
            bytes = texture2D.EncodeToPNG();

            System.IO.File.WriteAllBytes(
                OurTempSquareImageLocation(), bytes);
            // virtualCam.SetActive(false); ... no great need for this.

            // now use the image, 
        }

        private string OurTempSquareImageLocation()
        {
            string r = Path.Combine(Directory.GetCurrentDirectory(), "p.png");
            return r;
        }
    }
}
