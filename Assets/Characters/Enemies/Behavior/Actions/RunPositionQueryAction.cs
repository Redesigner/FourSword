using System;
using Characters.Enemies.Behavior.Queries;
using Characters.Enemies.Scripts;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// @TODO: more flexible system? I don't really want an enum switch here
enum PositionQueryType
{
    ClosestPoint,
    AlongVector
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "RunPositionQuery",
    story: "Run a query [Query] around [Target] and write it to [position]",
    category: "Action",
    id: "e0cbb9bd53ae037921a108813d778fdf")]
internal class RunPositionQueryAction : Action
{

    [SerializeReference] public BlackboardVariable<GameObject> origin;
    [SerializeReference] public BlackboardVariable<KinematicCharacterController> target;
    [SerializeReference] public BlackboardVariable<PositionQuery> query;
    [SerializeReference] public BlackboardVariable<Transform> position;

    
    protected override Status OnStart()
    {
#if !UNITY_EDITOR
        position.Value.position = query.Value.RunQuery(origin.Value, target.Value);
#else
        position.Value.position = query.Value.RunQueryWithAllResults(origin.Value, target.Value, out var results);
        
        var pathfindingComponent = GameObject.GetComponent<EnemyPathfindingComponent>();
        if (pathfindingComponent)
        {
            pathfindingComponent.recentlyQueuedPoints = results;
        }
#endif
        return Status.Success;
    }
}

