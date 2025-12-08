using Characters.Enemies.Scripts;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LowerShield", story: "Lower [shield]", category: "Action/FourSword", id: "f076d142b834042b1274a7a48df45a91")]
public partial class LowerShieldAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyShieldComponent> shield;

    protected override Status OnStart()
    {
        if (!shield.Value)
        {
            return Status.Failure;
        }
        
        shield.Value.LowerShield();
        return Status.Success;
    }
}

