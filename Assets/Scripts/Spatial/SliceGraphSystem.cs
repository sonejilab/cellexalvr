using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Spatial
{
    public struct SliceTagComponent : IComponentData
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

        protected override void OnCreate()
        {
            base.OnCreate();
            quadrantSystem = World.GetOrCreateSystem<QuadrantSystem>();
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            query = GetEntityQuery(typeof(Point));
            slicer = GameObject.Find("SlicePlane");
        }

        protected override void OnUpdate()
        {
            //if (slicer == null)
            //{
            //    slicer = GameObject.Find("SlicePlane");
            //}

            //EntityManager.DestroyEntity(GetEntityQuery(typeof(RemoveEntityTagComponent)));
            //if (Input.GetKeyDown(KeyCode.K))
            //{
            // Slice(0, slicer.forward, slicer.position);
            //SliceAxis(0, 2);
            //}

            //if (Input.GetKeyDown(KeyCode.N))
            //{
            //    Slice(0, slicer.transform.forward, slicer.transform.position);
            //}
        }

        public void Slice(int graphNr, Vector3 planeNormal, Vector3 planePos)
        {
            Transform oldPc = quadrantSystem.graphParentTransforms[graphNr];
            GraphSlice parentSlice = oldPc.GetComponent<GraphSlice>();
            float3 localPlanePos = oldPc.transform.InverseTransformPoint(planePos);
            float3 localPlaneNorm = oldPc.transform.InverseTransformVector(planeNormal);
            int entityCount = query.CalculateEntityCount();
            NativeArray<bool> move = new NativeArray<bool>(entityCount, Allocator.TempJob);
            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer();
            float xMax = float.NegativeInfinity;
            float xMax2 = float.NegativeInfinity;
            float yMax = float.NegativeInfinity;
            float yMax2 = float.NegativeInfinity;
            float zMax = float.NegativeInfinity;
            float zMax2 = float.NegativeInfinity;
            float xMin = float.PositiveInfinity;
            float xMin2 = float.PositiveInfinity;
            float yMin = float.PositiveInfinity;
            float yMin2 = float.PositiveInfinity;
            float zMin = float.PositiveInfinity;
            float zMin2 = float.PositiveInfinity;
            JobHandle jobHandle = Entities.WithAll<Point>().WithStoreEntityQueryInField(ref query).ForEach(
                (Entity entity, int entityInQueryIndex, ref LocalToWorld localToWorld, ref Point point, ref Translation translation) =>
                {
                    if (point.parentID != graphNr) return;
                    float side = math.dot(localPlaneNorm, (point.offset - localPlanePos));
                    if (side < 0)
                    {
                        move[entityInQueryIndex] = true;
                    }
                }).ScheduleParallel(Dependency);
            jobHandle.Complete();

            //Dictionary<int, float3> points = new Dictionary<int, float3>();
            //Dictionary<int, float3> points2 = new Dictionary<int, float3>();
            List<Point> points = new List<Point>();
            List<Point> points2 = new List<Point>();
            Entities.WithoutBurst().WithAll<Point>().ForEach((Entity entity, int entityInQueryIndex, ref Point point) =>
            {
                if (point.parentID != graphNr) return;
                ecb.AddComponent<RemoveEntityTagComponent>(entity);
                if (move[entityInQueryIndex])
                {
                    //points[point.label] = point.offset;
                    if (point.offset.x > xMax)
                    {
                        xMax = point.offset.x;
                    }
                    else if (point.offset.x < xMin)
                    {
                        xMin = point.offset.x;
                    }
                    if (point.offset.y > yMax)
                    {
                        yMax = point.offset.y;
                    }
                    else if (point.offset.y < yMin)
                    {
                        yMin = point.offset.y;
                    }
                    if (point.offset.z > zMax)
                    {
                        zMax = point.offset.z;
                    }
                    else if (point.offset.z < zMin)
                    {
                        zMin = point.offset.z;
                    }
                    points.Add(point);
                }
                else
                {
                    if (point.offset.x > xMax2)
                    {
                        xMax2 = point.offset.x;
                    }
                    else if (point.offset.x < xMin2)
                    {
                        xMin2 = point.offset.x;
                    }
                    if (point.offset.y > yMax2)
                    {
                        yMax2 = point.offset.y;
                    }
                    else if (point.offset.y < yMin2)
                    {
                        yMin2 = point.offset.y;
                    }
                    if (point.offset.z > zMax2)
                    {
                        zMax2 = point.offset.z;
                    }
                    else if (point.offset.z < zMin2)
                    {
                        zMin2 = point.offset.z;
                    }
                    points2.Add(point);
                }
            }).Run();

            ecbSystem.AddJobHandleForProducer(Dependency);

            if (points.Count > 0)
            {
                PointCloud pc1 = PointCloudGenerator.instance.CreateFromOld(oldPc.transform);
                oldPc.GetComponent<GraphSlice>().ClearSlices();
                GraphSlice slice1 = pc1.GetComponent<GraphSlice>();
                slice1.transform.position = pc1.transform.position;
                slice1.sliceCoords = pc1.transform.position;
                slice1.SliceNr = 0;
                slice1.gameObject.name = oldPc.gameObject.name + "_" + slice1.SliceNr;
                slice1.points = points;
                slice1.sliceCoords -= 0.2f * planeNormal;
                float3 max = new float3(xMax, yMax, zMax);
                float3 min = new float3(xMin, yMin, zMin);
                pc1.maxCoordValues = max;
                pc1.minCoordValues = min;
                pc1.SetCollider(true);

                PointCloud pc2 = PointCloudGenerator.instance.CreateFromOld(oldPc.transform);
                GraphSlice slice2 = pc2.GetComponent<GraphSlice>();
                slice2.transform.position = pc2.transform.position;
                slice2.sliceCoords = pc2.transform.position;
                slice2.SliceNr = 1;
                slice2.gameObject.name = oldPc.gameObject.name + "_" + slice2.SliceNr;
                slice2.points = points2;
                slice2.sliceCoords += 0.2f * planeNormal;
                max = new Vector3(xMax2, yMax2, zMax2);
                min = new Vector3(xMin2, yMin2, zMin2);
                pc2.maxCoordValues = max;
                pc2.minCoordValues = min;
                pc2.SetCollider(true);

                //parentSlice.childSlices.Add(slice1);
                //parentSlice.childSlices.Add(slice2);
                quadrantSystem.graphParentTransforms.Add(pc1.transform);
                quadrantSystem.graphParentTransforms.Add(pc2.transform);
                PointCloudGenerator.instance.BuildSlices(oldPc, new GraphSlice[] { slice1, slice2 });
            }
            //parentSlice.slicerBox.sliceAnimationActive = false;
            //parentSlice.ActivateSlice(true);
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
            // sliceSystem.slicing = true;
            //if (axis == 0)
            //{
            //    // slice x
            //}
            //else if (axis == 1)
            //{
            //    // slice y
            //}
            //else
            //{
            //    // slice z the default slice axis

            //    // Get the different z positions (slice positions) and place the slicer in each.
            //    PointCloud pc = PointCloudGenerator.instance.pointClouds[graphID];
            //    List<Point> sortedPoints = new List<Point>(points.Count);
            //    if (axis == 0)
            //    {
            //        if (sortedPointsX == null)
            //        {
            //            sortedPointsX = SortPoints(points, 0);
            //        }

            //        sortedPoints = sortedPointsX;
            //    }

            //    else if (axis == 1)
            //    {
            //        if (sortedPointsY == null)
            //        {
            //            sortedPointsY = SortPoints(points, 1);
            //        }

            //        sortedPoints = sortedPointsY;
            //    }
            //    else if (axis == 2)
            //    {
            //        if (sortedPointsZ == null)
            //        {
            //            sortedPointsZ = SortPoints(points, 2);
            //        }

            //        sortedPoints = sortedPointsZ;
            //    }
            //}


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