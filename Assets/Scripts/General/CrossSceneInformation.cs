using UnityEngine;
using System.Collections;

namespace CellexalVR.General
{ 
    public static class CrossSceneInformation
    {
        public static bool Tutorial { get; set; }
        public static bool Spectator { get; set; }
        public static bool Ghost { get; set; }
        public static bool Normal { get; set; } = true;
        public static string Username { get; set; }
        public static string RScriptPath { get; set; }
    }
}
