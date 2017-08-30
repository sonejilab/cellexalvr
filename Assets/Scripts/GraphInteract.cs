using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VRTK;


class GraphInteract : VRTK_InteractableObject
{
    public MagnifierTool magnifier;
    private bool magnifierDisabled;

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        magnifierDisabled = magnifier.gameObject.activeSelf;
        magnifier.enabled = false;
        base.OnInteractableObjectGrabbed(e);
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {

        magnifier.enabled = true;
        base.OnInteractableObjectUngrabbed(e);
    }
}
