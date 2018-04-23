using System;
using UnityEngine;

/// <summary>
/// Represents the button that opens the color by gene menu.
/// </summary>
public class ColorByGeneMenuButton : CellexalButton
{
    public TextMesh textMeshToDarken;

    private GameObject buttons;
    private ColorByGeneMenu colorByGeneMenu;

    protected override string Description
    {
        get
        {
            return "Show menu for calculating\ntop differentially expressed genes";
        }
    }

    void Start()
    {
        buttons = referenceManager.leftButtons;
        colorByGeneMenu = referenceManager.colorByGeneMenu;
        colorByGeneMenu.gameObject.SetActive(true);
        colorByGeneMenu.SetMenuVisible(false);
    }

    protected override void Click()
    {
        spriteRenderer.sprite = standardTexture;
        controllerInside = false;
        descriptionText.text = "";
        colorByGeneMenu.SetMenuVisible(true);

        foreach (StationaryButton b in buttons.GetComponentsInChildren<StationaryButton>())
        {
            b.SetButtonActivated(false);
        }
        textMeshToDarken.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
    }
}
