using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

public class ShowInfoButton : CellexalButton
{
    protected override string Description
    {
        get { return "Show info"; }
    }
    public GameObject infoCanvas;

    protected override void Awake()
    {
        base.Awake();
        //CellexalEvents.NetworkEnlarged.AddListener(TurnOn);
        //CellexalEvents.NetworkUnEnlarged.AddListener(TurnOff);
        TurnOff();
    }

    protected override void Click()
    {
        infoCanvas.SetActive(!infoCanvas.activeSelf);
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }


}
