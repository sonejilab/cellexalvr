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
            if (slicer == null)
            {
                slicer = GameObject.Find("SlicePlane");
            }

            EntityManager.DestroyEntity(GetEntityQuery(typeof(RemoveEntityTagComponent)));
            if (Input.GetKeyDown(KeyCode.K))
            {
                // Slice(0, slicer.forward, slicer.position);
                SliceAxis(2, 0);
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                Slice(0, slicer.transform.forward, slicer.transform.position);
            }
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
                    points.Add(point);
                }
                else
                {
                    //points2[point.label] = point.offset;
                    points2.Add(point);
                }
            }).Run();

            ecbSystem.AddJobHandleForProducer(Dependency);

            if (points.Count > 0)
            {
                //PointCloudGenerator.instance.nrOfGraphs--;
                PointCloud pc1 = PointCloudGenerator.instance.CreateFromOld(oldPc.transform);
                GraphSlice slice1 = pc1.GetComponent<GraphSlice>();
                slice1.transform.position = pc1.transform.position;
                slice1.sliceCoords = pc1.transform.position;
                slice1.SliceNr = 0;
                slice1.gameObject.name = oldPc.gameObject.name + "_" + slice1.SliceNr;
                slice1.points = points;
                slice1.sliceCoords -= 0.2f * planeNormal;

                PointCloud pc2 = PointCloudGenerator.instance.CreateFromOld(oldPc.transform);
                GraphSlice slice2 = pc2.GetComponent<GraphSlice>();
                slice2.transform.position = pc2.transform.position;
                slice2.sliceCoords = pc2.transform.position;
                slice2.SliceNr = 1;
                slice2.gameObject.name = oldPc.gameObject.name + "_" + slice2.SliceNr;
                slice2.points = points2;
                slice2.sliceCoords += 0.2f * planeNormal;

                parentSlice.childSlices.Add(slice1);
                parentSlice.childSlices.Add(slice2);
                PointCloudGenerator.instance.BuildSlices(oldPc);
                quadrantSystem.graphParentTransforms.Add(pc1.transform);
                quadrantSystem.graphParentTransforms.Add(pc2.transform);
            }

            move.Dispose();
        }

        private void SliceAxis(int axis, int graphID)
        {
            List<Point> points = new List<Point>();
            Entities.WithoutBurst().WithAll<Point>().ForEach((Entity entity, int entityInQueryIndex, ref Point point) =>
            {
                if (point.parentID != graphID) return;
                points.Add(point);
            }).Run();

            var sliceSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<SliceGraphSystem>();
            // sliceSystem.slicing = true;
            if (axis == 0)
            {
                // slice x
            }
            else if (axis == 1)
            {
                // slice y
            }
            else
            {
                // slice z the default slice axis

                // Get the different z positions (slice positions) and place the slicer in each.
                PointCloud pc = PointCloudGenerator.instance.pointClouds[graphID];
                List<Vector3> cutPositions = new List<Vector3>();
                List<Point> sortedPoints = new List<Point>(points.Count);
                if (axis == 0)
                {
                    if (sortedPointsX == null)
                    {
                        sortedPointsX = SortPoints(points, 0);
                    }

                    sortedPoints = sortedPointsX;
                }

                else if (axis == 1)
                {
                    if (sortedPointsY == null)
                    {
                        sortedPointsY = SortPoints(points, 1);
                    }

                    sortedPoints = sortedPointsY;
                }
                else if (axis == 2)
                {
                    if (sortedPointsZ == null)
                    {
                        sortedPointsZ = SortPoints(points, 2);
                    }

                    sortedPoints = sortedPointsZ;
                }

                int sliceNr = 0;
                PointCloud newPc = PointCloudGenerator.instance.CreateFromOld(pc.transform);
                GraphSlice slice = newPc.GetComponent<GraphSlice>();
                slice.transform.position = pc.transform.position;
                slice.sliceCoords = pc.transform.position;
                slice.SliceNr = ++sliceNr;
                slice.gameObject.name = "Slice" + sliceNr;
                GraphSlice parentSlice = pc.GetComponent<GraphSlice>();
                slice.SliceNr = parentSlice.SliceNr;
                parentSlice.childSlices.Add(slice);
                slice.gameObject.name = pc.gameObject.name + "_" + parentSlice.SliceNr;

                float currentCoord, diff, prevCoord;
                Point point = sortedPoints[0];
                float firstCoord = prevCoord = point.offset[axis];
                float lastCoord = sortedPoints[sortedPoints.Count - 1].offset[axis];
                float dividers = 20f;
                float epsilonToUse = math.abs(firstCoord - lastCoord) / (float)dividers;

                if (axis == 2)
                {
                    epsilonToUse = 0.01f;
                }

                for (int i = 1; i < sortedPoints.Count; i++)
                {
                    point = sortedPoints[i];
                    currentCoord = point.offset[axis];
                    // when we reach new slice (new x/y/z coordinate) build the graph and then start adding to a new one.
                    diff = math.abs(currentCoord - firstCoord);

                    if (diff > epsilonToUse) // || Math.Abs(currentCoord - prevCoord) > 0.1f)
                    {
                        cutPositions.Add(point.offset);
                        newPc = PointCloudGenerator.instance.CreateFromOld(pc.transform);
                        slice = newPc.GetComponent<GraphSlice>();
                        slice.transform.position = pc.transform.position;
                        slice.sliceCoords = pc.transform.position;
                        slice.SliceNr = ++sliceNr;
                        slice.gameObject.name = pc.gameObject.name + "_" + sliceNr;
                        parentSlice.childSlices.Add(slice);
                        firstCoord = currentCoord;
                    }

                    else if (i == sortedPoints.Count - 1)
                    {
                        parentSlice.childSlices.Add(slice);
                    }

                    slice.points.Add(point);
                    prevCoord = currentCoord;
                }

                parentSlice.childSlices.ForEach(s => s.sliceCoords[axis] = -0.5f + s.SliceNr * (1f / (parentSlice.childSlices.Count - 1)));
                PointCloudGenerator.instance.BuildSlices(pc.transform);


            }
        }


        private static List<Point> SortPoints(IReadOnlyCollection<Point> points, int axis)
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