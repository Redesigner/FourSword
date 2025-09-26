using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Check Distance Vector",
        category: "Conditions",
        story: "distance between [transform] and [target] [op] [threshold]")]
    partial class CheckDistanceVectorCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<Transform> transform;
        [SerializeReference] public BlackboardVariable<Vector3> target;
        [Comparison(comparisonType: ComparisonType.All)]
        [SerializeReference] public BlackboardVariable<ConditionOperator> op;
        [SerializeReference] public BlackboardVariable<float> threshold;

        public override bool IsTrue()
        {
            if (transform.Value == null)
            {
                return false;
            }

            float distance = Vector3.Distance(transform.Value.position, target.Value);
            return ConditionUtils.Evaluate(distance, op, threshold.Value);
        }
    }
}