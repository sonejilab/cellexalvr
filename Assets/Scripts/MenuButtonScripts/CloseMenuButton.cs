using UnityEngine;

/// <summary>
/// Represents a button that closes a menu that is opened on top of the main menu.
/// </summary>
public class CloseMenuButton : CellexalButton
{
    public GameObject buttonsToActivate;
    public GameObject menuToClose;
    public TextMesh textMeshToUndarken;

    public bool deactivateMenu = false;

    protected override string Description
    {
        get
        {
            return "Close menu";
        }
    }

    protected override void Click()
    {
        spriteRenderer.sprite = standardTexture;
        controllerInside = false;
        descriptionText.text = "";
        if (deactivateMenu)
        {
            menuToClose.SetActive(false);
        }
        else
        {
            foreach (Renderer r in menuToClose.GetComponentsInChildren<Renderer>())
                r.enabled = false;
            foreach (Collider c in menuToClose.GetComponentsInChildren<Collider>())
                c.enabled = false;
        }
        //textMeshToUndarken.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        textMeshToUndarken.GetComponent<MeshRenderer>().enabled = true;
        foreach (CellexalButton b in buttonsToActivate.GetComponentsInChildren<CellexalButton>())
        {
            b.SetButtonActivated(true);
        }
    }
}

