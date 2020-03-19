using UnityEngine;

namespace CellexalVR.DesktopUI
{
    /// <summary>
    /// Validates if a profile name is valid.
    /// </summary>
    [CreateAssetMenu(fileName = "Profile Field Validator", menuName = "TextMeshPro/Profile Field Validator")]
    public class ProfileNameValidator : TMPro.TMP_InputValidator
    {

        public override char Validate(ref string text, ref int pos, char ch)
        {
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                if (invalidChar == ch)
                {
                    // invalid character, don't insert anything
                    return '\0';
                }
            }
            text += ch;
            pos++;
            return ch;
        }
    }
}
