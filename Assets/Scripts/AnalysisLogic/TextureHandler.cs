using System;
using System.Collections.Generic;
using System.Linq;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DefaultNamespace
{
    public class TextureHandler : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;
        private EntityManager entityManager;
        private int frameCount;
        private EntityQuery query;

        public List<Tuple<int, int>> sps;
        public List<Texture2D> colorTextureMaps = new List<Texture2D>();
        public List<Texture2D> mainColorTextureMaps = new List<Texture2D>();
        public List<Texture2D> alphaTextureMaps = new List<Texture2D>();
        public List<Texture2D> clusterTextureMaps = new List<Texture2D>();

        protected override void OnCreate()
        {
            base.OnCreate();
            entityManager = World.EntityManager;
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            sps = new List<Tuple<int, int>>();
            query = GetEntityQuery(typeof(RaycastCheckComponent));
        }


        protected override void OnUpdate()
        {
            if (colorTextureMaps.Count == 0) return;
            //if (frameCount >= 10)
            //{
            frameCount = 0;
            Color col = SelectionToolCollider.instance.GetCurrentColor();
            List<Tuple<int, int>> orgPixels = new List<Tuple<int, int>>();
            Entities.WithAll<SelectedPointComponent>().WithoutBurst().ForEach((Entity e, int entityInQueryIndex, ref SelectedPointComponent sp) =>
            {
                //int index = sp.parentID;
                //if (index > mainColorTextureMaps.Count)
                //{
                //    colorTextureMaps[index].SetPixel(sp.xindex, sp.yindex, col);
                //    alphaTextureMaps[index].SetPixel(sp.xindex, sp.yindex, Color.white);
                //}
                orgPixels.Add(new Tuple<int, int>(sp.orgXIndex, sp.orgYIndex));
                sps.Add(new Tuple<int, int>(sp.label, sp.group));
            }).Run();
            for (int i = 0; i < mainColorTextureMaps.Count; i++)
            {
                Texture2D map = mainColorTextureMaps[i];
                foreach (Tuple<int, int> tuple in orgPixels)
                {
                    map.SetPixel(tuple.Item1, tuple.Item2, col);
                }
                //map.SetPixels(mainTexColors);
                map.Apply();
            }
            //for (int i = 0; i < colorTextureMaps.Count; i++)
            //{
            //    Texture2D map = colorTextureMaps[i];
            //    foreach (Tuple<int, int> tuple in orgPixels)
            //    {
            //        map.SetPixel(tuple.Item1, tuple.Item2, col);
            //    }
            //    //map.SetPixels(mainTexColors);
            //    map.Apply();
            //}
            //ecbSystem.AddJobHandleForProducer(Dependency);
            EntityManager.DestroyEntity(GetEntityQuery(typeof(SelectedPointComponent)));
            CellexalEvents.SelectionStarted.Invoke();
            //colorTextureMaps.ForEach(tex => tex.Apply());
            //alphaTextureMaps.ForEach(tex => tex.Apply());
            //}


            frameCount++;
        }

        public void MainTexToSliceTex(int x, int y)
        {
            //Find correct texture pixel in slice map from main map.
        }

        public void ColorCluster(string cluster, bool toggle)
        {
            List<Vector2> indices = ReferenceManager.instance.pointCloudGenerator.clusters[cluster];
            Color col = toggle ? ReferenceManager.instance.pointCloudGenerator.colorDict[cluster] : new Color(0.32f, 0.32f, 0.32f, 0.7f);
            // Debug.Log($"{cluster}, ps : {indices.Count}, col :{col}");
            foreach (Vector2 ind in indices)
            {
                colorTextureMaps[0].SetPixel((int)ind.x, (int)ind.y, col);
            }

            if (toggle)
            {
                ReferenceManager.instance.legendManager.attributeLegend.AddEntry(cluster, indices.Count, col);
            }
            else
            {
                ReferenceManager.instance.legendManager.attributeLegend.RemoveEntry(cluster);
            }
            colorTextureMaps[0].Apply();
        }
    }
}