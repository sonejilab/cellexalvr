using UnityEngine;
using System.Collections;


namespace CellexalVR.Interaction
{
    public class OperatorKeyboardHandler : KeyboardHandler
    {
        public override string[][] Layouts { get; protected set; } = {
            new string[] { "=", "!=",
                           ">", "<",
                           ">=", "<=" }
        };
    }
}
