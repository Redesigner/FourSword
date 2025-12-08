using Characters.Enemies.Scripts;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RaiseShield", story: "Raise [shield]", category: "Action/FourSword", id: "b3c1b2531d35692e26496077600beb57")]
public partial class RaiseShieldAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyShieldComponent> shield;

    protected override Status OnStart()
    {
        if (!shield.Value)
        {
            return Status.Failure;
        }
        
        shield.Value.RaiseShield();
        return Status.Success;
    }
}

