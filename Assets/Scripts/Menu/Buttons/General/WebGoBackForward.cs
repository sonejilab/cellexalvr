namespace CellexalVR.Menu.Buttons.General
{
    /// <summary>
    /// Button on the web browser to go back or forward. Forward boolean is set in the inspector.
    /// </summary>
    public class WebGoBackForward : CellexalButton
    {
        public SimpleWebBrowser.WebBrowser webBrowser;
        public bool forward;

        protected override string Description
        {
            get { if (forward) { return "Go Forward"; } else return "Go Back"; }
        }

        public override void Click()
        {
            webBrowser.GoBackForward(forward);
        }
    }
}