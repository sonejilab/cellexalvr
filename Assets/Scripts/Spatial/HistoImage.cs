using UnityEngine;
using System.Collections;
using UnityEngine.VFX;
using System.Collections.Generic;
using CellexalVR.General;
using System.IO;
using System.Threading;
using Unity.Mathematics;
using Valve.VR.InteractionSystem;
using Valve.VR.Extras;
using AnalysisLogic;
using DefaultNamespace;

namespace CellexalVR.Spatial
{
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
        private Texture2D colorTexture;
        private Texture2D alphaTexture;
        private Transform rayCastingSource;
        private Hand rightHand;
        private Vector2Int cropStart = new Vector2Int();
        private Vector3 startHit = new Vector3();
        private Vector3 originalScale = new Vector3();
        private float originalImageRatio;
        private int layerMask;
        private List<Vector2Int> tissueCoords = new List<Vector2Int>();
        private List<Vector3> scaledCoords = new List<Vector3>();
        private PointCloud pc;

        private void Start()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            visualEffect = GetComponentInChildren<VisualEffect>();
            rightHand = Player.instance.rightHand;
            if (rightHand)
            {
                rayCastingSource = rightHand.GetComponent<SteamVR_LaserPointer>().transform;
            }
            pc = GetComponent<PointCloud>();

            originalScale = transform.localScale;
            originalImageRatio = (float)texture.width / (float)texture.height;
            layerMask = 1 << LayerMask.NameToLayer("EnvironmentButtonLayer");

