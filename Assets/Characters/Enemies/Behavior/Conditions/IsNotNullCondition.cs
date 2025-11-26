using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is not Null", story: "[Variable] is not Null", category: "Variable Conditions", id: "984bbf4a401d7e7aacb232b304976803")]
public partial class IsNotNullCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> variable;

    public override bool IsTrue()
    {
        return variable.Value;
    }

}
