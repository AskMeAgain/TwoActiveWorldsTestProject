using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Serialization;
using Unity.Transforms;
using UnityEngine;

[ExecuteAlways]
public class TestSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        var settings = new GameObjectConversionSettings(EntityManager.World,
            GameObjectConversionUtility.ConversionFlags.AssignName,
            new BlobAssetStore());

        var cube = GameObjectConversionUtility.ConvertGameObjectHierarchy(Resources.Load<GameObject>("cube"), settings);
        EntityManager.SetName(cube, "Prefab:" + EntityManager.World.Name);

        var entity2 = EntityManager.Instantiate(cube);
        EntityManager.SetName(entity2, "Real Cube:" + EntityManager.World.Name);

        if (EntityManager.World.Name.Equals("World 1"))
        {
            EntityManager.SetComponentData(entity2, new Translation() {Value = new float3(3, 0, 0)});
            EntityManager.SetComponentData(cube, new Translation() {Value = new float3(3, 0, 0)});
        }
        else
        {
            EntityManager.SetComponentData(entity2, new Translation() {Value = new float3(0, 0, 0)});
            EntityManager.SetComponentData(cube, new Translation() {Value = new float3(0, 0, 0)});
        }
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation position) =>
        {
            position.Value.y += (float) math.sin(Time.ElapsedTime * 10) * 0.01f;
        });
    }
}