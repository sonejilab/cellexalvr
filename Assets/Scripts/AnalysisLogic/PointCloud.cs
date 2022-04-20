using System.Collections;
using System.Collections.Generic;
using CellexalVR;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Spatial;
using CellexalVR.AnalysisLogic
    ;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using UnityEngine.Pool;

namespace AnalysisLogic
{
    public class PointCloud : MonoBehaviour
    {
        private VisualEffect vfx;
        private int pointCount;
        private bool spawn = true;
        private EntityManager entityManager;
        private int frameCount;
        private bool morphed;
        [SerializeField] private SlicerBox slicerBoxPrefab;

        private Dictionary<string, Color> clusterCentroids = new Dictionary<string, Color>();

        public GameObject sliceImage;
        public VisualEffectAsset pointCloudHighCap;
        public VisualEffectAsset pointCloudQuad;
        public Texture2D positionTextureMap;
        public Texture2D orgPositionTextureMap;
        public Texture2D colorTextureMap;
        public Texture2D alphaTextureMap;
        public Texture2D targetPositionTextureMap;
        public Texture2D morphTexture;
        public Texture2D clusterSpreadTexture;
        public Texture2D pointSpreadTexture;
        public float3 minCoordValues;
        public float3 maxCoordValues;
        public float3 longestAxis;
        public float3 scaledOffset;
        public float3 diffCoordValues;
        public Dictionary<string, float3> points = new Dictionary<string, float3>();
        public int pcID;
        //public Transform selectionSphere;
        public Entity parent;
        public List<float> zPositions = new List<float>();
        public GraphSlice graphSlice;
        public GameObject infoParent;
        public TextMeshPro graphNameText;
        public TextMeshPro graphInfoText;
        public string otherName;
        public string originalName;

        public string GraphName
        {
            get => graphName;
            set
            {
                graphName = value;
                // We don't want two objects with the exact same name. Could cause issues in find graph and in multi user sessions.
                //GameObject existingGraph = GameObject.Find(graphName);
                //while (existingGraph != null)
                //{
                //    graphName += "_Copy";
                //    existingGraph = GameObject.Find(graphName);
                //}

                graphNameText.text = graphName;
                name = graphName;
                gameObject.name = graphName;
            }
        }

        private string graphName;
        private string folderName;

        public void Initialize(int id)
        {
            minCoordValues = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxCoordValues = new float3(float.MinValue, float.MinValue, float.MinValue);
            pcID = id;
            gameObject.name = "PointCloud" + pcID;
            vfx = GetComponent<VisualEffect>();
            if (vfx == null)
            {
                vfx = GetComponentInChildren<VisualEffect>();
            }
            //selectionSphere = SelectionToolCollider.instance.transform;
            vfx.pause = true;
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //interactableObjectBasic = GetComponent<InteractableObjectBasic>();
            //interactableObjectBasic.InteractableObjectGrabbed += OnGrabbed;
            //interactableObjectBasic.InteractableObjectUnGrabbed += OnUnGrabbed;
            //slicerBox = GetComponentInChildren<SlicerBox>(true);
            graphSlice = GetComponent<GraphSlice>();
        }

