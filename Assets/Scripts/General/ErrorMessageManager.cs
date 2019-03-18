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

        public void Awake()
        {
            CellexalError.ReferenceManager = referenceManager;
            CellexalError.errorPrefab = errorMessagePrefab;
        }
    }
}