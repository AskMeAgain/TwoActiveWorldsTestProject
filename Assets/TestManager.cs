using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

public class TestManager : MonoBehaviour
{
    static MethodInfo insertManagerIntoSubsystemListMethod = typeof(ScriptBehaviourUpdateOrder).GetMethod(
        "InsertManagerIntoSubsystemList", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

    void Start()
    {
        CreateWorld("World 1");
        CreateWorld("World 2");
    }

    private void CreateWorld(string n)
    {
        var world = new World(n);

        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);

        var list = systems.ToList();

        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, list);

        SetStuff(world);

    }

    private void SetStuff(World world)
    {
        var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

        for (int i = 0; i < playerLoop.subSystemList.Length; i++)
        {
            ComponentSystemGroup mgr;

            var group = playerLoop.subSystemList[i];

            if (@group.type == typeof(FixedUpdate))
            {
                mgr = world.GetOrCreateSystem<SimulationSystemGroup>();
            }
            else if (@group.type == typeof(PreLateUpdate))
            {
                mgr = world.GetOrCreateSystem<PresentationSystemGroup>();
            }
            else if (@group.type == typeof(Initialization))
            {
                mgr = world.GetOrCreateSystem<InitializationSystemGroup>();
            }
            else
            {
                continue;
            }

            var newSubsystemList = new PlayerLoopSystem[@group.subSystemList.Length + 1];

            for (var ii = 0; ii < @group.subSystemList.Length; ++ii)
            {
                newSubsystemList[ii] = @group.subSystemList[ii];
            }

            insertManagerIntoSubsystemListMethod.MakeGenericMethod(mgr.GetType())
                .Invoke(null, new object[] {newSubsystemList, @group.subSystemList.Length + 0, mgr});

            playerLoop.subSystemList[i].subSystemList = newSubsystemList;
        }

        PlayerLoop.SetPlayerLoop(playerLoop);
    }
}