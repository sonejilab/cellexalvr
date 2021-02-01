using CellexalVR.Interaction;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{

    public class TextureHandler : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;
        private EntityManager entityManager;
        private int frameCount;
        
        public Texture2D colorTextureMap;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityManager = World.EntityManager;
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {
            if (colorTextureMap == null) return;
            if (frameCount >= 10)
            {
                frameCount = 0;
                EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer();
                Color col = SelectionToolCollider.instance.GetCurrentColor();
                Entities.WithAll<SelectedPointComponent>().WithoutBurst().ForEach((Entity e, int entityInQueryIndex, ref SelectedPointComponent sp) =>
                {
                    colorTextureMap.SetPixel(sp.point.xindex, sp.point.yindex, col);
                }).Run();
                ecbSystem.AddJobHandleForProducer(Dependency);
                EntityManager.DestroyEntity(GetEntityQuery(typeof(SelectedPointComponent)));
                
                colorTextureMap.Apply();
            }

            frameCount++;
        }
    }
}