using System;
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
    story: "Run a position query around [Target] and write it to [position]",
    category: "Action",
    id: "e0cbb9bd53ae037921a108813d778fdf")]
internal class RunPositionQueryAction : Action
{

    [SerializeReference] public BlackboardVariable<GameObject> origin;
    [SerializeReference] public BlackboardVariable<KinematicCharacterController> target;
    [SerializeReference] public BlackboardVariable<Transform> position;
    [SerializeReference] public BlackboardVariable<float> radius; 
    [SerializeReference] public BlackboardVariable<PositionQueryType> queryType; 
    
    protected override Status OnStart()
    {
        switch (queryType.Value)
        {
            default:
            case PositionQueryType.ClosestPoint:
                position.Value.position = NavigationHelpers.GetClosestPointAroundRadius(origin.Value.transform.position, target.Value.transform.position, radius.Value, 8);
                break;
            case PositionQueryType.AlongVector:
                position.Value.position = NavigationHelpers.GetPointInRadiusByDirection(target.Value.transform.position, -target.Value.lookDirection, radius.Value, 8);
                break;
        }
        return Status.Success;
    }
}

