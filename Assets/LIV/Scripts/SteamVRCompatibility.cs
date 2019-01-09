using System;

namespace LIV.SDK.Unity
{
    static class SteamVRCompatibility
    {
        public static bool IsAvailable;

        public static Type SteamVRCamera;
        public static Type SteamVRExternalCamera;
        public static Type SteamVRFade;

        static SteamVRCompatibility()
        {
            IsAvailable = FindSteamVRAsset();
        }

        static bool FindSteamVRAsset()
        {
            SteamVRCamera = Type.GetType("SteamVR_Camera", false);
            SteamVRExternalCamera = Type.GetType("SteamVR_ExternalCamera", false);
            SteamVRFade = Type.GetType("SteamVR_Fade", false);

            return SteamVRCamera != null &&
                   SteamVRExternalCamera != null &&
                   SteamVRFade != null;
        }
    }
}
