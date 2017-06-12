# vrJeans

When project is pulled down, an 'Untitled' scene will show. Double-click on Assets > vrjeans_scene1 to get the correct scene.

To be able to see scenes/projects with the HTC Vive:
In Unity,
  1. Add necessary asset: Prefabs > SteamVR > Prefabs, drag CameraRig to scene. 
  2. To make controllers visible in VE (Virtual Environment): for [CameraRig]>( Controller (left) | Controller (right) ), set 'Steam VR_Tracked Object (Script)' > Index to Device 1 respectively Device 2 (one for left and one for right, mapping of left/right to controller doesn't matter, the important thing is the mapping).
  3. To make HMD work: for [CameraRig]>Camera(head)>Camera(eye), add script 'Steam VR_Update Poses (Script)' (Add Component, find script in that drop-down list).

USE UNITY 5.6.1F1 AND R 3.4.0 OR EVERYTHING IS BREAKS. please