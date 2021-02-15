using System;
using System.Collections.Generic;
using System.Linq;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class TextureHandler : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;
        private EntityManager entityManager;
        private int frameCount;

        public List<Tuple<int, int>> sps;
        public Texture2D colorTextureMap;
        public Texture2D clusterTextureMap;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityManager = World.EntityManager;
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            sps = new List<Tuple<int, int>>();
        }


        protected override void OnUpdate()
        {
            if (colorTextureMap == null) return;
            if (frameCount >= 10)
            {
                frameCount = 0;
                Color col = SelectionToolCollider.instance.GetCurrentColor();
                Entities.WithAll<SelectedPointComponent>().WithoutBurst().ForEach((Entity e, int entityInQueryIndex, ref SelectedPointComponent sp) =>
                {
                    colorTextureMap.SetPixel(sp.xindex, sp.yindex, col);
                    sps.Add(new Tuple<int, int> (sp.label, sp.group) );
                }).Run();
                ecbSystem.AddJobHandleForProducer(Dependency);
                EntityManager.DestroyEntity(GetEntityQuery(typeof(SelectedPointComponent)));
                CellexalEvents.SelectionStarted.Invoke();
                colorTextureMap.Apply();
            }


            frameCount++;
        }

        public void ColorCluster(string cluster, bool toggle)
        {
            List<Vector2> indices = ReferenceManager.instance.pointCloudGenerator.clusters[cluster];
            Color col = toggle ? ReferenceManager.instance.pointCloudGenerator.colorDict[cluster] : new Color(0.32f, 0.32f, 0.32f);
            // Debug.Log($"{cluster}, ps : {indices.Count}, col :{col}");
            foreach (Vector2 ind in indices)
            {
                colorTextureMap.SetPixel((int)ind.x, (int)ind.y, col);
            }
            
            if (toggle)
            {
                ReferenceManager.instance.legendManager.attributeLegend.AddEntry(cluster, indices.Count, col);
            }
            else
            {
                ReferenceManager.instance.legendManager.attributeLegend.RemoveEntry(cluster);
            }
            colorTextureMap.Apply();
        }
    }
}