using System;
using System.Linq;
using Shared;
using UnityEngine;

namespace Characters.Player.Scripts
{
    public enum SwordCommand
    {
        Press, // Button pressed
        Release, // Button released
        Expire, // Timer expired
        CostFailed, // Stamina cost was too high
        Hit // Hit something
    }
    
    public class SwordStance
    {
        public HitboxType hitboxType;
        public string name;
        public bool canChangeDirection = false;
        public float transitionTime;
        public Func<SwordDirection, float> costFunction = (SwordDirection _) => 0.0f;
        public float attackAnimationSpeed = 1.0f;

        public virtual void Stab(SwordAttackController controller, SwordDirection direction)
        {
        }
        public virtual void Slash(SwordAttackController controller, SwordDirection start, SwordDirection end)
        {
        }
        
        public virtual void Slam(SwordAttackController controller, SwordDirection direction)
        {
        }

        public virtual void Enter(SwordAttackController controller)
        {
            controller.primaryHitbox.transform.localScale = Vector3.one;
            controller.primaryHitbox.transform.localPosition = controller.GetLocalPositionFromRotation(SwordAttackController.GetRotation(controller.swordDirection));
        }

        public virtual void Exit(SwordAttackController controller)
        {
            
        }
    }

    public class IdleStance : SwordStance
    {
        public override void Enter(SwordAttackController controller)
        {
            controller.primaryHitbox.transform.localPosition = controller.GetLocalPositionFromRotation(SwordAttackController.GetRotation(controller.swordDirection)) * 0.5f;
            controller.primaryHitbox.transform.localScale = new Vector3(1.0f, 0.5f, 1.0f);
        }
    }

    public class AttackStance : SwordStance
    {
        public override void Stab(SwordAttackController controller, SwordDirection direction)
        {
            controller.currentDamageType = DamageType.Piercing;
            var initialHitboxLength = controller.primaryHitbox.GetHitboxSize().x;
            controller.primaryHitbox.transform.localPosition = controller.GetLocalPositionFromRotation(SwordAttackController.GetRotation(direction)) * 1.5f;
            controller.primaryHitbox.transform.localScale = new Vector3(1.0f, 2.0f * controller.stabReachMultiplier, 1.0f);
            controller.primaryHitbox.Disable();
            controller.primaryHitbox.Enable();
            controller.secondaryHitbox.Disable();
            controller.diagonalHitbox.Disable();
        }
        
        public override void Slash(SwordAttackController controller, SwordDirection start, SwordDirection end)
        {
            controller.currentDamageType = DamageType.Slashing;
            controller.primaryHitbox.transform.localScale = Vector3.one;
            controller.primaryHitbox.transform.localPosition = controller.GetLocalPositionFromRotation(SwordAttackController.GetRotation(end));
            controller.primaryHitbox.transform.rotation = Quaternion.Euler(0.0f, 0.0f, SwordAttackController.GetRotation(end) - 90.0f);
            controller.secondaryHitbox.transform.localPosition = controller.GetLocalPositionFromRotation(SwordAttackController.GetRotation(start));
            controller.diagonalHitbox.transform.localPosition = controller.GetLocalPositionFromRotation(SwordAttackController.GetRotation(start, end));
            controller.diagonalHitbox.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, SwordAttackController.GetRotation(start, end) - 90.0f);
            controller.primaryHitbox.Enable();
            controller.secondaryHitbox.Enable();
            controller.diagonalHitbox.Enable();
            
            TimerManager.instance.CreateOrResetTimer(ref controller.diagonalHitboxTimer, controller, 0.2f / attackAnimationSpeed, () => { controller.diagonalHitbox.Disable(); });
            TimerManager.instance.CreateOrResetTimer(ref controller.secondaryHitboxTimer, controller, 0.1f / attackAnimationSpeed, () => { controller.secondaryHitbox.Disable(); });
        }

        public override void Slam(SwordAttackController controller, SwordDirection direction)
        {
            controller.currentDamageType = DamageType.Smash;
            controller.primaryHitbox.transform.localScale = Vector3.one;
            controller.primaryHitbox.transform.localPosition = controller.GetLocalPositionFromRotation(SwordAttackController.GetRotation(direction));
            controller.primaryHitbox.Enable();
            controller.secondaryHitbox.Disable();
            controller.diagonalHitbox.Disable();
        }
    }

    public class CounterStance : SwordStance
    {
        public override void Stab(SwordAttackController controller, SwordDirection direction)
        {
            const float knockbackStrength = 4.0f;
            var knockbackAngle = SwordAttackController.GetRotation(direction) * Mathf.Deg2Rad;
            var knockback = new Vector2(
                Mathf.Cos(knockbackAngle) * knockbackStrength,
                Mathf.Sin(knockbackAngle) * knockbackStrength
            );
            
            foreach (var enemyController in controller.blockedEnemies.Select(enemy => enemy.GetComponent<KinematicCharacterController>()).Where(enemyController => enemyController))
            {
                enemyController.Knockback(knockback, 0.75f);
            }
        }

        public override void Slash(SwordAttackController controller, SwordDirection start, SwordDirection end)
        {
            const float knockbackStrength = 2.0f;
            var knockbackAngle = SwordAttackController.GetRotation(end) * Mathf.Deg2Rad;
            var knockback = new Vector2(
                Mathf.Cos(knockbackAngle) * knockbackStrength,
                Mathf.Sin(knockbackAngle) * knockbackStrength
            );
            
            foreach (var enemyHealth in controller.blockedEnemies)
            {
                var enemyController = enemyHealth.GetComponent<KinematicCharacterController>();
                if (enemyController)
                {
                    enemyController.Knockback(knockback, 0.5f);
                }

                enemyHealth.Stun(2.0f, controller);
            }
        }

        public override void Slam(SwordAttackController controller, SwordDirection direction)
        {
            const float knockbackStrength = 2.0f;
            var knockbackAngle = SwordAttackController.GetRotation(direction) * Mathf.Deg2Rad + Mathf.PI;
            var knockback = new Vector2(
                Mathf.Cos(knockbackAngle) * knockbackStrength,
                Mathf.Sin(knockbackAngle) * knockbackStrength
            );
            
            foreach (var enemyController in controller.blockedEnemies.Select(enemy => enemy.GetComponent<KinematicCharacterController>()).Where(enemyController => enemyController))
            {
                enemyController.Knockback(knockback, 0.5f);
            }
        }
    }

    public class BlockStance : SwordStance
    {
        public override void Enter(SwordAttackController controller)
        {
            base.Enter(controller);
            
            controller.SetBlockingAnimator(true);
        }

        public override void Exit(SwordAttackController controller)
        {
            base.Exit(controller);
            
            controller.SetBlockingAnimator(false);
        }
    }
}