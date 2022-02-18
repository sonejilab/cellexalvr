using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using DefaultNamespace;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CellexalVR.Spatial
{
    public struct Slice1TagComponent : IComponentData
    {
    }
    public struct Slice2TagComponent : IComponentData
    {
    }

    public struct RemoveEntityTagComponent : IComponentData
    {
    }

    public class SliceGraphSystem : SystemBase
    {
        // private Slicer slicer;
        private EndSimulationEntityCommandBufferSystem ecbSystem;
        private QuadrantSystem quadrantSystem;
        private EntityQuery query;
        private GameObject slicer;
        private List<Point> sortedPointsX;
        private List<Point> sortedPointsY;
        private List<Point> sortedPointsZ;
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
            quadrantSystem = World.GetOrCreateSystem<QuadrantSystem>();
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            query = GetEntityQuery(typeof(Point));
            slicer = GameObject.Find("SlicePlane");
        }

        protected override void OnUpdate() {}

        [BurstCompile]
        public void Slice(int graphNr, Vector3 planeNormal, Vector3 planePos)
        {
            Transform oldPc = quadrantSystem.graphParentTransforms[graphNr];
            graphToSlice = oldPc;
            GraphSlice parentSlice = oldPc.GetComponent<GraphSlice>();
            float3 localPlanePos = oldPc.transform.InverseTransformPoint(planePos);
            float3 localPlaneNorm = oldPc.transform.InverseTransformVector(planeNormal);
            int entityCount = query.CalculateEntityCount();
            NativeArray<bool> move = new NativeArray<bool>(entityCount, Allocator.TempJob);
            //NativeArray<float3> dirs = new NativeArray<float3>(entityCount, Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            JobHandle jobHandle = Entities.WithAll<Point>().WithStoreEntityQueryInField(ref query).ForEach(
                (Entity entity, int entityInQueryIndex, ref LocalToWorld localToWorld, ref Point point, ref Translation translation) =>
                {
                    if (point.parentID != graphNr) return;
                    float side = math.dot(localPlaneNorm, (point.offset - localPlanePos));
                    if (side < 0)
                    {
                        move[entityInQueryIndex] = true;
                        //dirs[entityInQueryIndex] = localPlaneNorm * -1;
                        //ecb.AddComponent<Slice1TagComponent>(entityInQueryIndex, entity);
                    }
                    //else
                    //{
                    //    dirs[entityInQueryIndex] = localPlaneNorm;
                    //    //ecb.AddComponent<Slice2TagComponent>(entityInQueryIndex, entity);
                    //}
                }).ScheduleParallel(Dependency);
            jobHandle.Complete();


            //oldPc.GetComponent<PointCloud>().SliceSpread(dirs);

            //dirs.Dispose();
            //ecbSystem.AddJobHandleForProducer(Dependency);
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
                    if (point.parentID != graphNr) return;
                    if (move[entityInQueryIndex])
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
                slice1.sliceCoords = pc1.transform.position - 0.2f * planeNormal;
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
                slice2.sliceCoords = pc2.transform.position + 0.2f * planeNormal;
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
            move.Dispose();
        }


        public List<Point> GetPoints(int graphID)
        {
            List<Point> points = new List<Point>();
            Entities.WithoutBurst().WithAll<Point>().ForEach((Entity entity, int entityInQueryIndex, ref Point point) =>
            {
                if (point.parentID != graphID) return;
                points.Add(point);
            }).Run();

            return points;
        }


        public static List<Point> SortPoints(IReadOnlyCollection<Point> points, int axis)
        {
            List<Point> sortedPoints = new List<Point>(points);
            sortedPoints.Sort((x, y) => x.offset[axis].CompareTo(y.offset[axis]));
            return sortedPoints;
        }


        // public void GatherSlices()
        // {
        //     EntityCommandBuffer.ParallelWriter ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
        //     Entities.WithAll<PointMovedToNewParent>().ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Point point, ref PointMovedToNewParent pointMovedToNewParent) =>
        //     {
        //         GraphParent gpPrev = GetComponent<GraphParent>(pointMovedToNewParent.previousParent);
        //         // if (gpPrev.graphNr != graphNr) return;
        //         pointMovedToNewParent.newParent = pointMovedToNewParent.previousParent;
        //         pointMovedToNewParent.previousParent = point.parent;
        //         point.parentId = gpPrev.graphNr;
        //         point.previousParent = point.parent;
        //         point.parent = pointMovedToNewParent.newParent;
        //         ecb.AddComponent<MoveTowards>(entityInQueryIndex, entity);
        //         ecb.SetComponent(entityInQueryIndex, entity, new MoveTowards {speed = 1.2f});
        //         ecb.SetComponent(entityInQueryIndex, entity, pointMovedToNewParent);
        //         ecb.SetComponent(entityInQueryIndex, entity, point);
        //     }).ScheduleParallel();
        //     ecbSystem.AddJobHandleForProducer(Dependency);
        //     // DestroyEmptySlices();
        // }
        //
        // public void GatherSlicesToOriginal()
        // {
        //     EntityCommandBuffer.ParallelWriter ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
        //     Entities.WithAll<PointMovedToNewParent>().ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Point point, ref PointMovedToNewParent pointMovedToNewParent) =>
        //     {
        //         // if (gpPrev.graphNr != graphNr) return;
        //         pointMovedToNewParent.newParent = point.originalParent;
        //         pointMovedToNewParent.previousParent = point.parent;
        //         GraphParent graphParent = GetComponent<GraphParent>(point.originalParent);
        //         point.parentId = graphParent.graphNr;
        //         point.previousParent = point.parent;
        //         point.parent = pointMovedToNewParent.newParent;
        //         ecb.AddComponent<MoveTowards>(entityInQueryIndex, entity);
        //         ecb.SetComponent(entityInQueryIndex, entity, new MoveTowards {speed = 1.2f});
        //         ecb.SetComponent(entityInQueryIndex, entity, pointMovedToNewParent);
        //         ecb.SetComponent(entityInQueryIndex, entity, point);
        //     }).ScheduleParallel();
        //     ecbSystem.AddJobHandleForProducer(Dependency);
        //     // DestroyEmptySlices();
        // }
    }
}