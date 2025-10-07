using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters
{
    public class AttackController : MonoBehaviour
    {
        protected readonly List<HealthComponent> targetsHit = new();

        protected void OnHitboxOverlapped(Collider2D hitbox, Collider2D hitboxOther)
        {
            // Don't attack ourselves
            
            if (hitboxOther.transform.root.gameObject == gameObject.transform.root.gameObject)
            {
                return;
            }

            if (hitboxOther.gameObject.layer == 8)
            {
                KnockbackPlayer(hitbox);
                return;
            }
            
            Debug.LogFormat("'{0}' attacked '{1}'", gameObject.transform.root.gameObject.name, hitboxOther.gameObject.name);
            var enemyHealth = hitboxOther.transform.root.GetComponent<HealthComponent>();
            if (!enemyHealth)
            {
                return;
            }

            if (targetsHit.Contains(enemyHealth))
            {
                return;
            }

            var result = new List<Collider2D>();
            // Check if we've overlapped *any* armor hitboxes with this hitbox
            if (hitbox.Overlap(result) > 0)
            {
                if (result.Any(overlappedHitbox => overlappedHitbox.gameObject.layer == 8))
                {
                    KnockbackPlayer(hitbox);
                    return;
                }
            }
        
            targetsHit.Add(enemyHealth);
            enemyHealth.TakeDamage(1.0f, gameObject);
        }
        
        private void KnockbackPlayer(Collider2D selfHitbox)
        {
            transform.root.GetComponent<KinematicCharacterController>().Knockback(
                (Vector2)(selfHitbox.transform.position - transform.position).normalized * -5.0f, 0.1f);
        }
    }
}