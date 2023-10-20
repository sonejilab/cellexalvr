using CellexalVR.AnalysisLogic;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Main class to handle slicing of graphs.
    /// Slicing means dividing up the graph into two or more new graphs that can be interacted with individually.
    /// </summary>
    public class SliceGraphSystem : SystemBase
    {
        // private Slicer slicer;
        private EndSimulationEntityCommandBufferSystem ecbSystem;
        private OctantSystem quadrantSystem;
        private EntityQuery query;
        private GameObject slicer;
        private int graphToSliceID;
        private Transform graphToSlice;

        private GraphSlice slice1;
        private GraphSlice slice2;
        private PointCloud pc1;
        private PointCloud pc2;
        private float xMax;
        private float xMax2;
        private float yMax;
        private float yMax2;
        private float zMax;
        private float zMax2;
        private float xMin;
        private float xMin2;
        private float yMin;
        private float yMin2;
        private float zMin;
        private float zMin2;

        protected override void OnCreate()
        {
            base.OnCreate();
            quadrantSystem = World.GetOrCreateSystem<OctantSystem>();
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            query = GetEntityQuery(typeof(Point));
            slicer = GameObject.Find("SlicePlane");
        }

        protected override void OnUpdate() { }

        /// <summary>
        /// Slice from a plane. 
        /// This means that the graph will be divided into two new graphs.
        /// One on each side of the slicing plane.
        /// </summary>
        /// <param name="graphNr">The id of the graph to slice.</param>
        /// <param name="planeNormal">Normal of the plane used to calculate which side each point is on.</param>
        /// <param name="planePos">Position of plane used to calculate which side each point is on.</param>
        [BurstCompile]
        public void Slice(int graphNr, Vector3 planeNormal, Vector3 planePos)
        {
            Transform oldPc = quadrantSystem.graphParentTransforms[graphNr];
            graphToSlice = oldPc;
            float3 localPlanePos = oldPc.transform.InverseTransformPoint(planePos);
            float3 localPlaneNorm = oldPc.transform.InverseTransformVector(planeNormal);
            int entityCount = query.CalculateEntityCount();
            NativeArray<bool> move = new NativeArray<bool>(entityCount, Allocator.TempJob);
            JobHandle jobHandle = Entities.WithAll<Point>().WithStoreEntityQueryInField(ref query).ForEach(
                (Entity entity, int entityInQueryIndex, ref LocalToWorld localToWorld, ref Point point, ref Translation translation) =>
                {
                    if (point.parentID != graphNr && point.orgParentID != graphNr) return;
                    float side = math.dot(localPlaneNorm, (point.offset - localPlanePos));
                    if (side < 0)
                    {
                        move[entityInQueryIndex] = true;
                    }
                }).ScheduleParallel(Dependency);
            jobHandle.Complete();

            DoSlice(move, graphNr, planeNormal);
            move.Dispose();
        }

        /// <summary>
        /// Slice based on selected/highlighted points. 
        /// </summary>
        /// <param name="graphNr"></param>
        [BurstCompile]
        public void SliceFromSelection(int graphNr)
        {
            NativeArray<Color> colorArray = new NativeArray<Color>(TextureHandler.instance.colorTextureMaps[0].GetPixels(), Allocator.TempJob);
            int entityCount = query.CalculateEntityCount();
            NativeArray<bool> move = new NativeArray<bool>(entityCount, Allocator.TempJob);
            JobHandle jobHandle = Entities.WithAll<Point>().WithStoreEntityQueryInField(ref query).ForEach(
                (Entity entity, int entityInQueryIndex, ref LocalToWorld localToWorld, ref Point point, ref Translation translation) =>
                {
                    if (point.parentID != graphNr && point.orgParentID != graphNr) return;
                    move[entityInQueryIndex] = colorArray[entityInQueryIndex].a > 0.9f;
                }).ScheduleParallel(Dependency);
            jobHandle.Complete();

            colorArray.Dispose();
            DoSlice(move, graphNr, PointCloudGenerator.instance.pointClouds[graphNr].transform.forward * 2f);
            move.Dispose();
        }

        /// <summary>
        /// Divides up the points into two new sets of points and creates two new graphs.
        /// </summary>
        /// <param name="slice">Boolean array of each point will be true for one slice and false for the other. </param>
        /// <param name="graphNr">The id of the graph to slice. </param>
        /// <param name="moveDir">Direction to separate the slices from to easier see the slicing. </param>
        private void DoSlice(NativeArray<bool> slice, int graphNr, Vector3 moveDir)
        {
            Transform oldPc = quadrantSystem.graphParentTransforms[graphNr];
            graphToSlice = oldPc;
            xMax = float.NegativeInfinity;
            xMax2 = float.NegativeInfinity;
            yMax = float.NegativeInfinity;
            yMax2 = float.NegativeInfinity;
            zMax = float.NegativeInfinity;
            zMax2 = float.NegativeInfinity;
            xMin = float.PositiveInfinity;
            xMin2 = float.PositiveInfinity;
            yMin = float.PositiveInfinity;
            yMin2 = float.PositiveInfinity;
            zMin = float.PositiveInfinity;
            zMin2 = float.PositiveInfinity;
            List<Point> firstSlicePoints = new List<Point>();
            List<Point> secondSlicePoints = new List<Point>();
            Entities.WithoutBurst().WithAll<Point>().ForEach((Entity entity, int entityInQueryIndex, ref Point point) =>
            {
                if (point.parentID != graphNr && point.orgParentID != graphNr) return;
                if (slice[entityInQueryIndex])
                {
                    xMax = math.max(point.offset.x, xMax);
                    xMin = math.min(point.offset.x, xMin);
                    yMax = math.max(point.offset.y, yMax);
                    yMin = math.min(point.offset.y, yMin);
                    zMax = math.max(point.offset.z, zMax);
                    zMin = math.min(point.offset.z, zMin);
                    firstSlicePoints.Add(point);
                }
                else
                {
                    xMax2 = math.max(point.offset.x, xMax2);
                    xMin2 = math.min(point.offset.x, xMin2);
                    yMax2 = math.max(point.offset.y, yMax2);
                    yMin2 = math.min(point.offset.y, yMin2);
                    zMax2 = math.max(point.offset.z, zMax2);
                    zMin2 = math.min(point.offset.z, zMin2);
                    secondSlicePoints.Add(point);
                }
            }).Run();
            if (firstSlicePoints.ToList().Count > 0)
            {
                pc1 = PointCloudGenerator.instance.CreateFromOld(graphToSlice.transform);
                graphToSlice.GetComponent<GraphSlice>().ClearSlices();
                slice1 = pc1.GetComponent<GraphSlice>();
                slice1.sliceCoords = pc1.transform.position - 0.2f * moveDir;
                slice1.SliceNr = 0;
                slice1.gameObject.name = graphToSlice.gameObject.name + "_" + slice1.SliceNr;
                slice1.points = firstSlicePoints;
                slice1.pointCloud = pc1;
                float3 max = new float3(xMax, yMax, zMax);
                float3 min = new float3(xMin, yMin, zMin);
                pc1.maxCoordValues = max;
                pc1.minCoordValues = min;
                pc1.SetCollider(true);


                pc2 = PointCloudGenerator.instance.CreateFromOld(graphToSlice.transform);
                slice2 = pc2.GetComponent<GraphSlice>();
                slice2.sliceCoords = pc2.transform.position + 0.2f * moveDir;
                slice2.SliceNr = 1;
                slice2.gameObject.name = graphToSlice.gameObject.name + "_" + slice2.SliceNr;
                slice2.points = secondSlicePoints;
                slice2.pointCloud = pc2;
                max = new Vector3(xMax2, yMax2, zMax2);
                min = new Vector3(xMin2, yMin2, zMin2);
                pc2.maxCoordValues = max;
                pc2.minCoordValues = min;
                pc2.SetCollider(true);

                quadrantSystem.graphParentTransforms.Add(pc1.transform);
                quadrantSystem.graphParentTransforms.Add(pc2.transform);
                PointCloudGenerator.instance.BuildSlices(graphToSlice, new GraphSlice[] { slice1, slice2 });
            }

        }

        /// <summary>
        /// Helper function to retrieve the points for a certain graph.
        /// </summary>
        /// <param name="graphID">The id of the graph of interest.</param>
        /// <returns></returns>
        public List<Point> GetPoints(int graphID)
        {
            List<Point> points = new List<Point>();
            Entities.WithoutBurst().WithAll<Point>().ForEach((Entity entity, int entityInQueryIndex, ref Point point) =>
            {
                if (point.parentID != graphID && point.orgParentID != graphID) return;
                points.Add(point);
            }).Run();

            return points;
        }

        /// <summary>
        /// Sorts the points based on position in a certain axis.
        /// </summary>
        /// <param name="points">The points to sort.</param>
        /// <param name="axis">The axis to use for sorting.</param>
        /// <returns></returns>
        public static List<Point> SortPoints(IReadOnlyCollection<Point> points, int axis)
        {
            List<Point> sortedPoints = new List<Point>(points);
            sortedPoints.Sort((x, y) => x.offset[axis].CompareTo(y.offset[axis]));
            return sortedPoints;
        }
    }
}