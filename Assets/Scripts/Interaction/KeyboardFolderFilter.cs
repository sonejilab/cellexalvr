namespace CellexalVR.Interaction
{

    /// <summary>
    /// The keyboard that handles searching for data folders. Similar to gene keyboard but handles key click
    /// differently.
    /// </summary>
    public class KeyboardFolderFilter : CurvedVRKeyboard.KeyboardStatus
    {

        protected override void BackspaceKey()
        {
            if (output.Length >= 1)
            {
                keyboardOutput.RemoveLetter();
                output = output.Remove(output.Length - 1, 1);
                referenceManager.inputFolderGenerator.GenerateFolders(keyboardOutput.textMesh.text);
            }
        }

        protected override void TypeKey(char key)
        {
            if (output.Length < maxOutputLength)
            {
                keyboardOutput.AddLetter(key);
                output = output + key.ToString();
                referenceManager.inputFolderGenerator.GenerateFolders(keyboardOutput.textMesh.text);
            }
        }

        protected override void SpaceKey()
        {
            TypeKey(' ');
        }

        public override void ClearKey()
        {
            base.ClearKey();
            referenceManager.inputFolderGenerator.GenerateFolders(keyboardOutput.textMesh.text);
        }

    }
}