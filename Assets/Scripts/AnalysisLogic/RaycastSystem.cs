using CellexalVR.Interaction;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// This class is also part of the selection of points in a point cloud logic see also <see cref="PointSelectionSystem"/> and <see cref="OctantSystem"/>.
    /// Relevant points, meaning points that are in the same or an octant close to the users selection tool are raycasted and checked if they are outside or inside of the selection tool mesh.
    /// If they are inside they are selected, i.e. a <see cref="SelectedPointComponent"/> is added.
    /// All the logic happends inside the <see cref="OnUpdate"/> function.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class RaycastSystem : SystemBase
    {
        public bool move;

        private EntityQuery query;
        private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EntityArchetype selectEntityArchetype;
        private EntityArchetype moveEntityArchetype;

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
            query = GetEntityQuery(typeof(RaycastCheckComponent));
            selectEntityArchetype = EntityManager.CreateArchetype(typeof(SelectedPointComponent));
            moveEntityArchetype = EntityManager.CreateArchetype(typeof(MovePointComponent));
        }

        protected override void OnDestroy() {}

        protected override void OnUpdate()
        {
            if (!SelectionToolCollider.instance.selActive) return;
            int entityCount = query.CalculateEntityCount();

            float3 origin = SelectionToolCollider.instance.GetCurrentCollider().transform.position;
            EntityCommandBuffer commandBuffer = ecbSystem.CreateCommandBuffer();

            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(entityCount, Allocator.TempJob);
            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(entityCount, Allocator.TempJob);
            NativeArray<bool> hits = new NativeArray<bool>(entityCount, Allocator.TempJob);
            JobHandle jobHandle = Entities.WithAll<RaycastCheckComponent>().WithStoreEntityQueryInField(ref query)
                .ForEach((Entity entity, int entityInQueryIndex, ref RaycastCheckComponent rc) =>
                {
                    Vector3 dir = origin - rc.position;
                    commands[entityInQueryIndex] = new RaycastCommand(rc.position, dir, 10, 1 << 14);
                }).ScheduleParallel(Dependency);
            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, results.Length, jobHandle);
            handle.Complete();

            for (int i = 0; i < results.Length; i++)
            {
                hits[i] = results[i].collider != null;
            }

            int group = SelectionToolCollider.instance.CurrentColorIndex;
            PointCloud pc1 = PointCloudGenerator.instance.pointClouds[0];
            PointCloud pc2 = PointCloudGenerator.instance.pointClouds[1];
            Entities.WithoutBurst().WithAll<RaycastCheckComponent>().ForEach((Entity entity, int entityInQueryIndex, ref RaycastCheckComponent rc) =>
            {
                if (!hits[entityInQueryIndex])
                {
                    Entity selectEntity = commandBuffer.CreateEntity(selectEntityArchetype);
                    Entity moveEntity = commandBuffer.CreateEntity(moveEntityArchetype);
                    commandBuffer.SetComponent(selectEntity, new SelectedPointComponent
                    {
                        orgXIndex = rc.orgXIndex,
                        orgYIndex = rc.orgYIndex,
                        xindex = rc.xindex,
                        yindex = rc.yindex,
                        label = rc.label,
                        group = group,
                        parentID = rc.parentID
                    });
                    if (move)
                    {
                        var targetPositionC = pc1.orgPositionTextureMap.GetPixel(rc.xindex, rc.yindex);
                        var currentPositionC = pc2.positionTextureMap.GetPixel(rc.xindex, rc.yindex);
                        var posInWSpace = pc2.transform.TransformPoint(new Vector3(currentPositionC.r, currentPositionC.g, currentPositionC.b));
                        var posInPc1Space = pc1.transform.InverseTransformPoint(posInWSpace);
                        commandBuffer.SetComponent(moveEntity, new MovePointComponent
                        {
                            xindex = rc.xindex,
                            yindex = rc.yindex,
                            targetPosition = new Vector3(targetPositionC.r, targetPositionC.g, targetPositionC.b),
                            currentPosition = posInPc1Space
                        });
                    }

                }
            }).Run();

            EntityManager.DestroyEntity(GetEntityQuery(typeof(RaycastCheckComponent)));
            results.Dispose();
            commands.Dispose();
            hits.Dispose();

        }
    }
}