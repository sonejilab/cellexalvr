using System.Collections.Generic;
using System.Linq;
using CellexalVR.General;
using CellexalVR.Interaction;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace DefaultNamespace
{
    // [MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
    // public struct PointColor : IComponentData
    // {
    //     public float4 color;
    // }
    //
    // [MaterialProperty("_Alpha", MaterialPropertyFormat.Float)]
    // public struct Alpha : IComponentData
    // {
    //     public float value;
    // }

    // [MaterialProperty("_PlanePosition", MaterialPropertyFormat.Float4)]
    // public struct PlanePositionComponent : IComponentData
    // {
    //     public float4 planePosition;
    // }
    //
    // [MaterialProperty("_PlaneNormal", MaterialPropertyFormat.Float4)]
    // public struct PlaneNormalComponent : IComponentData
    // {
    //     public float4 planeNormal;
    // }

    public struct QuadrantData
    {
        public float3 position;
        public int orgXIndex;
        public int orgYIndex;
        public int xindex;
        public int yindex;
        public int label;
        public int group;
        public int parentID;
    }

    public class QuadrantSystem : SystemBase
    {
        public const int quadrantYMultiplier = 1000;
        public const int quadrantZMultiplier = 100;
        public const int quadrantCellSize = 1;
        public static List<NativeMultiHashMap<int, QuadrantData>> quadrantMultiHashMaps;

        public List<Transform> graphParentTransforms = new List<Transform>();


        private bool updateQuadrantSystem;
        private EntityQuery query;
        private EndSimulationEntityCommandBufferSystem ecbSystem;

        public static int GetPositionHashMapKey(float3 position, int scale = 1)
        {
            return (int) (math.floor((position.x * 10) / quadrantCellSize) +
                          quadrantYMultiplier * math.floor((position.y * 10) / quadrantCellSize) +
                          quadrantZMultiplier * math.floor((position.z * 10) / quadrantCellSize));
        }

        public static int GetEntityCountInHashMap(NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap,
            int hashMapKey)
        {
            QuadrantData quadrantData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            int count = 0;
            if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
            {
                do
                {
                    count++;
                } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
            }

            return count;
        }

        [BurstCompile]
        private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<LocalToWorld, Point>
        {
            public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMultiHashMap;
            public int id;

            public void Execute(Entity entity, int index, ref LocalToWorld localToWorld,
                ref Point point)
            {
                if (point.parentID != id) return;
                int x = point.xindex;
                int y = point.yindex;
                int label = point.label;
                float3 pos = point.offset;
                int hashMapKey = GetPositionHashMapKey(point.offset);
                quadrantMultiHashMap.Add(hashMapKey, new QuadrantData
                {
                    orgXIndex = point.orgXIndex,
                    orgYIndex = point.orgYIndex,
                    xindex = x,
                    yindex = y,
                    label = label,
                    position = pos,
                    group = -1,
                    parentID = id
                });
            }
        }

        protected override void OnCreate()
        {
            quadrantMultiHashMaps = new List<NativeMultiHashMap<int, QuadrantData>>();
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            foreach (var quadrantMultiHashMap in quadrantMultiHashMaps)
            {
                quadrantMultiHashMap.Dispose();
            }

            base.OnDestroy();
        }

        [BurstCompile]
        public void SetHashMap(int id)
        {
            NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
            quadrantMultiHashMaps.Add(quadrantMultiHashMap);
            EntityQuery entityQuery = GetEntityQuery(typeof(Point), typeof(LocalToWorld));
            quadrantMultiHashMap.Clear();
            if (entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity)
            {
                quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
            }

            var qmap = quadrantMultiHashMap.AsParallelWriter();

            JobHandle jobHandle = Entities.WithAll<Point>().ForEach((Entity e, int entityInQueryIndex, ref Point point) =>
            {
                if (point.parentID != id) return;
                int x = point.xindex;
                int y = point.yindex;
                int label = point.label;
                float3 pos = point.offset;
                int hashMapKey = GetPositionHashMapKey(point.offset);
                qmap.Add(hashMapKey, new QuadrantData
                {
                    orgXIndex = point.orgXIndex,
                    orgYIndex = point.orgYIndex,
                    xindex = x,
                    yindex = y,
                    label = label,
                    position = pos,
                    group = -1,
                    parentID = id
                });
            }).ScheduleParallel(Dependency);
            jobHandle.Complete();

            //EntityManager.DestroyEntity(GetEntityQuery(typeof(Point)));
            //CellexalLog.Log($"{n} Quadrant Hash map(s) set");
        }

        protected override void OnUpdate()
        {
            // if (Input.GetKeyDown(KeyCode.K))
            // {
            //     SetHashMap(2);
            // }
        }


        #region Debug

        public static void DebugDrawCubes(float3 position, Transform t)
        {
            DebugDrawCube(position, t);

            DebugDrawCube(position - new float3(-quadrantCellSize / 10f, -quadrantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(-quadrantCellSize / 10f, -quadrantCellSize / 10f, -quadrantCellSize / 10f), t);
            DebugDrawCube(position - new float3(-quadrantCellSize / 10f, -quadrantCellSize / 10f, quadrantCellSize / 10f), t);

            DebugDrawCube(position - new float3(-quadrantCellSize / 10f, 0, -quadrantCellSize / 10f), t);
            DebugDrawCube(position - new float3(-quadrantCellSize / 10f, 0, 0), t);
            DebugDrawCube(position - new float3(-quadrantCellSize / 10f, 0, quadrantCellSize / 10f), t);

            DebugDrawCube(position - new float3(-quadrantCellSize / 10f, quadrantCellSize / 10f, -quadrantCellSize / 10f), t);
            DebugDrawCube(position - new float3(-quadrantCellSize / 10f, quadrantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(-quadrantCellSize / 10f, quadrantCellSize / 10f, quadrantCellSize / 10f), t);

            DebugDrawCube(position - new float3(0, -quadrantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(0, -quadrantCellSize / 10f, -quadrantCellSize / 10f), t);
            DebugDrawCube(position - new float3(0, -quadrantCellSize / 10f, quadrantCellSize / 10f), t);

            DebugDrawCube(position - new float3(0, 0, -quadrantCellSize / 10f), t);
            DebugDrawCube(position - new float3(0, 0, 0), t);
            DebugDrawCube(position - new float3(0, 0, quadrantCellSize / 10f), t);

            DebugDrawCube(position - new float3(0, quadrantCellSize / 10f, -quadrantCellSize / 10f), t);
            DebugDrawCube(position - new float3(0, quadrantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(0, quadrantCellSize / 10f, quadrantCellSize / 10f), t);

            DebugDrawCube(position - new float3(quadrantCellSize / 10f, -quadrantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(quadrantCellSize / 10f, -quadrantCellSize / 10f, -quadrantCellSize / 10f), t);
            DebugDrawCube(position - new float3(quadrantCellSize / 10f, -quadrantCellSize / 10f, quadrantCellSize / 10f), t);

            DebugDrawCube(position - new float3(quadrantCellSize / 10f, 0, -quadrantCellSize / 10f), t);
            DebugDrawCube(position - new float3(quadrantCellSize / 10f, 0, 0), t);
            DebugDrawCube(position - new float3(quadrantCellSize / 10f, 0, quadrantCellSize / 10f), t);

            DebugDrawCube(position - new float3(quadrantCellSize / 10f, quadrantCellSize / 10f, -quadrantCellSize / 10f), t);
            DebugDrawCube(position - new float3(quadrantCellSize / 10f, quadrantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(quadrantCellSize / 10f, quadrantCellSize / 10f, quadrantCellSize / 10f), t);
        }

        public static void DebugDrawCube(float3 position, Transform t)
        {
            Vector3 lowerLeft = new Vector3(math.floor((position.x * 10) / quadrantCellSize) * quadrantCellSize,
                math.floor((position.y * 10) / quadrantCellSize) * quadrantCellSize,
                math.floor((position.z * 10) / quadrantCellSize) * quadrantCellSize);

            lowerLeft /= 10;
            float size = quadrantCellSize / 10f;

            Vector3[] corners = new Vector3[]
            {
                lowerLeft,
                new Vector3(lowerLeft.x + size, lowerLeft.y, lowerLeft.z),
                new Vector3(lowerLeft.x, lowerLeft.y + size, lowerLeft.z),
                new Vector3(lowerLeft.x, lowerLeft.y, lowerLeft.z + size),
                new Vector3(lowerLeft.x + size, lowerLeft.y + size, lowerLeft.z + size),
                new Vector3(lowerLeft.x + size, lowerLeft.y, lowerLeft.z + size),
                new Vector3(lowerLeft.x, lowerLeft.y + size, lowerLeft.z + size),
                new Vector3(lowerLeft.x + size, lowerLeft.y + size, lowerLeft.z)
            };
            Debug.DrawLine(t.TransformPoint(corners[0]), t.TransformPoint(corners[1]));
            Debug.DrawLine(t.TransformPoint(corners[0]), t.TransformPoint(corners[2]));
            Debug.DrawLine(t.TransformPoint(corners[0]), t.TransformPoint(corners[3]));
            Debug.DrawLine(t.TransformPoint(corners[1]), t.TransformPoint(corners[5]));
            Debug.DrawLine(t.TransformPoint(corners[1]), t.TransformPoint(corners[7]));
            Debug.DrawLine(t.TransformPoint(corners[2]), t.TransformPoint(corners[7]));
            Debug.DrawLine(t.TransformPoint(corners[2]), t.TransformPoint(corners[6]));
            Debug.DrawLine(t.TransformPoint(corners[3]), t.TransformPoint(corners[6]));
            Debug.DrawLine(t.TransformPoint(corners[3]), t.TransformPoint(corners[5]));
            Debug.DrawLine(t.TransformPoint(corners[4]), t.TransformPoint(corners[6]));
            Debug.DrawLine(t.TransformPoint(corners[4]), t.TransformPoint(corners[7]));
            Debug.DrawLine(t.TransformPoint(corners[4]), t.TransformPoint(corners[5]));

            // Debug.DrawLine((corners[0]), (corners[1]));
            // Debug.DrawLine((corners[0]), (corners[2]));
            // Debug.DrawLine((corners[0]), (corners[3]));
            // Debug.DrawLine((corners[1]), (corners[5]));
            // Debug.DrawLine((corners[1]), (corners[7]));
            // Debug.DrawLine((corners[2]), (corners[7]));
            // Debug.DrawLine((corners[2]), (corners[6]));
            // Debug.DrawLine((corners[3]), (corners[6]));
            // Debug.DrawLine((corners[3]), (corners[5]));
            // Debug.DrawLine((corners[4]), (corners[6]));
            // Debug.DrawLine((corners[4]), (corners[7]));
            // Debug.DrawLine((corners[4]), (corners[5]));
        }

        #endregion
    }
}