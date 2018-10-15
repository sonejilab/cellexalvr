using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Toggle on/off the web browser tool. This also activates the laser on the right hand so that one can interact with the browser.
/// </summary>
public class WebBrowserButton : CellexalToolButton
{
    private void Start()
    {
        SetButtonActivated(true);
    }

    protected override string Description
    {
        get { return "Toggle Web Browser"; }
    }


    protected override ControllerModelSwitcher.Model ControllerModel
    {
        get { return ControllerModelSwitcher.Model.WebBrowser; }
    }


}
