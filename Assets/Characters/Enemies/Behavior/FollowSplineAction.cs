using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FollowSpline", story: "[Agent] follows [Spline]", category: "Action/Navigation2D", id: "3c4f0d3a51935397b15f60cc2cc5be94")]
public partial class FollowSplineAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyPathfindingComponent> Agent;
    [SerializeReference] public BlackboardVariable<Spline2DComponent> Spline;

    protected override Status OnStart()
    {
        if (!Agent.Value || !Spline.Value)
        {
            return Status.Failure;
        }

        Agent.Value.MoveToSpline(Spline.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Agent.Value.pathFollowingMode == EnemyPathfindingComponent.PathFollowingMode.Spline ? Status.Running : Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

