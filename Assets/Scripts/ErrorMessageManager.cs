using UnityEngine;

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
