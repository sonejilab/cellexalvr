namespace CellexalVR.Interaction
{
    /// <summary>
    /// The keyboard that handles searching for genes.
    /// </summary>
    public class KeyboardGene : CurvedVRKeyboard.KeyboardStatus
    {
        public AutoCompleteList autoCompleteList;

        protected override void SpaceKey()
        {
            //base.SpaceKey();
            var type = autoCompleteList.LookUpName(output);
            autoCompleteList.ClearList();
            keyboardOutput.SendToTarget();
            keyboardOutput.Clear();
        }

        protected override void BackspaceKey()
        {
            //base.BackspaceKey();
            if (output.Length >= 1)
            {
                keyboardOutput.RemoveLetter();
                output = output.Remove(output.Length - 1, 1);
                autoCompleteList.KeyboardOutput = output;
                //autoCompleteList.KeyboardOutput = output;
                //referenceManager.gameManager.InformKeyClicked("backspace");
            }
        }

        protected override void TypeKey(char key)
        {
            //base.TypeKey(key);
            if (output.Length < maxOutputLength)
            {
                keyboardOutput.AddLetter(key);
                output = output + key.ToString();
                autoCompleteList.KeyboardOutput = output;
                //referenceManager.gameManager.InformKeyClicked(key.ToString());
            }

        }

    }
}