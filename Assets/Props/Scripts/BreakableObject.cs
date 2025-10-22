using Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Props.Scripts
{
    public class BreakableObject : DamageListener
    {
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] [Min(0.0f)] private float particleLifetime;
        
        public override void TakeDamage(float damage, GameObject source, DamageType damageType = DamageType.Raw)
        {
            if (damageType != DamageType.Slashing)
            {
                return;
            }
            
            Destroy(gameObject);
            if (!particlePrefab)
            {
                return;
            }

            var particleObject = Instantiate(particlePrefab);
            particleObject.transform.position = transform.position;
            TimerManager.instance.CreateTimer(particleObject, particleLifetime, () =>
            {
                Destroy(particleObject);
            });
        }
    }
}