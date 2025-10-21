using System.Collections.Generic;
using System.Linq;
using Shared;
using UnityEngine;

namespace Characters
{
    public class AttackController : MonoBehaviour
    {
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
                BlockedEnemyAttack(hitbox, otherHitbox);
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

            if (targetsHit.Contains(damagedEnemy))
            {
                return;
            }

            var result = new List<Collider2D>();
            // Check if we've overlapped *any* armor hitboxes with this hitbox
            if (hitbox.Overlap(result) > 0)
            {
                if (result.Any(overlappedHitbox => overlappedHitbox.gameObject.layer == 8))
                {
                    AttackBlocked(hitbox, otherHitbox);
                    return;
                }
            }
        
            targetsHit.Add(damagedEnemy);
            damagedEnemy.TakeDamage(1.0f, gameObject, currentDamageType);
        }
        
        private void KnockbackPlayer(Collider2D selfHitbox)
        {
            transform.root.GetComponent<KinematicCharacterController>().Knockback(
                (Vector2)(selfHitbox.transform.position - transform.position).normalized * -5.0f, 0.1f);
        }

        public virtual void AttackBlocked(Collider2D selfHitbox, Collider2D otherHitbox)
        {
            KnockbackPlayer(selfHitbox);
        }

        public virtual void BlockedEnemyAttack(Collider2D selfArmorHitbox, Collider2D attackerHitbox)
        {
            
        }
    }
}