        private void Update()
        {
            //if ((Player.instance.rightHand != null && controllerAction.GetStateDown(Player.instance.rightHand.handType)) || Input.GetKeyDown(KeyCode.M))
            //{
            //    targetPositionTextureMap = morphTexture;
            //    vfx.SetTexture("TargetPosMapTex", targetPositionTextureMap);
            //    StartCoroutine(Morph());
            //}

            //if (Player.instance.leftHand != null && controllerAction.GetStateDown(Player.instance.leftHand.handType))
            //{
            //    SpreadOutClusters();
            //}

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                SpreadOutPoints();
            }
            if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                SpreadOutClusters();
            }


        }

        //private void OnGrabbed(object sender, Hand hand)
        //{
        //    foreach (CullingWall cw in slicerBox.cullingWalls)
        //    {
        //        cw.GetComponent<InteractableObjectBasic>().isGrabbable = false;
        //    }
        //}

        //private void OnUnGrabbed(object sender, Hand hand)
        //{
        //    foreach (CullingWall cw in slicerBox.cullingWalls)
        //    {
        //        cw.GetComponent<InteractableObjectBasic>().isGrabbable = true;
        //    }
        //}

        public void ToggleInfoText()
        {
            infoParent.SetActive(!infoParent.gameObject.activeSelf);
        }


        public void SetCollider(bool offset = false)
        {
            diffCoordValues = maxCoordValues - minCoordValues;
            float3 mid = (minCoordValues + maxCoordValues) / 2;
            longestAxis = math.max(diffCoordValues.x, math.max(diffCoordValues.y, diffCoordValues.z));
            scaledOffset = (diffCoordValues / longestAxis) / 2;
            var bc = GetComponent<BoxCollider>();
            if (bc == null) return;
            if (offset)
            {
                bc.center = mid;
            }
            bc.size = scaledOffset * 2;
            var slicerBox = GetComponentInChildren<SlicerBox>(true);
            //slicerBox = Instantiate(slicerBoxPrefab, transform);
            if (slicerBox != null)
            {
                slicerBox.box.transform.localScale = bc.size;
                //slicerBox.box.transform.localPosition = bc.center - Vector3.one * 0.5f;
                slicerBox.SetHandlePositions();
                slicerBox.transform.localPosition = bc.center;
            }
            if (TryGetComponent(out GraphSlice graphSlice))
            {
                graphSlice.slicerBox = slicerBox;
            }
            //slicerBox.gameObject.SetActive(false);
        }

        public void SetCollider(Vector3 mid, Vector3 size)
        {
            diffCoordValues = maxCoordValues - minCoordValues;
            longestAxis = math.max(diffCoordValues.x, math.max(diffCoordValues.y, diffCoordValues.z));
            scaledOffset = (diffCoordValues / longestAxis) / 2;
            var bc = GetComponent<BoxCollider>();
            bc.center = mid;
            bc.size = size;
            var slicerBox = GetComponentInChildren<SlicerBox>(true);
            if (slicerBox != null)
            {
                slicerBox.box.transform.localScale = bc.size;
                slicerBox.box.transform.localPosition = bc.center;// - Vector3.one * 0.5f;
            }
        }

        public void SetTargetTexture(Color[] newCols)
        {
            targetPositionTextureMap.SetPixels(newCols);
            targetPositionTextureMap.Apply(false);
            //vfx.SetTexture("TargetPosMapTex", targetPositionTextureMap);
        }


        public void Morph(float animationTime = 1f)
        {
            morphed = !morphed;
            float endVal = morphed ? 1f : -0.11f;
            float blendVal = vfx.GetFloat("morphStep");
            DOTween.To(() => blendVal, x => blendVal = x, endVal, animationTime).OnUpdate(
                () => vfx.SetFloat("morphStep", blendVal))
                .SetEase(Ease.OutBack);

            GraphName = morphed ? otherName : originalName;
        }

        private void UpdateTargetTexture()
        {
            Texture2D targetTex = new Texture2D(positionTextureMap.width, positionTextureMap.height);
            Color[] currentCoords = positionTextureMap.GetPixels();
            Color[] newCoords = currentCoords;
            Color[] targetCoords = targetPositionTextureMap.GetPixels();
            Color[] alphas = TextureHandler.instance.alphaTextureMaps[0].GetPixels();

            for (int i = 0; i < alphas.Length; i++)
            {
                if (alphas[i].r > 0.5f)
                {
                    newCoords[i] = targetCoords[i];
                }
                //else
                //{
                //    newCoords[i] = currentCoords[i];
                //}
            }

            targetTex.SetPixels(newCoords);
            targetTex.Apply(false);
            vfx.SetTexture("TargetPosMapTex", targetTex);
        }

        public void SetAlphaClipThreshold(float val)
        {
            int ind = (int)val;
            vfx.SetFloat("AlphaClipThresh", (0.05f + (float)ind / (CellexalConfig.Config.GraphNumberOfExpressionColors)));
        }

        public void SetAlphaClipThreshold(bool toggle)
        {
            float val = toggle ? 0.5f : 0f;
            vfx.SetFloat("AlphaClipThresh", val);
        }


        public IEnumerator CreatePositionTextureMap(List<Point> points, PointCloud parentPC)
        {
            pointCount = points.Count;
            if (pointCount > 500000)
            {
                vfx.visualEffectAsset = pointCloudHighCap;
                vfx.SetFloat("Size", 0.003f);
            }
            vfx.SetInt("SpawnRate", pointCount);
            int width = PointCloudGenerator.textureWidth;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(pointCount / (float)PointCloudGenerator.textureWidth);//width;
            positionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            targetPositionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            Texture2D parentTargetTextureMap = parentPC.targetPositionTextureMap;
            if (parentTargetTextureMap == null)
            {
                print("parent target is null");
                parentTargetTextureMap = parentPC.morphTexture;
            }
            Color[] positions = new Color[width * height];
            Color[] targetPositions = new Color[positions.Length];
            //for (int i = 0; i < points.Count; i++)
            //{
            //    Point p = points[i];
            //    Color c = parentTextureMap.GetPixel(p.xindex, p.yindex);
            //    targetPositions[i] = c;
            //}

            Color c;
            float3 pos;
            float3 wPos;
            Point p;
            Point newP;
            //Entity e;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (width * y);
                    if (ind >= pointCount) continue;
                    if (ind % 2000 == 0) yield return null;
                    p = points[ind];
                    c = parentTargetTextureMap.GetPixel(p.xindex, p.yindex);
                    targetPositions[ind] = c;

                    pos = p.offset;
                    //Entity e = entityManager.Instantiate(PrefabEntities.prefabEntity);
                    wPos = math.transform(transform.localToWorldMatrix, pos);
                    entityManager.SetComponentData(p.entity, new Translation { Value = wPos });
                    newP = new Point
                    {
                        selected = false,
                        orgXIndex = p.orgXIndex,
                        orgYIndex = p.orgYIndex,
                        xindex = x,
                        yindex = y,
                        label = p.label,
                        offset = pos,
                        parentID = pcID,
                        orgParentID = p.orgParentID,
                        entity = p.entity
                    };
                    entityManager.SetComponentData(p.entity, newP);
                    points[ind] = newP;
                    positions[ind] = new Color(pos.x, pos.y, pos.z, 1);

                }
                //if (y % 10 == 0) yield return null;
            }
            targetPositionTextureMap.SetPixels(targetPositions);
            positionTextureMap.SetPixels(positions);
            positionTextureMap.Apply(false);
            targetPositionTextureMap.Apply(false);
            vfx.enabled = true;
            vfx.SetTexture("PositionMapTex", positionTextureMap);
            vfx.SetTexture("TargetPosMapTex", targetPositionTextureMap);
            vfx.pause = false;
            //PointCloudGenerator.instance.creatingGraph = false;
        }


        public void CreatePositionTextureMap(List<float3> pointPositions, List<string> names)
        {
            pointCount = pointPositions.Count;
            if (vfx == null) vfx = GetComponent<VisualEffect>();
            //vfx.visualEffectAsset = pointCount < 500000 ? pointCloudQuad : pointCloudHighCap;  //
            if (pointCount > 500000)
            {
                vfx.visualEffectAsset = pointCloudHighCap;
                vfx.SetFloat("Size", 0.003f);
            }
            vfx.SetInt("SpawnRate", pointCount);
            int width = PointCloudGenerator.textureWidth;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(pointCount / (float)PointCloudGenerator.textureWidth);//width;
            positionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            orgPositionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            targetPositionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            Color[] positions = new Color[width * height];
            Color col;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (width * y);
                    if (ind >= pointCount) continue;
                    float3 pos = pointPositions[ind];
                    col = new Color(pos.x, pos.y, pos.z, 1);

                    Vector2Int textureCoord = TextureHandler.instance.textureCoordDict[names[ind]];

                    Entity e = entityManager.Instantiate(PrefabEntities.prefabEntity);
                    float3 wPos = math.transform(transform.localToWorldMatrix, pos);
                    entityManager.SetComponentData(e, new Translation { Value = wPos });
                    entityManager.AddComponent(e, typeof(Point));
                    Point p = new Point
                    {
                        selected = false,
                        orgXIndex = textureCoord.x,
                        orgYIndex = textureCoord.y,
                        xindex = x,
                        yindex = y,
                        label = ind,
                        offset = pos,
                        parentID = pcID,
                        orgParentID = pcID,
                        entity = e
                    };
                    entityManager.SetComponentData(e, p);
                    positions[ind] = col;
                }
            }

            positionTextureMap.SetPixels(positions);
            orgPositionTextureMap.SetPixels(positions);
            targetPositionTextureMap.SetPixels(positions);
            positionTextureMap.Apply(false);
            orgPositionTextureMap.Apply(false);
            targetPositionTextureMap.Apply(false);
            vfx.enabled = true;
            vfx.SetTexture("PositionMapTex", positionTextureMap);
            vfx.SetTexture("TargetPosMapTex", targetPositionTextureMap);
            vfx.pause = false;
            PointCloudGenerator.instance.creatingGraph = false;
            //SpreadOutPoints();
            //StartCoroutine(SpawnAnimation());
        }
        
        public void CreatePositionTextureMap(List<float3> pointPositions, List<string> names, ref List<Point> slicePoints)
        {
            pointCount = pointPositions.Count;
            if (vfx == null) vfx = GetComponent<VisualEffect>();
            vfx.visualEffectAsset = pointCount < 500000 ? pointCloudQuad : pointCloudHighCap;  //
            vfx.SetInt("SpawnRate", pointCount);
            int width = PointCloudGenerator.textureWidth;//(int)math.ceil(math.sqrt(pointCount));
            int height = (int)math.ceil(pointCount / (float)PointCloudGenerator.textureWidth);//width;
            positionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            orgPositionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            targetPositionTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            Color[] positions = new Color[width * height];
            Color col;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ind = x + (width * y);
                    if (ind >= pointCount) continue;
                    float3 pos = pointPositions[ind];
                    col = new Color(pos.x, pos.y, pos.z, 1);

                    Vector2Int textureCoord = TextureHandler.instance.textureCoordDict[names[ind]];

                    Entity e = entityManager.Instantiate(PrefabEntities.prefabEntity);
                    float3 wPos = math.transform(transform.localToWorldMatrix, pos);
                    entityManager.SetComponentData(e, new Translation { Value = wPos });
                    entityManager.AddComponent(e, typeof(Point));
                    Point p = new Point
                    {
                        selected = false,
                        orgXIndex = textureCoord.x,
                        orgYIndex = textureCoord.y,
                        xindex = x,
                        yindex = y,
                        label = ind,
                        offset = pos,
                        parentID = pcID,
                        orgParentID = pcID,
                        entity = e
                    };
                    slicePoints.Add(p);
                    entityManager.SetComponentData(e, p);
                    positions[ind] = col;
                }
            }

            positionTextureMap.SetPixels(positions);
            orgPositionTextureMap.SetPixels(positions);
            targetPositionTextureMap.SetPixels(positions);
            positionTextureMap.Apply(false);
            orgPositionTextureMap.Apply(false);
            targetPositionTextureMap.Apply(false);
            vfx.enabled = true;
            vfx.SetTexture("PositionMapTex", positionTextureMap);
            vfx.SetTexture("TargetPosMapTex", targetPositionTextureMap);
            vfx.pause = false;
            PointCloudGenerator.instance.creatingGraph = false;
            GetComponent<GraphSlice>().points = slicePoints;
            //SpreadOutPoints();
            //StartCoroutine(SpawnAnimation());
        }


        private IEnumerator SpawnAnimation()
        {
            float blend = 0f;
            while (blend < 3f)
            {
                while (blend < 2f)
                {
                    blend += Time.deltaTime;
                    yield return null;
                }
                blend += Mathf.Min(0.05f, Time.deltaTime);
                vfx.SetFloat("SpawnAnimation", blend);
                yield return null;
            }
            vfx.SetFloat("SpawnAnimation", 1f);
        }


        public void BlendToGlassOrgan()
        {
            pointSpreadTexture = new Texture2D(positionTextureMap.width, positionTextureMap.height, TextureFormat.RGBAFloat, true, true);
            Color[] colors = alphaTextureMap.GetPixels();
            Color[] positions = positionTextureMap.GetPixels();
            Color[] newPositions = new Color[positions.Length];
            Vector3 pos;
            Vector3 dir;
            Vector3 newPos;
            for (int i = 0; i < positions.Length; i++)
            {
                if (colors[i].r < 0.5f)
                {
                    newPositions[i] = positions[i];
                }
                else
                {
                    pos = new Vector3(positions[i].r, positions[i].g, positions[i].b);
                    dir = pos - new Vector3(0f, 0, 0);

                    //Vector3 dir = pos - new Vector3(1f, 0, 0); // for half sphere
                    newPos = (pos + dir).normalized;
                    newPositions[i] = new Color(newPos.x, newPos.y, newPos.z);
                }
            }

            pointSpreadTexture.SetPixels(newPositions);
            pointSpreadTexture.Apply(false);
            vfx.SetTexture("TargetPosMapTex", pointSpreadTexture);

        }

        public void SpreadOutPoints(bool doSpread = true)
        {
            //if (pointSpreadTexture == null && doSpread) // skip redo texture if it hasnt changed (?)
            if (doSpread)
            {
                pointSpreadTexture = new Texture2D(positionTextureMap.width, positionTextureMap.height, TextureFormat.RGBAFloat, true, true);
                Color[] colors = alphaTextureMap.GetPixels();
                Color[] positions = positionTextureMap.GetPixels();
                Color[] newPositions = new Color[positions.Length];
                Vector3 pos;
                Vector3 dir;
                Vector3 newPos;
                for (int i = 0; i < positions.Length; i++)
                {
                    //if (i % 10000 == 0)
                    //    yield return null;
                    if (colors[i].r < 0.5f)
                    {
                        newPositions[i] = positions[i];
                    }
                    else
                    {
                        pos = new Vector3(positions[i].r, positions[i].g, positions[i].b);
                        dir = pos - new Vector3(0f, 0, 0);

                        //Vector3 dir = pos - new Vector3(1f, 0, 0); // for half sphere
                        newPos = (pos + dir).normalized;
                        newPositions[i] = new Color(newPos.x, newPos.y, newPos.z);
                    }
                }

                pointSpreadTexture.SetPixels(newPositions);
                pointSpreadTexture.Apply(false);
                vfx.SetTexture("TargetPosMapTex", pointSpreadTexture);
            }
            else if (doSpread)
            {
                vfx.SetTexture("TargetPosMapTex", pointSpreadTexture);
            }

            Morph();
        }

        public void SliceSpread(NativeArray<float3> dirs)
        {
            pointSpreadTexture = new Texture2D(positionTextureMap.width, positionTextureMap.height, TextureFormat.RGBAFloat, true, true);
            Color[] positions = positionTextureMap.GetPixels();
            Color[] newPositions = new Color[positions.Length];
            float3 pos;
            float3 newPos;
            for (int i = 0; i < positions.Length; i++)
            {
                pos = new float3(positions[i].r, positions[i].g, positions[i].b);
                newPos = (pos + dirs[i] * 0.2f);
                newPositions[i] = new Color(newPos.x, newPos.y, newPos.z);
            }

            pointSpreadTexture.SetPixels(newPositions);
            pointSpreadTexture.Apply(false);

            vfx.SetTexture("TargetPosMapTex", pointSpreadTexture);
            Morph(0.4f);
        }

        public void SpreadOutClusters()
        {
            if (!morphed)
            {
                clusterSpreadTexture = new Texture2D(positionTextureMap.width, positionTextureMap.height, TextureFormat.RGBAFloat, true, true);
                if (clusterCentroids.Count == 0)
                    CalculateClusterCentroids();
                Color[] colors = alphaTextureMap.GetPixels();
                Color[] positions = positionTextureMap.GetPixels();
                Color[] newPositions = new Color[positionTextureMap.width * positionTextureMap.height];
                for (int i = 0; i < colors.Length; i++)
                {
                    if (i >= pointCount) break;
                    Color pos = positions[i];
                    if (colors[i].r < 0.7f) // if only for highlighted points
                    {
                        newPositions[i] = pos;
                    }
                    else
                    {
                        string cluster = PointCloudGenerator.instance.clusterDict[i];
                        Color centroid = clusterCentroids[cluster];
                        Vector3 cP = (new Vector3(centroid.r, centroid.g, centroid.b) - Vector3.zero);
                        Vector3 newPos = (new Vector3(pos.r, pos.g, pos.b) + cP) * 2f;//.normalized;
                        Color newPosCol = new Color(newPos.x, newPos.y, newPos.z);
                        newPositions[i] = newPosCol;
                    }
                    //}

                    //else
                    //{
                    //    newPositions[i] = pos;
                    //}
                }

                clusterSpreadTexture.SetPixels(newPositions);
                clusterSpreadTexture.Apply(false);
                targetPositionTextureMap = clusterSpreadTexture;
            }
            vfx.SetTexture("TargetPosMapTex", clusterSpreadTexture);
            Morph();
        }

        private void CalculateClusterCentroids()
        {
            foreach (KeyValuePair<string, List<Vector2Int>> kvp in PointCloudGenerator.instance.clusters)
            {
                Color sum = Color.black;
                foreach (Vector2 ind in kvp.Value)
                {
                    Color pos = positionTextureMap.GetPixel((int)ind.x, (int)ind.y);
                    sum += pos;
                }

                Color centroid = sum / kvp.Value.Count;
                clusterCentroids[kvp.Key] = centroid;
            }

        }

        //
        // public void UpdateColorTexture(int x, int y)
        // {
        //     colorMap.SetPixel(x, y, Color.red);
        //     colorMap.Apply();
        //     vfx.SetTexture("ColorMapTex", colorMap);
        // }







    }


}