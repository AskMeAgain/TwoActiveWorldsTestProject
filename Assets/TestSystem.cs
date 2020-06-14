using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Serialization;
using Unity.Transforms;
using UnityEngine;

[ExecuteAlways]
public class TestSystem : SystemBase
{

    protected override void OnCreate()
    {
        Debug.Log("World: " + EntityManager.World.Name);

        var settings = new GameObjectConversionSettings(EntityManager.World,
            GameObjectConversionUtility.ConversionFlags.AssignName,
            new BlobAssetStore());

        var cube = GameObjectConversionUtility.ConvertGameObjectHierarchy(Resources.Load<GameObject>("cube"), settings);
        EntityManager.SetName(cube, "Base:" + EntityManager.World.Name);

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
        Debug.Log("test" + EntityManager.World.Name);
    }
}