using UnityEngine;

///<summary>
/// Represents a button used for resetting the input data folders. 
/// It is spawned on a confirm submenu (which is spawned by the "Loading Menu Sub button" and if it is pressed closes this submenu.
///</summary>
public class ResetFolderButton : CellexalButton
{
    //public GameObject subMenu;
    public GameObject buttonsToActivate;
    public GameObject menuToClose;
    public TextMesh textMeshToUndarken;

    public bool deactivateMenu = false;

    protected override string Description
    {
        get
        {
            return "Go back to loading a folder";
        }
    }

    private void Start()
    {

    }

    // Reset everything without clicking the button.
    public void Reset()
    {
        Click();
    }

    protected override void Click()
    {
        CloseSubMenu();
        referenceManager.loaderController.ResetFolders();
        referenceManager.gameManager.InformLoadingMenu();
    }

    void CloseSubMenu()
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
