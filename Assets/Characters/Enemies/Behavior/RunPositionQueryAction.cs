using System;
using Characters.Enemies.Scripts;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "RunPositionQuery",
    story: "Run a position query around [Target] and write it to [position]",
    category: "Action",
    id: "e0cbb9bd53ae037921a108813d778fdf")]
internal partial class RunPositionQueryAction : Action
{

    [SerializeReference] public BlackboardVariable<GameObject> origin;
    [SerializeReference] public BlackboardVariable<GameObject> target;
    [SerializeReference] public BlackboardVariable<Transform> position;
    [SerializeReference] public BlackboardVariable<float> radius; 
    
    protected override Status OnStart()
    {
        position.Value.position = NavigationHelpers.GetClosestPointAroundRadius(origin.Value.transform.position, target.Value.transform.position, radius.Value, 8);
        return Status.Success;
    }
}

