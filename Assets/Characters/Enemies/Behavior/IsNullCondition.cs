using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Null", story: "[Variable] is Null", category: "Variable Conditions", id: "6fcc96cd1e8749614d7a036a0d9d51c7")]
public partial class IsNullCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> variable;

    public override bool IsTrue()
    {
        return !variable.Value;
    }

}
