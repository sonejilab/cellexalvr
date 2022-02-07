using UnityEngine;
using System.Collections;
using CellexalVR.Menu.Buttons;
using TMPro;
using CellexalVR.Spatial;

public class ScrollSuggestionsButton : CellexalButton
{
    public int dir;

    protected override string Description => "Scroll " + (dir > 0 ? "Up" : "Down");

    public override void Click()
    {
        AllenReferenceBrain.instance.ScrollSuggestions(dir);
    }

}
