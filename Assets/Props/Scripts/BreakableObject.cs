using Shared;
using UnityEngine;

namespace Props.Scripts
{
    public class BreakableObject : DamageListener
    {
        public override void TakeDamage(float damage, GameObject source, DamageType damageType = DamageType.Raw)
        {
            if (damageType == DamageType.Slashing)
            {
                Destroy(gameObject);
            }
        }
    }
}