            maxValues = new Vector2(int.MinValue, int.MinValue);
            minValues = new Vector2(int.MaxValue, int.MaxValue);
        }

        public void Initialize()
        {
            StartCoroutine(InitializeCoroutine());
        }

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


        private IEnumerator InitializeCoroutine()
        {
            yield return null;
            ScaleCoordinates();
            int width = 1000;
            int height = (int)math.ceil(tissueCoords.Count / 1000f);
            float maxX = 0f;
            float minX = 0f;
            float maxY = 0f;
            float minY = 0f;
            posTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
            colorTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
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

            //posTexture.SetPixels(positions);
            //colorTexture.SetPixels(colors);
            //alphaTexture.SetPixels(alphas);
            image.GetComponent<MeshRenderer>().material.mainTexture = texture;

            //posTexture.Apply();
            //colorTexture.Apply();
            //alphaTexture.Apply();
            //visualEffect.SetTexture("PositionMapTex", posTexture);
            //visualEffect.SetTexture("ColorMapTex", colorTexture);
            //visualEffect.SetTexture("AlphaMapTex", alphaTexture);
            //visualEffect.SetInt("SpawnRate", (width * height));



            visualEffect.Stop();
            visualEffect.Play();
        }

        public void CropToTissue()
        {
            Vector2 c1 = PointToTexture(maxValues.x, maxValues.y);
            Vector2 c4 = PointToTexture(minValues.x, minValues.y);

            c1.x += 5;
            c1.y -= 5;
            c4.x -= 5;
            c4.y += 5;

            CropImage((int)((c1.x)), (int)((c1.y)), (int)((c4.x)), (int)((c4.y)));
        }

        public void CropToTissue2()
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

            //print(textureCoords.Count);
            //int j = 0;
            //foreach (Vector2Int tissueCoord in textureCoords.Values)
            //{
            //    Vector2 texCoord = PointToTexture((float)tissueCoord.x, (float)tissueCoord.y);
            //    //print($"{texCoord}");
            //    if (j++ < 250)
            //    {
            //        print($"{texCoord}, {tissueCoord}");
            //    }
            //    //texture.SetPixel((int)tissueCoord.x, (int)tissueCoord.y, Color.white);
            //    texture.SetPixel((int)texCoord.y, (int)texCoord.x, Color.red);
            //}
            texture.Apply();
        }



        private Vector2 PointToTexture(float x, float y)
        {
            Vector2 textureCoord = new Vector2(x, y);
            textureCoord.x = y;
            textureCoord.y = texture.height - x;
            return textureCoord;
        }

        public void ScaleCoordinates()
        {
            texMaxValues = new Vector3(texture.width, texture.height, 0f);
            //texMaxValues = new Vector3(1200, 1200, 0f);
            texMinValues = new Vector3(0f, 0f, 0f);
            diffCoordValues = texMaxValues - texMinValues;
            //diffCoordValues = maxValues - minValues;
            longestAxis = Mathf.Max(diffCoordValues.x, diffCoordValues.y);
            //longestAxis = Mathf.Max(texture.width, texture.height);
            scaledOffset = (diffCoordValues / longestAxis) / 2;
        }

        public Vector3 ScaleCoordinate(Vector3 coord)
        {
            Vector3 scaledCoord = coord - texMinValues;
            scaledCoord /= longestAxis;
            scaledCoord -= scaledOffset;

            return scaledCoord;
        }

        public void AddGraphPoint(int x, int y)
        {
            tissueCoords.Add(new Vector2Int(x, y));
            UpdateMinMaxCoords(x, y);
        }


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

        private void ImageRayCast()
        {
            Physics.Raycast(rayCastingSource.position, rayCastingSource.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity, layerMask);
            //Debug.DrawRay(rayCastingSource.position, rayCastingSource.TransformDirection(Vector3.forward));
            int hitx = 0;
            int hity = 0;
            if (hit.collider && hit.transform == image.transform)
            {
                hitx = (int)(hit.textureCoord.x * texture.width);
                hity = (int)(hit.textureCoord.y * texture.height);
                if (rightHand.grabPinchAction.GetStateDown(rightHand.handType))
                {
                    // Save coordinates when button is first pressed.
                    foreach (LineRenderer lr in lines)
                    {
                        lr.gameObject.SetActive(true);
                    }
                    cropStart.x = hitx;
                    cropStart.y = hity;
                    startHit = hit.point;
                }
                if (rightHand.grabPinchAction.GetState(rightHand.handType))
                {
                    // Show crop area with lines while button is down.
                    Vector3 start = startHit;
                    Vector3 corner2 = new Vector3(hit.point.x, startHit.y, startHit.z);
                    Vector3 corner3 = new Vector3(hit.point.x, hit.point.y, startHit.z);
                    Vector3 corner4 = new Vector3(startHit.x, hit.point.y, startHit.z);

                    //Vector3 corner5 = new Vector3(startHit.x, startHit.y, hit.point.z);
                    //Vector3 corner6 = new Vector3(hit.point.x, startHit.y, hit.point.z);
                    //Vector3 corner7 = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                    //Vector3 corner8 = new Vector3(startHit.x, hit.point.y, hit.point.z);

                    lines[0].SetPositions(new Vector3[] { start, corner2 });
                    lines[1].SetPositions(new Vector3[] { corner2, corner3 });
                    lines[2].SetPositions(new Vector3[] { corner3, corner4 });
                    lines[3].SetPositions(new Vector3[] { corner4, start });

                    //lines[4].SetPositions(new Vector3[] { start, corner5 });
                    //lines[5].SetPositions(new Vector3[] { corner2, corner6 });
                    //lines[6].SetPositions(new Vector3[] { corner3, corner7 });
                    //lines[7].SetPositions(new Vector3[] { corner4, corner8 });

                    //lines[8].SetPositions(new Vector3[] { corner5, corner6});
                    //lines[9].SetPositions(new Vector3[] { corner6, corner7 });
                    //lines[10].SetPositions(new Vector3[] { corner7, corner8 });
                    //lines[11].SetPositions(new Vector3[] { corner8, corner5 });

                }
            }
            if (rightHand.grabPinchAction.GetStateUp(rightHand.handType))
            {
                // Do cropping when button is released.
                foreach (LineRenderer lr in lines)
                {
                    lr.gameObject.SetActive(false);
                }
                if (math.abs(hitx - cropStart.x) > 1 && math.abs(hity - cropStart.y) > 1)
                {
                    CropImage((int)cropStart.x, (int)cropStart.y, hitx, hity);
                    CropPoints(startHit, hit.point);
                }
            }

        }

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
                colors[i].a = 0.01f;// = new Color(0.1f, 0.1f, 0.1f, 0.1f);
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

        private void CropPoints(Vector3 startPos, Vector3 endPos)
        {
            startPos = visualEffect.transform.InverseTransformPoint(startPos);
            endPos = visualEffect.transform.InverseTransformPoint(endPos);
            //if (startPos.x > endPos.x)
            //{
            //    float temp = startPos.x;
            //    startPos.x = endPos.x;
            //    endPos.x = temp;
            //}

            //if (startPos.y > endPos.y)
            //{
            //    float temp = startPos.y;
            //    startPos.y = endPos.y;
            //    endPos.y = temp;
            //}
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
                    //alphas[i] = new Color(0.01f, 0.01f, 0.01f);
                    alphas[i] = Color.black;
                }
            }
            alphaTexture.SetPixels(alphas);
            alphaTexture.Apply();
        }

        private void Update()
        {
            //ImageRayCast();
            UpdateColorTexture();


        }
    }
}
