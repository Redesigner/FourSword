using UnityEngine;

namespace Characters.Enemies.Scripts
{
    [Icon("Assets/Editor/Icons/ShieldIcon.png")]
    public class EnemyShieldComponent : MonoBehaviour
    {
        [SerializeField] private HitboxTrigger armor;

        private bool _shieldRaised = true;
        private bool _shieldForcedDown = false;
        
        public void SetLookDirection(float direction)
        {
            var rotation = transform.rotation.eulerAngles;
            rotation.z = Shared.Math.RoundTo(direction * Mathf.Rad2Deg, 90);
            transform.rotation = Quaternion.Euler(rotation);
        }

        public void RaiseShield()
        {
            _shieldRaised = true;
            if (!armor)
            {
                return;
            }

            if (IsShieldActive())
            {
                armor.Enable();
            }
        }

        public void LowerShield()
        {
            _shieldRaised = false;
            if (!armor)
            {
                return;
            }
            
            armor.Disable();
        }

        private bool IsShieldActive()
        {
            return !_shieldForcedDown && _shieldRaised;
        }

        public void DisableShield(float time)
        {
            _shieldForcedDown = true;
            armor.Disable();
            
            TimerManager.instance.CreateTimer(this, time, () =>
            {
                _shieldForcedDown = false;
                if (IsShieldActive() && armor)
                {
                    armor.Enable();
                }
            });
        }
    }
}