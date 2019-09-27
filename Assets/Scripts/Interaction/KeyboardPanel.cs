using System.Collections;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// A clickable panel on the keyboard. Can be a button that adds a character to the output or switches the layout of the keyboard for example.
    /// </summary>
    public class KeyboardPanel : ClickablePanel
    {
        public KeyboardHandler handler;


        public enum Type
        {
            /// <summary>
            /// The character in <see cref="Text"/> will be added to the output.
            /// </summary>
            Character,
            /// <summary>
            /// Switches between upper and lowercase.
            /// </summary>
            Shift,
            /// <summary>
            /// Removes the last typed character.
            /// </summary>
            Back,
            /// <summary>
            /// Removes all typed characters.
            /// </summary>
            Clear,
            /// <summary>
            /// Switches between lowercase and the numbers and special layout.
            /// </summary>
            NumChar,
            /// <summary>
            /// Colors all graphs based on the typed characters.
            /// </summary>
            Enter
        }

        public Type keyType;
        public bool replaceOutputs = false;
        private string text;

        /// <summary>
        /// The text that is displayed on the button.
        /// If <see cref="keyType"/> is set the <see cref="Type.Character"/> this button will add that to the keyboard output.
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                transform.parent.GetComponentInChildren<TMPro.TextMeshPro>().text = value + "";
                text = value;
            }
        }


        /// <summary>
        /// Handles what happens when a user clicks this button.
        /// </summary>
        public override void Click()
        {
            switch (keyType)
            {
                case Type.Character:
                    if (replaceOutputs)
                    {
                        handler.Clear();
                    }
                    handler.AddText(Text, true);
                    break;
                case Type.Shift:
                    handler.Shift();
                    break;
                case Type.Back:
                    handler.BackSpace(true);
                    break;
                case Type.Clear:
                    handler.Clear(true);
                    break;
                case Type.NumChar:
                    handler.NumChar();
                    break;
                case Type.Enter:
                    handler.SubmitOutput();
                    break;
            }
        }

    }
}
