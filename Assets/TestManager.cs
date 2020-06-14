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

    private World w1;
    private World w2;

    private HybridRendererSystem s1;
    private HybridRendererSystem s2;

    void Start()
    {
        var tuple1 = CreateWorld("World 1");
        var tuple2 = CreateWorld("World 2");

        w1 = tuple1.Item1;
        w2 = tuple2.Item1;

        s1 = tuple1.Item2;
        s2 = tuple2.Item2;
    }

    private (World, HybridRendererSystem) CreateWorld(string n)
    {
        var world = new World(n);

        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);

        var list = systems.ToList();

        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, list);


        SetStuff(world);
        var hybrid = world.GetOrCreateSystem<HybridRendererSystem>();

        return (world, hybrid);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            s1.Enabled = false;
            s2.Enabled = true;
            Display();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            s2.Enabled = false;
            s1.Enabled = true;
            Display();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            s2.Enabled = _switch;
            s1.Enabled = _switch;
            _switch = !_switch;
            Display();
        }
    }

    private bool _switch;

    private void Display()
    {
        Debug.Log($"HybridRenderer of World 1 is {(s1.Enabled ? "" : "not")} enabled");
        Debug.Log($"HybridRenderer of World 2 is {(s2.Enabled ? "" : "not")} enabled");
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