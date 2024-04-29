using CellexalVR.AnalysisLogic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace CellexalVR.Spatial
{
    public class CellToCellTracking : MonoBehaviour
    {
        [SerializeField] private VisualEffect vfx;

        private Texture2D positionMap;
        private Texture2D endPositionMap;
        private Texture2D startPositionMap;

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                SpawnLines();
            }
        }

        private void SpawnLines()
        {
            Transform pc1 = PointCloudGenerator.instance.pointClouds[0].transform;
            Transform pc2 = PointCloudGenerator.instance.pointClouds[1].transform;

            Color[] startPositions = PointCloudGenerator.instance.pointClouds[0].positionTextureMap.GetPixels();
            Color[] endPositions = PointCloudGenerator.instance.pointClouds[1].positionTextureMap.GetPixels();

            Color[] worldStartPosition = new Color[endPositions.Length];
            Color[] worldEndPositions = new Color[endPositions.Length];
            Color[] velocities = new Color[endPositions.Length];
            Vector3 diff;
            Vector3 start;
            Vector3 end;
            for (int i = 0; i < startPositions.Length; i++)
            {
                start = new Vector3(startPositions[i].r, startPositions[i].g, startPositions[i].b);
                start = pc1.TransformPoint(start);
                end = new Vector3(endPositions[i].r, endPositions[i].g, endPositions[i].b);
                end = pc2.TransformPoint(end);
                worldStartPosition[i] = new Color(start.x, start.y, start.z);
                worldEndPositions[i] = new Color(end.x, end.y, end.z);
                //diff = (pc1.TransformPoint(start) - pc2.TransformPoint(end));
                //velocities[i] = new Color(diff.x, diff.y, diff.z);
                if (i < 10)
                {
                    print($"start pos {i}: {worldStartPosition[i].r}, {worldStartPosition[i].g}, {worldStartPosition[i].b}");
                    print($"end pos {i}: {worldEndPositions[i].r}, {worldEndPositions[i].g}, {worldEndPositions[i].b}");
                }
            }

            positionMap = PointCloudGenerator.instance.pointClouds[0].positionTextureMap;
            startPositionMap = new Texture2D(positionMap.width, positionMap.height, TextureFormat.RGBAFloat, false, true);
            endPositionMap = new Texture2D(positionMap.width, positionMap.height, TextureFormat.RGBAFloat, false, true);
            endPositionMap.SetPixels(endPositions);
            endPositionMap.Apply();
            startPositionMap.SetPixels(startPositions);
            startPositionMap.Apply();

            vfx.SetTexture("PositionMap", endPositionMap);
            vfx.SetTexture("VelocityMap", startPositionMap);

            vfx.enabled = true;
        }
    }
}
