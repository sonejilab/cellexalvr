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

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            int entityCount = query.CalculateEntityCount();

            float3 origin = SelectionToolCollider.instance.transform.position;
            EntityCommandBuffer.ParallelWriter commandBuffer = ecbSystem.CreateCommandBuffer().AsParallelWriter();

            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(entityCount, Allocator.TempJob);
            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(entityCount, Allocator.TempJob);
            NativeArray<bool> hits = new NativeArray<bool>(entityCount, Allocator.TempJob);

            JobHandle jobHandle = Entities.WithAll<RaycastCheckComponent>().WithStoreEntityQueryInField(ref query)
                .ForEach((Entity entity, int entityInQueryIndex, ref LocalToWorld localToWorld) =>
                {
                    Vector3 dir = origin - localToWorld.Position;
                    commands[entityInQueryIndex] = new RaycastCommand(localToWorld.Position, dir, dir.magnitude, 1 << 14);
                }).ScheduleParallel(Dependency);
            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, results.Length, jobHandle);
            handle.Complete();

            for (int i = 0; i < results.Length; i++)
            {
                hits[i] = results[i].collider != null;
            }


            JobHandle selectJob = Entities.WithAll<RaycastCheckComponent>().ForEach((Entity entity, int entityInQueryIndex,
                ref RaycastCheckComponent raycastCheckComponent) =>
            {
                if (!hits[entityInQueryIndex])
                {
                    commandBuffer.AddComponent<SelectedPointComponent>(entityInQueryIndex, entity);
                    Point p = GetComponent<Point>(entity);
                    p.selected = true;
                    commandBuffer.SetComponent(entityInQueryIndex, entity, p);
                    // AddGraphPointToSelection(entity);
                }

                commandBuffer.RemoveComponent<RaycastCheckComponent>(entityInQueryIndex, entity);
            }).ScheduleParallel(handle);
            selectJob.Complete();

            results.Dispose();
            commands.Dispose();
            hits.Dispose();
        }
    }
}