using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class AttackController : MonoBehaviour
    {
        protected List<HealthComponent> _targetsHit = new();
        
        protected void OnHitboxOverlapped(Collider2D hitbox, Collider2D hitboxOther)
        {
            // Don't attack ourselves
            
            Debug.LogFormat("'{0}' attacked '{1}'", gameObject.transform.root.gameObject.name, hitboxOther.gameObject.name);
            if (hitboxOther.gameObject == gameObject.transform.root.gameObject)
            {
                return;
            }
            
            var enemyHealth = hitboxOther.GetComponent<HealthComponent>();
            if (!enemyHealth)
            {
                return;
            }

            if (_targetsHit.Contains(enemyHealth))
            {
                return;
            }
        
            _targetsHit.Add(enemyHealth);
            enemyHealth.TakeDamage(1.0f, gameObject);
        }
    }
}