using CellexalVR.Interaction;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DefaultNamespace
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class RaycastSystem : SystemBase
    {
        private EntityQuery query;
        private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EntityArchetype entityArchetype;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
            query = GetEntityQuery(typeof(RaycastCheckComponent));
            entityArchetype = EntityManager.CreateArchetype(typeof(SelectedPointComponent));
        }

        protected override void OnDestroy()
        {
            query.Dispose();
        }

        protected override void OnUpdate()
        {
            if (!SelectionToolCollider.instance.selActive) return;
            int entityCount = query.CalculateEntityCount(); //GetEntityQuery(typeof(Point)).CalculateEntityCount();

            float3 origin = SelectionToolCollider.instance.GetCurrentCollider().transform.position;
            EntityCommandBuffer commandBuffer = ecbSystem.CreateCommandBuffer();

            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(entityCount, Allocator.TempJob);
            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(entityCount, Allocator.TempJob);
            NativeArray<bool> hits = new NativeArray<bool>(entityCount, Allocator.TempJob);
            // Try looping every entity and check field raycast 
            JobHandle jobHandle = Entities.WithAll<RaycastCheckComponent>().WithStoreEntityQueryInField(ref query)
                .ForEach((Entity entity, int entityInQueryIndex, ref RaycastCheckComponent rc) =>
                {
                    Vector3 dir = origin - rc.position;
                    commands[entityInQueryIndex] = new RaycastCommand(rc.position, dir, 10, 1 << 14);
                    // Debug.DrawRay(rc.position, dir);
                }).ScheduleParallel(Dependency);
            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, results.Length, jobHandle);
            handle.Complete();

            for (int i = 0; i < results.Length; i++)
            {
                hits[i] = results[i].collider != null;
            }

            
            Entities.WithoutBurst().WithAll<RaycastCheckComponent>().ForEach((Entity entity, int entityInQueryIndex, ref RaycastCheckComponent rc) =>
            {
                if (!hits[entityInQueryIndex])
                {
                    // commandBuffer.AddComponent<SelectedPointComponent>(entityInQueryIndex, entity);
                    Point p = GetComponent<Point>(rc.entity);
                    p.selected = true;
                    commandBuffer.SetComponent(rc.entity, p);
                    Entity e = commandBuffer.CreateEntity(entityArchetype);
                    commandBuffer.SetComponent(e, new SelectedPointComponent { point = p });
                }

            }).Run();

            EntityManager.DestroyEntity(GetEntityQuery(typeof(RaycastCheckComponent)));
            
            results.Dispose();
            commands.Dispose();
            hits.Dispose();
            
        }
    }
}