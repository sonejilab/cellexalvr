using UnityEngine;
using System.Collections;
using CellexalVR.General;
using CellexalVR.Tutorial;
using UnityEngine.UI;

namespace CellexalVR.SceneObjects
{
    /// <summary>
    ///  Canvas that appears in fron of the users and takes up the field of view.
    ///  Can be used if one wants to temporarily block the users view or for example play fade in/out animation.
    /// </summary>

    public class ScreenCanvas : MonoBehaviour
    {
        private float fadeTime;
        private float elapsedTime = 0.0f;
        private float colorAlpha;
        private bool fade;

        public ReferenceManager referenceManager;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        // Use this for initialization
        void Start()
        {
            GetComponent<Canvas>().worldCamera = referenceManager.headset.GetComponent<Camera>();
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (fade)
            {
                if (elapsedTime < fadeTime / 2.0f)
                {
                    elapsedTime += Time.deltaTime;
                    colorAlpha += 0.02f;
                    GetComponentInChildren<Image>().color = new Color(0, 0, 0, colorAlpha);
                }
                else if (elapsedTime < fadeTime && elapsedTime > (fadeTime / 2.0f))
                {
                    elapsedTime += Time.deltaTime;
                    colorAlpha -= 0.02f;
                    GetComponentInChildren<Image>().color = new Color(0, 0, 0, colorAlpha);
                }
                else
                {
                    fade = false;
                    elapsedTime = 0f;
                    colorAlpha = 0f;
                }
            }
        }

        public void FadeAnimation(float time=4f)
        {
            fadeTime = time;
            fade = true;
        }


    }
}
