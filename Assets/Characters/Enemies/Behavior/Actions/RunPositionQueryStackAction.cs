using System;
using System.Linq;
using Characters.Enemies.Behavior.Queries;
using Characters.Enemies.Scripts;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "RunPositionQueryStack",
    story: "Run a query stack [Query] and write it to [position]",
    category: "Action",
    id: "b42103bd915d5fd1b2d574b7f2d45bb0")]
internal class RunPositionQueryStackAction : Action
{

    [SerializeReference] public BlackboardVariable<KinematicCharacterController> self;
    [SerializeReference] public BlackboardVariable<KinematicCharacterController> target;
    [SerializeReference] public BlackboardVariable<PositionQueryStack> query;
    [SerializeReference] public BlackboardVariable<Transform> position;

    
    protected override Status OnStart()
    {
        var result = query.Value.Evaluate(self.Value, target.Value);
        if (result.Count == 0)
        {
            return Status.Failure;
        }
        
        result.Sort((a, b) => -a.score.CompareTo(b.score));
        position.Value.position = result.First().position;
        
#if UNITY_EDITOR
        var pathfindingComponent = GameObject.GetComponent<EnemyPathfindingComponent>();
        if (pathfindingComponent)
        {
            pathfindingComponent.recentlyQueuedPoints = result;
        }
#endif
        return Status.Success;
    }
}

