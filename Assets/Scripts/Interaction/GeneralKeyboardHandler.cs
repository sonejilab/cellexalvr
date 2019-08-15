using UnityEngine;
using System.Collections;

namespace CellexalVR.Interaction
{
    public class GeneralKeyboardHandler : KeyboardHandler
    {
        public override string[][] Layouts { get; protected set; } = {
            // lowercase
            new string[] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p",
                           "Shift", "a", "s", "d", "f", "g", "h", "j", "k", "l",
                           "123\n!#%", "z", "x", "c", "v", "b", "n", "m", "Back", "Clear",
                           "Enter"},
            // uppercase
            new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P",
                           "Shift", "A", "S", "D", "F", "G", "H", "J", "K", "L",
                           "123\n!#%", "Z", "X", "C", "V", "B", "N", "M", "Back", "Clear",
                           "Enter"},
            // special
            new string[] {  "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
                            "Shift", "!", "#", "%", "&", "/", "(", ")", "=", "@",
                            "ABC\nabc", "\\", "-", "_", ".", ":", ",", ";", "Back", "Clear",
                            "Enter" }
        };

        /// <summary>
        /// Switches between uppercase and lowercase layout.
        /// </summary>
        public override void Shift()
        {
            if (CurrentLayout == 0)
            {
                SwitchLayout(Layouts[1]);
                CurrentLayout = 1;
            }
            else if (CurrentLayout == 1)
            {
                SwitchLayout(Layouts[0]);
                CurrentLayout = 0;
            }
        }

        /// <summary>
        /// Switches between lowercase and special layout.
        /// </summary>
        public override void NumChar()
        {
            if (CurrentLayout == 2)
            {
                SwitchLayout(Layouts[0]);
            }
            else
            {
                SwitchLayout(Layouts[2]);
            }
        }
    }
}
