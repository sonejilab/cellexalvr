using System.Linq;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Keyboard for web browser. Enter key sends output to navigate function in web browser.
    /// </summary>
    public class KeyboardWebBrowser : CurvedVRKeyboard.KeyboardStatus
    {

        public SimpleWebBrowser.WebBrowser webBrowser;

        // Use this for initialization
        void Start()
        {
        }

        protected override void BackspaceKey()
        {
            if (output.Length >= 1)
            {
                keyboardOutput.RemoveLetter();
                output = output.Remove(output.Length - 1, 1);
            }
        }

        protected override void SpaceKey()
        {
            TypeKey(' ');
        }

        protected override void TypeKey(char key)
        {
            if (output.Length < maxOutputLength)
            {
                keyboardOutput.AddLetter(key);
                output = output + key.ToString();
            }
        }

        public override void ClearKey()
        {
            base.ClearKey();
            output = "";
        }

        public override void EnterKey()
        {
            print("Navigate to - " + output);
            // If url field does not contain '.' then may not be a url so google the output instead
            if (!output.Contains('.'))
            {
                output = "www.google.com/search?q=" + output;
            }
            webBrowser.OnNavigate(output);
        }


    }
}