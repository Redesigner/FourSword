using Characters.Enemies.Scripts;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Countdown", story: "[explosion] starts bomb countdown", category: "Action", id: "d08d11625436bb95d0de73fab73176ee")]
public partial class CountdownAction : Action
{
    [SerializeReference] public BlackboardVariable<ExplosionComponent> explosion;

    protected override Status OnStart()
    {
        if (explosion.Value == null)
        {
            return Status.Failure;
        }
        
        explosion.Value.StartCountDown();
        return Status.Success;
    }
}

