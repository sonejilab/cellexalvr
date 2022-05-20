using AnalysisLogic;
using CellexalVR.Interaction;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// Component struct to add to each selected point so that it can then be found and colored.
    /// </summary>
    public struct SelectedPointComponent : IComponentData
    {
        public int orgXIndex;
        public int orgYIndex;
        public int xindex;
        public int yindex;
        public int label;
        public int group;
        public int parentID;
    }

    /// <summary>
    /// Component to add to the points that are to be raycasted and checked if they are inside the selection tool.
    /// If they are a <see cref="SelectedPointComponent"/> is added.
    /// </summary>
    public struct RaycastCheckComponent : IComponentData
    {
        public int orgXIndex;
        public int orgYIndex;
        public float3 position;
        public float3 origin;
        public int xindex;
        public int yindex;
        public int label;
        public int parentID;
    }

    /// <summary>
    /// This class is a component system to handle the point selection in point clouds.
    /// Since the point clouds are not built using octree as the normal graphs we can not use the same selection system.
    /// But this system also in the end uses ray casts to decide if the points are inside of the selection tool.
    /// Which points are to be checked <see cref="OctantSystem"/>
    /// </summary>
    public class PointSelectionSystem : SystemBase
    {
        private int frameCount;
        private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EntityArchetype entityArchetype;

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
            entityArchetype = EntityManager.CreateArchetype(typeof(RaycastCheckComponent));
        }

        protected override void OnUpdate()
        {
            if (SelectionToolCollider.instance == null || !SelectionToolCollider.instance.selActive) return;
            Transform selTransform = SelectionToolCollider.instance.GetCurrentCollider().transform;
            Collider[] colliders = Physics.OverlapBox(selTransform.position, Vector3.one * 0.1f, Quaternion.identity,
                1 << LayerMask.NameToLayer("GraphLayer"));
            PointCloud pc = null;
            foreach (var col in colliders)
            {
                pc = col.GetComponent<PointCloud>();
                if (pc != null)
                {
                    break;
                }
            }
            if (pc == null) return;
            float3 selectionToolCenter = pc.transform.InverseTransformPoint(selTransform.position);
            //QuadrantSystem.DebugDrawCubes(selectionToolCenter, pc.transform);
            int hashMapKey = OctantSystem.GetPositionHashMapKey(selectionToolCenter);
            if (frameCount == 10)
            {
                CheckOneOctantYLayer(hashMapKey, -1, pc);
            }

            if (frameCount == 11)
            {
                CheckOneHalfOctantYLayer(hashMapKey, -1, pc);
            }

            if (frameCount == 12)
            {
                CheckOneOctantYLayer(hashMapKey, 0, pc);
            }

            if (frameCount == 13)
            {
                CheckOneHalfOctantYLayer(hashMapKey, 0, pc);
            }

            if (frameCount == 14)
            {
                CheckOneOctantYLayer(hashMapKey, 1, pc);
            }
            else if (frameCount == 15)
            {
                frameCount = 0;
                CheckOneHalfOctantYLayer(hashMapKey, 1, pc);
            }

            frameCount++;
        }

        /// <summary>
        /// Check one quadrant y layer for nearby points to see if they are inside the selection tool. 
        /// For more information what a quadrant is <see cref="OctantSystem"/>.
        /// </summary>
        /// <param name="hashMapKey"></param>
        /// <param name="y"></param>
        /// <param name="pc"></param>
        [BurstCompile]
        private void CheckOneOctantYLayer(int hashMapKey, int y, PointCloud pc)
        {
            CheckForNearbyEntities(hashMapKey + y * OctantSystem.octantYMultiplier, pc); // current quadrant
            CheckForNearbyEntities((hashMapKey + 1) + y * OctantSystem.octantYMultiplier, pc); // one to the right
            CheckForNearbyEntities((hashMapKey - 1) + y * OctantSystem.octantYMultiplier, pc); // one to the left
            CheckForNearbyEntities((hashMapKey) + 1 * OctantSystem.octantZMultiplier + y * OctantSystem.octantYMultiplier, pc); // and so on..
        }

        [BurstCompile]
        private void CheckOneHalfOctantYLayer(int hashMapKey, int y, PointCloud pc)
        {
            CheckForNearbyEntities((hashMapKey) - 1 * OctantSystem.octantZMultiplier + y * OctantSystem.octantYMultiplier, pc);
            CheckForNearbyEntities((hashMapKey + 1) + 1 * OctantSystem.octantZMultiplier + y * OctantSystem.octantYMultiplier, pc);
            CheckForNearbyEntities((hashMapKey - 1) - 1 * OctantSystem.octantZMultiplier + y * OctantSystem.octantYMultiplier, pc);
            CheckForNearbyEntities((hashMapKey + 1) - 1 * OctantSystem.octantZMultiplier + y * OctantSystem.octantYMultiplier, pc);
            CheckForNearbyEntities((hashMapKey - 1) + 1 * OctantSystem.octantZMultiplier + y * OctantSystem.octantYMultiplier, pc);
        }

        /// <summary>
        /// Check for points/entities near the selection tool and check if they are inside the selection tool mesh.
        /// </summary>
        /// <param name="hashMapKey"></param>
        /// <param name="pc"></param>
        [BurstCompile]
        private void CheckForNearbyEntities(int hashMapKey, PointCloud pc)
        {
            NativeList<RaycastCheckComponent> entityArray = new NativeList<RaycastCheckComponent>(Allocator.Temp);
            float3 origin = SelectionToolCollider.instance.GetCurrentCollider().transform.position;
            // If a childSlice check quadrant map of parent slice;
            if (OctantSystem.quadrantMultiHashMaps[pc.pcID].TryGetFirstValue(hashMapKey, out OctantData quadrantData,
                out NativeMultiHashMapIterator<int> nativeMultiHashMapIterator))
            {
                do
                {
                    var c1 = pc.colorTextureMap.GetPixel(quadrantData.xindex, quadrantData.yindex);
                    var c2 = SelectionToolCollider.instance.Colors[SelectionToolCollider.instance.CurrentColorIndex];
                    if (InputReader.CompareColor(c1, c2)) continue;
                    float3 pos = pc.transform.TransformPoint(quadrantData.position);
                    //Debug.DrawRay(origin, origin - pos, Color.green, 0.5f);
                    entityArray.Add(new RaycastCheckComponent
                    {
                        position = pos,
                        origin = origin,
                        orgXIndex = quadrantData.orgXIndex,
                        orgYIndex = quadrantData.orgYIndex,
                        xindex = quadrantData.xindex,
                        yindex = quadrantData.yindex,
                        label = quadrantData.label,
                        parentID = pc.pcID
                    });
                } while (OctantSystem.quadrantMultiHashMaps[pc.pcID].TryGetNextValue(out quadrantData,
                    ref nativeMultiHashMapIterator));
            }
            NativeArray<Entity> entities = new NativeArray<Entity>(entityArray.Length, Allocator.Temp);
            EntityManager.CreateEntity(entityArchetype, entities);
            for (int i = 0; i < entities.Length; i++)
            {
                EntityManager.SetComponentData(entities[i], new RaycastCheckComponent
                {
                    position = entityArray[i].position,
                    origin = entityArray[i].origin,
                    orgXIndex = entityArray[i].orgXIndex,
                    orgYIndex = entityArray[i].orgYIndex,
                    label = entityArray[i].label,
                    xindex = entityArray[i].xindex,
                    yindex = entityArray[i].yindex,
                    parentID = entityArray[i].parentID
                });
            }

            ecbSystem.AddJobHandleForProducer(Dependency);

            entities.Dispose();
            entityArray.Dispose();
        }
    }
}