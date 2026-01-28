using System.Collections.Generic;
using System.Linq;
using Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Characters
{
    [Icon("Assets/Editor/Icons/AttackControllerIcon.png")]
    public class AttackController : MonoBehaviour
    {
        /// <summary>
        /// Should we track which targets we've hit with an attack?
        /// Prevents attacks from hitting multiple times, but
        /// requires the target list to be reset, usually inside
        /// the attack animation
        /// </summary>
        [SerializeField] private bool hitTargetsOnce = true;
        
        protected readonly List<DamageListener> targetsHit = new();

        public DamageType currentDamageType;

        protected void OnHitboxOverlapped(Collider2D hitbox, Collider2D otherHitbox)
        {
            // Don't attack ourselves
            if (otherHitbox.transform.root.gameObject == gameObject.transform.root.gameObject)
            {
                return;
            }

            if (hitbox.gameObject.layer == 8)
            {
                var enemyAttackController = HitboxTrigger.GetOwningObject(hitbox).GetComponentInChildren<AttackController>();
                if (enemyAttackController)
                {
                    BlockedEnemyAttack(enemyAttackController.currentDamageType, hitbox, otherHitbox);
                }
                return;
            }

            if (otherHitbox.gameObject.layer == 8)
            {
                AttackBlocked(hitbox, otherHitbox);
                return;
            }
            
            // Debug.LogFormat("'{0}' attacked '{1}'", gameObject.transform.root.gameObject.name, otherHitbox.gameObject.name);
            var damagedEnemy = otherHitbox.transform.root.GetComponent<DamageListener>();
            if (!damagedEnemy)
            {
                return;
            }
            
            if (hitTargetsOnce && targetsHit.Contains(damagedEnemy))
            {
                return;
            }

            var result = new List<Collider2D>();
            // Check if we've overlapped *any* armor hitboxes with this hitbox
            if (hitbox.Overlap(result) > 0)
            {
                // Ignore the armor if it belongs to us!
                if (result.Any(overlappedHitbox => overlappedHitbox.gameObject.layer == 8 && overlappedHitbox.transform.root != transform.root))
                {
                    AttackBlocked(hitbox, otherHitbox);
                    return;
                }
            }

            if (hitTargetsOnce)
            {
                targetsHit.Add(damagedEnemy);
            }
            
            DealDamage(damagedEnemy);
        }
        
        private void KnockbackPlayer(Collider2D selfHitbox)
        {
            transform.root.GetComponent<KinematicCharacterController>().Knockback(
                (Vector2)(selfHitbox.transform.position - transform.position).normalized * -5.0f, 0.1f);
        }

        public virtual void AttackBlocked(Collider2D selfHitbox, Collider2D otherHitbox)
        {
            var enemyAttackController = HitboxTrigger.GetOwningObject(otherHitbox).GetComponentInChildren<AttackController>();
            if (!enemyAttackController)
            {
                KnockbackPlayer(selfHitbox);
                return;
            }
            
            if (enemyAttackController.BlockedEnemyAttack(currentDamageType, otherHitbox, selfHitbox))
            {
                KnockbackPlayer(selfHitbox);
            }
        }

        public virtual bool BlockedEnemyAttack(DamageType blockedDamageType, Collider2D selfArmorHitbox, Collider2D attackerHitbox)
        {
            return false;
        }

        protected virtual void DealDamage(DamageListener enemy)
        {
            enemy.TakeDamage(1.0f, gameObject, currentDamageType);
        }

        private void AttemptAttackThroughShield(Collider2D hitbox, Collider2D otherHitbox)
        {

        }
    }
}