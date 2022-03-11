using CellexalVR.AnalysisLogic;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public struct MovePointComponent : IComponentData
{
    public int xindex;
    public int yindex;
    public Vector3 targetPosition;
    public Vector3 currentPosition;
}

public class MoveParticleHandler : SystemBase
{
    private EndSimulationEntityCommandBufferSystem ecbSystem;
    private EntityCommandBuffer ecb;

    protected override void OnCreate()
    {
        base.OnCreate();
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        Texture2D positionMap = PointCloudGenerator.instance.pointClouds[0].positionTextureMap;
        Texture2D orgPositionMap = PointCloudGenerator.instance.pointClouds[0].orgPositionTextureMap;
        ecb = ecbSystem.CreateCommandBuffer();
        Entities.WithAll<MovePointComponent>().ForEach((Entity e, int entityInQueryIndex, ref MovePointComponent mp) =>
        {
            mp.currentPosition = Vector3.MoveTowards(mp.currentPosition, mp.targetPosition, 0.01f);
            positionMap.SetPixel(mp.xindex, mp.yindex, new Color(mp.currentPosition.x, mp.currentPosition.y, mp.currentPosition.z));
            if (Vector3.Distance(mp.targetPosition, mp.currentPosition) < 0.01f)
            {
                mp.currentPosition = mp.targetPosition;
                ecb.DestroyEntity(e);
            }
        }).WithoutBurst().Run();
        positionMap.Apply();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }

    private float Distance(float3 p1, float3 p2)
    {
        return math.sqrt(math.pow((p2.x - p1.x), 2) + math.pow((p2.y - p1.y), 2) + math.pow((p2.z - p1.z), 2));
    }
}
