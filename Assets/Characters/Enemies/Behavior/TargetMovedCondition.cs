using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[Condition(
    name: "Target Moved",
    story: "[Target] has moved [distance]",
    category: "Conditions",
    id: "f774fb41bed7e141d3d26d7d96b4253a")]
public class TargetMovedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> target;
    [SerializeReference] public BlackboardVariable<float> distance;

    [CreateProperty] private Vector3 _targetStartingPosition;
    [CreateProperty] private float _maxDistanceSquared;

    public override bool IsTrue()
    {
        return (_targetStartingPosition - target.Value.position).sqrMagnitude > _maxDistanceSquared;
    }

    public override void OnStart()
    {
        _targetStartingPosition = target.Value.position;
        _maxDistanceSquared = distance * distance;
    }

    public override void OnEnd()
    {
    }
}
