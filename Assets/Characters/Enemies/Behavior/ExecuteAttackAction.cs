using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "ExecuteAttack",
    story: "[agent] executes attack",
    category: "Action/FourSword",
    id: "7088c136be795539257856716116b7f3")]
public class ExecuteAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> agent;
    [SerializeReference] public BlackboardVariable<bool> waitForAttackCompletion;

    [CreateProperty] private bool _completed;
    
    protected override Status OnStart()
    {
        var attackController = agent.Value.GetComponent<EnemyAttackController>();
        if (attackController == null)
        {
            return Status.Failure;
        }
        
        attackController.Attack();
        if (!waitForAttackCompletion.Value)
        {
            return Status.Success;
        }
        
        attackController.attackCompleted.AddListener(OnAttackCompleted);
        _completed = false;
        return Status.Running;

    }

    protected override Status OnUpdate()
    {
        return _completed ? Status.Success : Status.Running;
    }

    public void OnAttackCompleted()
    {
        if (!waitForAttackCompletion.Value)
        {
            return;
        }
        var attackController = agent.Value.GetComponent<EnemyAttackController>();
        attackController.attackCompleted.RemoveListener(OnAttackCompleted);
        _completed = true;
    }
}

