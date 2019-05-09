using UnityEngine;
namespace CellexalVR.General
{
    /// <summary>
    /// Sets references of error message objects
    /// </summary>
    public class ErrorMessageManager : MonoBehaviour
    {

        public ReferenceManager referenceManager;
        public GameObject errorMessagePrefab;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        public void Awake()
        {
            CellexalError.ReferenceManager = referenceManager;
            CellexalError.errorPrefab = errorMessagePrefab;
        }
    }
}