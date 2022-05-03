using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Spatial;
using SQLiter;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    public class TextureHandler : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;
        private EntityManager entityManager;
        private int frameCount;
        private EntityQuery query;

        public Dictionary<int, int> sps;
        public List<Texture2D> colorTextureMaps = new List<Texture2D>();
        public List<Texture2D> mainColorTextureMaps = new List<Texture2D>();
        public List<Texture2D> alphaTextureMaps = new List<Texture2D>();
        public List<Texture2D> clusterTextureMaps = new List<Texture2D>();
        public Dictionary<string, Vector2Int> textureCoordDict = new Dictionary<string, Vector2Int>();

        public static TextureHandler instance;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityManager = World.EntityManager;
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            sps = new Dictionary<int, int>();
            query = GetEntityQuery(typeof(RaycastCheckComponent));
            instance = this;
        }


        protected override void OnUpdate()
        {
            if (colorTextureMaps.Count == 0) return;
            //if (frameCount >= 10)
            //{
            //frameCount = 0;
            List<Tuple<int, int>> orgPixels = new List<Tuple<int, int>>();
            List<int> indices = new List<int>();
            List<int> groups = new List<int>();
            Entities.WithAll<SelectedPointComponent>().WithoutBurst().ForEach((Entity e, int entityInQueryIndex, ref SelectedPointComponent sp) =>
            {
                orgPixels.Add(new Tuple<int, int>(sp.orgXIndex, sp.orgYIndex));
                indices.Add(sp.label);
                groups.Add(sp.group);
                sps[sp.label] = sp.group;
            }).Run();
            ReferenceManager.instance.multiuserMessageSender.SendMessageSelectedAddPointCloud(indices.ToArray(), groups.ToArray());
            Color col = SelectionToolCollider.instance.GetCurrentColor();
            col.a = 1f;
            Color a = Color.white;
            for (int i = 0; i < mainColorTextureMaps.Count; i++)
            {
                Texture2D map = mainColorTextureMaps[i];
                Texture2D aMap = alphaTextureMaps[i];
                foreach (Tuple<int, int> tuple in orgPixels)
                {
                    map.SetPixel(tuple.Item1, tuple.Item2, col);
                    aMap.SetPixel(tuple.Item1, tuple.Item2, a);
                }
                map.Apply(false);
                aMap.Apply(false);
            }
            EntityManager.DestroyEntity(GetEntityQuery(typeof(SelectedPointComponent)));
            CellexalEvents.SelectionStarted.Invoke();
            CellexalEvents.ColorTextureUpdated.Invoke();
            //frameCount++;
        }

        public void AddPointsToSelection(List<Vector2Int> indGroupTuple, bool select = false)
        {
            int i = 0;
            indGroupTuple.OrderBy(x => x.x);
            Color col; // SelectionToolCollider.instance.GetCurrentColor();
            col.a = 1f;
            Color a = Color.white;
            for (int j = 0; j < mainColorTextureMaps.Count; j++)
            {
                Texture2D map = mainColorTextureMaps[j];
                Texture2D amap = alphaTextureMaps[j];
                foreach (Vector2Int tup in indGroupTuple)
                {
                    if (select)
                        sps[tup.x] = tup.y;
                    col = SelectionToolCollider.instance.Colors[tup.y];
                    Vector2Int xy = textureCoordDict[PointCloudGenerator.instance.indToLabelDict[tup.x]];
                    map.SetPixel(xy.x, xy.y, col);
                    amap.SetPixel(xy.x, xy.y, a);
                }
                map.Apply();
                amap.Apply();
            }
            CellexalEvents.ColorTextureUpdated.Invoke();
        }

        public void ColorCluster(string cluster, bool toggle)
        {
            List<Vector2Int> indices = PointCloudGenerator.instance.clusters[cluster];
            Color col = toggle ? PointCloudGenerator.instance.colorDict[cluster] : new Color(0.32f, 0.32f, 0.32f);
            Color a = toggle ? Color.white : Color.white * 0.4f;
            Texture2D atex = alphaTextureMaps[0];
            Color[] aaray = atex.GetPixels();
            for (int i = 0; i < aaray.Length; i++)
            {
                Color val = aaray[i];
                if (val.r > 0.9f) continue;
                aaray[i] = Color.white * 0.05f;
            }
            atex.SetPixels(aaray);
            foreach (Vector2 ind in indices)
            {
                mainColorTextureMaps[0].SetPixel((int)ind.x, (int)ind.y, col);
                alphaTextureMaps[0].SetPixel((int)ind.x, (int)ind.y, a);
            }

            if (toggle)
            {
                ReferenceManager.instance.legendManager.attributeLegend.AddEntry(cluster, indices.Count, col);
                ReferenceManager.instance.attributeSubMenu.attributes.Add(cluster);
            }
            else
            {
                ReferenceManager.instance.attributeSubMenu.attributes.Remove(cluster);
                ReferenceManager.instance.legendManager.attributeLegend.RemoveEntry(cluster);
            }
            colorTextureMaps[0].Apply();
            alphaTextureMaps[0].Apply();

            if (MeshGenerator.instance.generateMeshes)
            {
                if (toggle)
                {
                    MeshGenerator.instance.GenerateMeshes();
                }
                else
                {
                    MeshGenerator.instance.RemoveMesh(SelectionToolCollider.instance.GetColorIndex(PointCloudGenerator.instance.colorDict[cluster]));
                }

            }
            CellexalEvents.ColorTextureUpdated.Invoke();
        }

        public void MakeAllPointsTransparent(bool toggle)
        {
            Color a = toggle ? Color.white * 0.4f : Color.white * 0.8f;
            Texture2D atex = alphaTextureMaps[0];
            Color[] aarray = atex.GetPixels();
            for (int i = 0; i < aarray.Length; i++)
            {
                if (aarray[i].maxColorComponent > 0.9f) continue;
                aarray[i] = a;
            }
            atex.SetPixels(aarray);
            atex.Apply();
            CellexalEvents.ColorTextureUpdated.Invoke();
        }

        public void ColorAllClusters(bool toggle)
        {
            if (toggle)
            {
                mainColorTextureMaps[0].SetPixels(clusterTextureMaps[0].GetPixels());
                mainColorTextureMaps[0].Apply();
                MakeAllPointsTransparent(false);
            }
            else
            {
                ResetTexture();
            }
            CellexalEvents.ColorTextureUpdated.Invoke();
        }

        public void ColorByExpression(ArrayList expressions)
        {
            Texture2D tex = mainColorTextureMaps[0];
            Texture2D atex = alphaTextureMaps[0];
            MakeAllPointsTransparent(true);
            Color a = new Color(1f, 1f, 1f) / 30f;
            foreach (CellExpressionPair pair in expressions)
            {
                Vector2Int coords = textureCoordDict[pair.Cell];
                tex.SetPixel(coords.x, coords.y, ReferenceManager.instance.graphGenerator.geneExpressionColors[pair.Color + 1]);
                atex.SetPixel(coords.x, coords.y, Color.white * 0.2f + (a * (pair.Color + 1)));
            }
            atex.Apply();
            tex.Apply();
            CellexalEvents.ColorTextureUpdated.Invoke();
        }

        public void ResetTexture()
        {
            Color[] colors = new Color[mainColorTextureMaps[0].width * mainColorTextureMaps[0].height];
            Color[] alphas = new Color[mainColorTextureMaps[0].width * mainColorTextureMaps[0].height];
            Color a = new Color(0.55f, 0f, 0);
            Color c = Color.white * 0.4f;
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = c;
                alphas[i] = a;
            }
            mainColorTextureMaps[0].SetPixels(colors);
            alphaTextureMaps[0].SetPixels(colors);
            mainColorTextureMaps[0].Apply();
            alphaTextureMaps[0].Apply();
            CellexalEvents.ColorTextureUpdated.Invoke();
        }


    }
}