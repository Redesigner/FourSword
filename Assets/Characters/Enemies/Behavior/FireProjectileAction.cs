using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FireProjectile", story: "[Self] fires [Projectile] at [Enemy]", category: "Action/FourSword", id: "81813bc11c702048994bfc4c6a8920e6")]
public class FireProjectileAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> self;
    [SerializeReference] public BlackboardVariable<GameObject> projectile;
    [SerializeReference] public BlackboardVariable<GameObject> enemy;

    protected override Status OnStart()
    {
        if (self == null || projectile == null || enemy == null)
        {
            Debug.LogWarning("FireProjectileAction failed. One or more blackboard variables were not set");
            return Status.Failure;
        }
        
        if (!projectile.Value)
        {
            return Status.Failure;
        }

        var projectileInstance = UnityEngine.Object.Instantiate(projectile.Value, self.Value.transform.position, Quaternion.identity);
        var projectileComponent = projectileInstance.GetComponent<ProjectileComponent>();
        projectileComponent.Setup(enemy.Value.transform.position, self.Value, 5.0f);
        
        return Status.Success;
    }
    
    protected override void OnEnd()
    {
    }
}

