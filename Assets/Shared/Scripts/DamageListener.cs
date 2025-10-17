using UnityEngine;

namespace Shared
{
    public enum DamageType
    {
        Raw,
        Slashing,
        Piercing,
        Blunt
    }
    public abstract class DamageListener : MonoBehaviour
    {
        public virtual void TakeDamage(float damage, GameObject source, DamageType damageType = DamageType.Raw)
        {
        }
    }
}