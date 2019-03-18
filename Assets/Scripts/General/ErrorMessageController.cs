using System.Collections;
using UnityEngine;

namespace CellexalVR.General
{
    /// <summary>
    /// Shows/Hides the text panel containing the error message when the error object is clicked.
    /// </summary>
    public class ErrorMessageController : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            gameObject.SetActive(false);
        }

        public void DisplayErrorMessage(int displayTime)
        {
            gameObject.SetActive(true);
            StartCoroutine(HideErrorMessage(displayTime));
        }

        private IEnumerator HideErrorMessage(int waiForSeconds)
        {
            yield return new WaitForSeconds(waiForSeconds);
            gameObject.SetActive(false);
        }
    }
}