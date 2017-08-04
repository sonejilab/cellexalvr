using System.Collections;
using UnityEngine;

/// <summary>
/// This class changes the right controller's model at the start of the program, this is needed because the controllermodelswitcher's gameobject is inactive so it can't run a coroutine
/// </summary>
class ChangeModelAtStart : MonoBehaviour
{

    public ControllerModelSwitcher modelSwitcher;

    private void Start()
    {
        StartCoroutine(ChangeModel());
    }

    IEnumerator ChangeModel()
    {
        while (!modelSwitcher.Ready())
            yield return null;
        modelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Normal);
    }
}

