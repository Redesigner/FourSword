using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "LookAtAction",
    story: "[Agent] looks at [Target]",
    category: "Action/FourSword",
    id: "40419517b30d00fb07be5845ccf8974e")]
public partial class LookAtAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> agent;
    [SerializeReference] public BlackboardVariable<Transform> target;

    protected override Status OnStart()
    {
        var kinematicCharacterController = agent.Value.GetComponent<KinematicCharacterController>();

        if (!kinematicCharacterController)
        {
            return Status.Failure;
        }
        
        kinematicCharacterController.SetLookDirection(target.Value.position - agent.Value.transform.position);
        return Status.Success;
    }
}

