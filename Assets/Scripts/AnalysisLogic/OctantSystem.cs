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

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// The data struct of a data point inside an octant.
    /// </summary>
    public struct OctantData
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

    /// <summary>
    /// This class handles part of the point selection logics. 
    /// It divides a graph into octants and stores data in these octants so it knows which data to look for when a users selection tool is in a certain part of the graph.
    /// </summary>
    public class OctantSystem : SystemBase
    {
        public const int octantYMultiplier = 1000;
        public const int octantZMultiplier = 100;
        public const int octantCellSize = 1;
        public static List<NativeMultiHashMap<int, OctantData>> quadrantMultiHashMaps;

        public List<Transform> graphParentTransforms = new List<Transform>();

        /// <summary>
        /// Depending on a position in the graph it returns the key of the octant.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static int GetPositionHashMapKey(float3 position, int scale = 1)
        {
            return (int) (math.floor((position.x * 10) / octantCellSize) +
                          octantYMultiplier * math.floor((position.y * 10) / octantCellSize) +
                          octantZMultiplier * math.floor((position.z * 10) / octantCellSize));
        }

        /// <summary>
        /// Given a octant key it returns how many points are in that octant.
        /// </summary>
        /// <param name="octantMultiHashMap">The hashmap containing all the octant data components of the graph.</param>
        /// <param name="hashMapKey">The octant key.</param>
        /// <returns></returns>
        public static int GetEntityCountInHashMap(NativeMultiHashMap<int, OctantData> octantMultiHashMap,
            int hashMapKey)
        {
            OctantData quadrantData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            int count = 0;
            if (octantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
            {
                do
                {
                    count++;
                } while (octantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
            }

            return count;
        }

        /// <summary>
        /// Sets the references between the points and their position and to which octant it belongs to.
        /// </summary>
        [BurstCompile]
        private struct SetOctantDataHashMapJob : IJobForEachWithEntity<LocalToWorld, Point>
        {
            public NativeMultiHashMap<int, OctantData>.ParallelWriter octantMultiHashMap;
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
                octantMultiHashMap.Add(hashMapKey, new OctantData
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
            quadrantMultiHashMaps = new List<NativeMultiHashMap<int, OctantData>>();
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


        /// <summary>
        /// Create the hashmap containing the octant data (point data) and decides to which octant it belongs.
        /// </summary>
        /// <param name="id"></param>
        [BurstCompile]
        public void SetHashMap(int id)
        {
            NativeMultiHashMap<int, OctantData> octantMultiHashMap = new NativeMultiHashMap<int, OctantData>(0, Allocator.Persistent);
            quadrantMultiHashMaps.Add(octantMultiHashMap);
            EntityQuery entityQuery = GetEntityQuery(typeof(Point), typeof(LocalToWorld));
            octantMultiHashMap.Clear();
            if (entityQuery.CalculateEntityCount() > octantMultiHashMap.Capacity)
            {
                octantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
            }

            var qmap = octantMultiHashMap.AsParallelWriter();
            JobHandle jobHandle = Entities.WithAll<Point>().ForEach((Entity e, int entityInQueryIndex, ref Point point) =>
            {
                if (point.parentID != id) return;
                int x = point.xindex;
                int y = point.yindex;
                int label = point.label;
                float3 pos = point.offset;
                int hashMapKey = GetPositionHashMapKey(point.offset);
                qmap.Add(hashMapKey, new OctantData
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
        }

        protected override void OnUpdate() {}


        #region Debug

        public static void DebugDrawCubes(float3 position, Transform t)
        {
            DebugDrawCube(position, t);

            DebugDrawCube(position - new float3(-octantCellSize / 10f, -octantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(-octantCellSize / 10f, -octantCellSize / 10f, -octantCellSize / 10f), t);
            DebugDrawCube(position - new float3(-octantCellSize / 10f, -octantCellSize / 10f, octantCellSize / 10f), t);

            DebugDrawCube(position - new float3(-octantCellSize / 10f, 0, -octantCellSize / 10f), t);
            DebugDrawCube(position - new float3(-octantCellSize / 10f, 0, 0), t);
            DebugDrawCube(position - new float3(-octantCellSize / 10f, 0, octantCellSize / 10f), t);

            DebugDrawCube(position - new float3(-octantCellSize / 10f, octantCellSize / 10f, -octantCellSize / 10f), t);
            DebugDrawCube(position - new float3(-octantCellSize / 10f, octantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(-octantCellSize / 10f, octantCellSize / 10f, octantCellSize / 10f), t);

            DebugDrawCube(position - new float3(0, -octantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(0, -octantCellSize / 10f, -octantCellSize / 10f), t);
            DebugDrawCube(position - new float3(0, -octantCellSize / 10f, octantCellSize / 10f), t);

            DebugDrawCube(position - new float3(0, 0, -octantCellSize / 10f), t);
            DebugDrawCube(position - new float3(0, 0, 0), t);
            DebugDrawCube(position - new float3(0, 0, octantCellSize / 10f), t);

            DebugDrawCube(position - new float3(0, octantCellSize / 10f, -octantCellSize / 10f), t);
            DebugDrawCube(position - new float3(0, octantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(0, octantCellSize / 10f, octantCellSize / 10f), t);

            DebugDrawCube(position - new float3(octantCellSize / 10f, -octantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(octantCellSize / 10f, -octantCellSize / 10f, -octantCellSize / 10f), t);
            DebugDrawCube(position - new float3(octantCellSize / 10f, -octantCellSize / 10f, octantCellSize / 10f), t);

            DebugDrawCube(position - new float3(octantCellSize / 10f, 0, -octantCellSize / 10f), t);
            DebugDrawCube(position - new float3(octantCellSize / 10f, 0, 0), t);
            DebugDrawCube(position - new float3(octantCellSize / 10f, 0, octantCellSize / 10f), t);

            DebugDrawCube(position - new float3(octantCellSize / 10f, octantCellSize / 10f, -octantCellSize / 10f), t);
            DebugDrawCube(position - new float3(octantCellSize / 10f, octantCellSize / 10f, 0), t);
            DebugDrawCube(position - new float3(octantCellSize / 10f, octantCellSize / 10f, octantCellSize / 10f), t);
        }

        public static void DebugDrawCube(float3 position, Transform t)
        {
            Vector3 lowerLeft = new Vector3(math.floor((position.x * 10) / octantCellSize) * octantCellSize,
                math.floor((position.y * 10) / octantCellSize) * octantCellSize,
                math.floor((position.z * 10) / octantCellSize) * octantCellSize);

            lowerLeft /= 10;
            float size = octantCellSize / 10f;

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