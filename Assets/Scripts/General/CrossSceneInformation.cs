using UnityEngine;
using System.Collections;

namespace CellexalVR.General
{ 
    public class CrossSceneInformation : MonoBehaviour
    {
        public static bool Tutorial { get; set; }
        public static bool Spectator { get; set; }
        public static bool Ghost { get; set; }
        public static string Username { get; set; }

    }
}
