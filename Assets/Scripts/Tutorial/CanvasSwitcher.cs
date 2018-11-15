using UnityEngine;

/// <summary>
/// 
/// </summary>
public class CanvasSwitcher : MonoBehaviour {

    public GameObject[] canvases;

    public void SwitchCanvas(int id)
    {
        for (int i = 0; i < canvases.Length; i++)
        {
            bool b = i == id;
            canvases[i].SetActive(b);
        }
    }
}
