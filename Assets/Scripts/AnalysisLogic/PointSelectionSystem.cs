using AnalysisLogic;
using CellexalVR.Interaction;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace
{
    public struct SelectedPointComponent : IComponentData
    {
        public int xindex;
        public int yindex;
        public int label;
        public int group;
        public int parentID;
    }

    public struct RaycastCheckComponent : IComponentData
    {
        public float3 position;
        public float3 origin;
        public int xindex;
        public int yindex;
        public int label;
        public int parentID;
    }

    public class PointSelectionSystem : SystemBase
    {
        private Entity brainParent;
        private bool textureChanged;
        private int frameCount;
        private EntityQuery query;
        private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private GameObject newParent;
        private EntityArchetype entityArchetype;

        private EntityManager entityManager;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityManager = World.EntityManager;
            ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
            entityArchetype = EntityManager.CreateArchetype(typeof(RaycastCheckComponent));
        }

        public void CreateMesh()
        {
            EntityQuery group = GetEntityQuery(typeof(SelectedPointComponent), typeof(Point));
            NativeArray<Entity> points = group.ToEntityArray(Allocator.Temp);
        }

        protected override void OnUpdate()
        {
            if (SelectionToolCollider.instance == null || !SelectionToolCollider.instance.selActive) return;
            Transform selTransform = SelectionToolCollider.instance.GetCurrentCollider().transform;
            Collider[] colliders = Physics.OverlapBox(selTransform.position, Vector3.one * 0.1f, Quaternion.identity,
                1 << LayerMask.NameToLayer("GraphLayer"));
            PointCloud pointCloud = null;
            foreach (var col in colliders)
            {
                pointCloud = col.GetComponent<PointCloud>();
                if (pointCloud != null)
                {
                    break;
                }
            }

            if (pointCloud == null) return;
            PointCloud mat = pointCloud;
            float3 selectionToolCenter = pointCloud.transform.InverseTransformPoint(selTransform.position);
            //QuadrantSystem.DebugDrawCubes(selectionToolCenter, mat.transform);
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(selectionToolCenter, (int)selTransform.localScale.x);
            if (frameCount == 10)
            {
                CheckOneQuadrantYLayer(hashMapKey, -1, mat);
            }

            if (frameCount == 11)
            {
                CheckOneHalfQuadrantYLayer(hashMapKey, -1, mat);
            }

            if (frameCount == 12)
            {
                CheckOneQuadrantYLayer(hashMapKey, 0, mat);
            }

            if (frameCount == 13)
            {
                CheckOneHalfQuadrantYLayer(hashMapKey, 0, mat);
            }

            if (frameCount == 14)
            {
                CheckOneQuadrantYLayer(hashMapKey, 1, mat);
            }
            else if (frameCount == 15)
            {
                frameCount = 0;
                CheckOneHalfQuadrantYLayer(hashMapKey, 1, mat);
            }

            frameCount++;
        }

        [BurstCompile]
        private void CheckOneQuadrantYLayer(int hashMapKey, int y, PointCloud mat)
        {
            CheckForNearbyEntities(hashMapKey + y * QuadrantSystem.quadrantYMultiplier, mat); // current quadrant
            CheckForNearbyEntities((hashMapKey + 1) + y * QuadrantSystem.quadrantYMultiplier, mat); // one to the right
            CheckForNearbyEntities((hashMapKey - 1) + y * QuadrantSystem.quadrantYMultiplier, mat); // one to the left
            CheckForNearbyEntities((hashMapKey) + 1 * QuadrantSystem.quadrantZMultiplier + y * QuadrantSystem.quadrantYMultiplier, mat); // and so on..
        }

        [BurstCompile]
        private void CheckOneHalfQuadrantYLayer(int hashMapKey, int y, PointCloud mat)
        {
            CheckForNearbyEntities((hashMapKey) - 1 * QuadrantSystem.quadrantZMultiplier + y * QuadrantSystem.quadrantYMultiplier, mat);
            CheckForNearbyEntities((hashMapKey + 1) + 1 * QuadrantSystem.quadrantZMultiplier + y * QuadrantSystem.quadrantYMultiplier, mat);
            CheckForNearbyEntities((hashMapKey - 1) - 1 * QuadrantSystem.quadrantZMultiplier + y * QuadrantSystem.quadrantYMultiplier, mat);
            CheckForNearbyEntities((hashMapKey + 1) - 1 * QuadrantSystem.quadrantZMultiplier + y * QuadrantSystem.quadrantYMultiplier, mat);
            CheckForNearbyEntities((hashMapKey - 1) + 1 * QuadrantSystem.quadrantZMultiplier + y * QuadrantSystem.quadrantYMultiplier, mat);
        }


        [BurstCompile]
        private void CheckForNearbyEntities(int hashMapKey, PointCloud pc)
        {
            NativeList<RaycastCheckComponent> entityArray = new NativeList<RaycastCheckComponent>(Allocator.Temp);
            float3 origin = SelectionToolCollider.instance.GetCurrentCollider().transform.position;
            if (QuadrantSystem.quadrantMultiHashMaps[pc.pcID].TryGetFirstValue(hashMapKey, out QuadrantData quadrantData,
                out NativeMultiHashMapIterator<int> nativeMultiHashMapIterator))
            {
                do
                {
                    if (quadrantData.group == SelectionToolCollider.instance.CurrentColorIndex) continue;
                    float3 pos = pc.transform.TransformPoint(quadrantData.position);
                    entityArray.Add(new RaycastCheckComponent {position = pos, origin = origin, xindex = quadrantData.xindex, yindex = quadrantData.yindex, label = quadrantData.label, parentID = pc.pcID});
                } while (QuadrantSystem.quadrantMultiHashMaps[pc.pcID].TryGetNextValue(out quadrantData,
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


        // [BurstCompile]
        // private void MoveToReferenceBrain(QuadrantData quadrantData, Entity entity, Point point)
        // {
        //     if (brainParent == Entity.Null)
        //     {
        //         GameObject brain = GameObject.Find("BrainParent");
        //         if (!brain) return;
        //         brainParent = AddNewParent(brain);
        //         int graphNr = PointSpawner.instance.nrOfGraphs++;
        //         PointMoveSystem pointMoveSystem = World.GetExistingSystem<PointMoveSystem>();
        //         pointMoveSystem.graphParentTransforms.Add(brain.transform);
        //         pointMoveSystem.graphParentTransforms[graphNr] = brain.transform;
        //     }
        //
        //     Point qdPoint = quadrantData.point;
        //     entityManager.AddComponent<PointMovedToNewParent>(quadrantData.entity);
        //     entityManager.AddComponent<MoveTowards>(quadrantData.entity);
        //     entityManager.SetComponentData(quadrantData.entity, new MoveTowards {speed = 0.5f});
        //     SetComponent(quadrantData.entity, new PointMovedToNewParent
        //     {
        //         previousParent = quadrantData.point.parent,
        //         newParent = brainParent
        //     });
        //     qdPoint.parentId = -1;
        //     qdPoint.previousParent = point.parent;
        //     qdPoint.parent = brainParent;
        //     qdPoint.offset = point.offset;
        //     // Debug.Log($"p : {point.offset}");
        //     SetComponent(quadrantData.entity, qdPoint);
        // }

        // [BurstCompile]
        // public Entity AddNewParent(GameObject newParent)
        // {
        //     float3 newParentPos = newParent.transform.position;
        //     quaternion newParentRot = newParent.transform.rotation;
        //     float3 newParentScale = newParent.transform.localScale;
        //     int graphNr = PointSpawner.instance.nrOfGraphs;
        //     // PointMoveSystem pointMoveSystem = World.GetExistingSystem<PointMoveSystem>();
        //     // pointMoveSystem.graphParentTransforms.Add(newParent.transform);
        //     // pointMoveSystem.graphParentTransforms[graphNr] = newParent.transform;
        //     newParent.transform.hasChanged = false;
        //     Entity parent = entityManager.CreateEntity(PointSpawner.instance.parentEntityArcheType);
        //
        //     entityManager.SetComponentData(parent, new Translation
        //     {
        //         Value = newParentPos
        //     });
        //     entityManager.SetComponentData(parent, new Rotation
        //     {
        //         Value = newParentRot
        //     });
        //     entityManager.SetComponentData(parent, new Scale
        //     {
        //         Value = newParentScale.x
        //     });
        //     entityManager.SetComponentData(parent, new GraphParent
        //     {
        //         graphNr = graphNr,
        //         pointCount = 0
        //     });
        //     // entityManager.AddComponent<ReferenceBrain>(brainParent);
        //     return parent;
        // }
    }
}