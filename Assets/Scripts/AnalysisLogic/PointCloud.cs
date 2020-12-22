using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace AnalysisLogic
{
    public class PointCloud : MonoBehaviour
    {
        public Texture2D positionTextureMap;
        public float3 minCoordValues;
        public float3 maxCoordValues;
        public float3 longestAxis;
        public float3 scaledOffset;
        public float3 diffCoordValues;
        public Dictionary<string, float3> points = new Dictionary<string, float3>();
        public int pcID;
        public Dictionary<int, string> clusterDict = new Dictionary<int, string>();
        public Dictionary<string, Color> colorDict = new Dictionary<string, Color>();

        public void Initialize(int id)
        {
            // scaledCoordinates.Clear();
            minCoordValues = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxCoordValues = new float3(float.MinValue, float.MinValue, float.MinValue);
            pcID = id;
            gameObject.name = "PointCloud" + pcID;
        }

        public void CreatePositionTextureMap(List<float3> pointPositions)
        {
            int width = (int) math.ceil(math.sqrt(pointPositions.Count));
            int height = width;
            positionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            Color col;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (height * y);
                    if (ind >= pointPositions.Count) continue;
                    float3 pos = pointPositions[ind] + 0.5f;
                    // col = new Color32((byte)(pos.x * 255), (byte)(pos.y * 255), (byte)(pos.z * 255), 1);
                    col = new Color(pos.x, pos.y, pos.z, 1);
                    // col = Color.H
                    positionTextureMap.SetPixel(x, y, col);
                }
            }

            positionTextureMap.Apply();
            VisualEffect vfx = GetComponent<VisualEffect>();
            vfx.enabled = true;
            vfx.Play();
            vfx.SetTexture("PositionMapTex", positionTextureMap);
            vfx.SetInt("SpawnRate", 500000);
        }

        public void CreateColorTextureMap()
        {
            int pointCount = clusterDict.Count;
            int width = (int) math.ceil(math.sqrt(pointCount));
            int height = width;
            Texture2D colorMap = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (width * y);
                    if (ind >= pointCount) continue;
                    colorMap.SetPixel(x, y, colorDict[clusterDict[ind]]);
                }
            }

            colorMap.Apply();
            VisualEffect vfx = GetComponent<VisualEffect>();
            vfx.enabled = true;
            vfx.Play();
            vfx.SetTexture("ColorMapTex", colorMap);
        }

        public void AddGraphPoint(string cellName, float x, float y, float z)
        {
            points[cellName] = new float3(x, y, z);
            UpdateMinMaxCoords(x, y, z);
            // if (cells.Contains(cellName)) return;
            // PointSpawner.instance.cells.Add(cellName);
        }

        public void UpdateMinMaxCoords(float x, float y, float z)
        {
            if (x < minCoordValues.x)
                minCoordValues.x = x;
            if (y < minCoordValues.y)
                minCoordValues.y = y;
            if (z < minCoordValues.z)
                minCoordValues.z = z;
            if (x > maxCoordValues.x)
                maxCoordValues.x = x;
            if (y > maxCoordValues.y)
                maxCoordValues.y = y;
            if (z > maxCoordValues.z)
                maxCoordValues.z = z;
        }
    }
}