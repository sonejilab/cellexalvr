using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Class that represents a hisology image for spatial data. Image coordinates are linked to graph points.
    /// </summary>
    public class HistoImage : MonoBehaviour
    {
        public Texture2D texture;
        public string file;
        public LineRenderer[] lines = new LineRenderer[4];
        public Vector3 maxValues;
        public Vector3 minValues;
        public Vector3 scaledMaxValues;
        public Vector3 scaledMinValues;
        public Vector3 texMaxValues;
        public Vector3 texMinValues;
        public Dictionary<string, Vector2Int> textureCoords = new Dictionary<string, Vector2Int>();
        public float longestAxis;
        public Vector3 scaledOffset;
        public int sliceNr;
        public GameObject image;
        public VisualEffect visualEffect;

        private Vector3 diffCoordValues;

        private Texture2D posTexture;
        private Texture2D alphaTexture;
        private List<Vector2Int> tissueCoords = new List<Vector2Int>();
        private PointCloud pc;

        private void Start()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            visualEffect = GetComponentInChildren<VisualEffect>();
            pc = GetComponent<PointCloud>();
            maxValues = new Vector2(int.MinValue, int.MinValue);
            minValues = new Vector2(int.MaxValue, int.MaxValue);
            image = GetComponent<GraphSlice>().image;
            transform.Rotate(0, 0, -90);
            CellexalEvents.ColorTextureUpdated.AddListener(UpdateColorTexture);
        }

        /// <summary>
        /// Initialize by setting texture coordinates and linking to graph points.
        /// </summary>
        public void Initialize()
        {
            InitializeCoroutine();
        }
        private void InitializeCoroutine()
        {
            ScaleCoordinates();
            int width = 1000;
            int height = (int)math.ceil(tissueCoords.Count / 1000f);
            float maxX = 0f;
            float minX = 0f;
            float maxY = 0f;
            float minY = 0f;
            posTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            alphaTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            Color[] positions = new Color[width * height];
            Color[] colors = new Color[width * height];
            Color[] alphas = new Color[width * height];
            Color c = new Color(1f, 1f, 1f);
            Color alpha = new Color(0.15f, 0.15f, 0.15f);
            int[] maxInds = new int[4];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (width * y);
                    if (ind >= tissueCoords.Count) continue;
                    Vector3Int coord = new Vector3Int(tissueCoords[ind].x, tissueCoords[ind].y, 0);
                    Vector3 scaledCoord = ScaleCoordinate(coord);
                    Color pos = new Color(scaledCoord.x, scaledCoord.y, 0);
                    // switch r & g bc image is rotated..
                    if (pos.g > maxX)
                    {
                        maxX = pos.g;
                        maxInds[0] = ind;
                    }

                    if (pos.g < minX)
                    {
                        minX = pos.g;
                        maxInds[1] = ind;
                    }
                    if (pos.r > maxY)
                    {
                        maxY = pos.r;
                        maxInds[2] = ind;
                    }
                    if (pos.r < minY)
                    {
                        minY = pos.r;
                        maxInds[3] = ind;
                    }

                    positions[ind] = pos;
                    colors[ind] = c;
                    alphas[ind] = alpha;
                }
            }

            scaledMaxValues = new Vector2(maxX, maxY);
            scaledMinValues = new Vector2(minX, minY);
            image.GetComponent<MeshRenderer>().material.mainTexture = texture;
            visualEffect.Stop();
            visualEffect.Play();
        }


        /// <summary>
        /// Similar to how a graph slice updates to its parent points cloud a histology image is seen as a subset of all the data and images loaded.
        /// This method makes sure the color of the points are in synch with the color texture map of the entire dataset.
        /// </summary>
        public void UpdateColorTexture()
        {
            if (pc.points.Count > 0)
            {
                Texture2D parentTexture = TextureHandler.instance.mainColorTextureMaps[0];
                Texture2D parentATexture = TextureHandler.instance.alphaTextureMaps[0];
                Color c;
                Color alpha;
                foreach (KeyValuePair<string, float3> kvp in pc.points)
                {
                    Vector2Int textureCoord = TextureHandler.instance.textureCoordDict[kvp.Key];
                    c = parentTexture.GetPixel(textureCoord.x, textureCoord.y);
                    alpha = parentATexture.GetPixel(textureCoord.x, textureCoord.y);
                    Vector2Int hiTexCoord = textureCoords[kvp.Key];
                    pc.colorTextureMap.SetPixel(hiTexCoord.x, hiTexCoord.y, c);
                    pc.alphaTextureMap.SetPixel(hiTexCoord.x, hiTexCoord.y, alpha);
                }

                pc.colorTextureMap.Apply();
                pc.alphaTextureMap.Apply();
            }
        }

        /// <summary>
        /// Crops outside parts of texture that does not have tissue on it.
        /// </summary>
        public void CropToTissue()
        {
            Color[] colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].a = 0f;
            }

            texture.SetPixels(colors);

            foreach (KeyValuePair<string, float3> kvp in pc.points)
            {
                float3 point = kvp.Value;
                Vector2 texCoord = PointToTexture(point.x, point.y);
                for (int i = -5; i <= 5; i++)
                {
                    for (int j = -5; j <= 5; j++)
                    {
                        Vector2Int coord = new Vector2Int((int)texCoord.x + i, (int)texCoord.y + j);
                        Color c = texture.GetPixel(coord.x, coord.y);
                        c.a = 1;
                        texture.SetPixel((int)texCoord.x + i, (int)texCoord.y + j, c);
                    }

                }
            }
            texture.Apply();
        }


        /// <summary>
        /// Helper function to go from a graph point to a coordinate on the tissue texture.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Vector2 PointToTexture(float x, float y)
        {
            Vector2 textureCoord = new Vector2(x, y);
            textureCoord.x = y;
            textureCoord.y = texture.height - x;
            return textureCoord;
        }

        /// <summary>
        /// Helper function to scale all point coordinates.
        /// </summary>
        public void ScaleCoordinates()
        {
            texMaxValues = new Vector3(texture.width, texture.height, 0f);
            texMinValues = new Vector3(0f, 0f, 0f);
            diffCoordValues = texMaxValues - texMinValues;
            longestAxis = Mathf.Max(diffCoordValues.x, diffCoordValues.y);
            scaledOffset = (diffCoordValues / longestAxis) / 2;
        }

        /// <summary>
        /// Helper function to scale a coordinate.
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public Vector3 ScaleCoordinate(Vector3 coord)
        {
            Vector3 scaledCoord = coord - texMinValues;
            scaledCoord /= longestAxis;
            scaledCoord -= scaledOffset;
            return scaledCoord;
        }

        /// <summary>
        /// Add graph point and update the min max coordinates later used for scaling the graph.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddGraphPoint(int x, int y)
        {
            tissueCoords.Add(new Vector2Int(x, y));
            UpdateMinMaxCoords(x, y);
        }

        /// <summary>
        /// Updates min max coordinates that are then used for scaling the graph.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void UpdateMinMaxCoords(int x, int y)
        {
            if (x > maxValues.x)
            {
                maxValues.x = x;
            }
            if (y > maxValues.y)
            {
                maxValues.y = y;
            }
            if (x < minValues.x)
            {
                minValues.x = x;
            }
            if (y < minValues.y)
            {
                minValues.y = y;
            }
        }

        /// <summary>
        /// Crop image based on the given coordinates. Mark a certain are in the image and crop away the outside of that area.
        /// </summary>
        /// <param name="startX">Start from this x coordinate.</param>
        /// <param name="startY">Start from this y coordinate.</param>
        /// <param name="endX">The end x coordinate.</param>
        /// <param name="endY">End y coordinate. The parameters together make up a square to crop (or rather inverse crop).</param>
        private void CropImage(int startX, int startY, int endX, int endY)
        {
            if (startX > endX)
            {
                int tempX = startX;
                startX = endX;
                endX = tempX;
            }

            if (startY > endY)
            {
                int tempY = startY;
                startY = endY;
                endY = tempY;
            }

            Texture2D croppedIm = new Texture2D(texture.width, texture.height, TextureFormat.RGBAFloat, true, true);
            Color[] colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].a = 0.01f;
            }
            croppedIm.SetPixels(colors);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    Color c = texture.GetPixel(x, y);
                    croppedIm.SetPixel(x, y, c);
                }
            }
            croppedIm.Apply();
            image.GetComponent<MeshRenderer>().material.mainTexture = croppedIm;
        }

        /// <summary>
        /// Crop are of tissue to only show the parts that you are interested in. 
        /// </summary>
        /// <param name="startPos">Start point to crop from.</param>
        /// <param name="endPos">End point to crop from.</param>
        private void CropPoints(Vector3 startPos, Vector3 endPos)
        {
            startPos = visualEffect.transform.InverseTransformPoint(startPos);
            endPos = visualEffect.transform.InverseTransformPoint(endPos);
            Color[] positions = posTexture.GetPixels();
            Color[] alphas = alphaTexture.GetPixels();
            for (int i = 0; i < positions.Length; i++)
            {
                Color pos = positions[i];
                if (pos.r > startPos.x && pos.r < endPos.x && pos.g > endPos.y && pos.g < startPos.y)
                {
                    alphas[i] = new Color(0.4f, 0.4f, 0.4f);
                }
                else
                {
                    alphas[i] = Color.black;
                }
            }
            alphaTexture.SetPixels(alphas);
            alphaTexture.Apply();
        }
    }
}
