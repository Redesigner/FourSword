using UnityEngine;

namespace Characters.Enemies.Scripts
{
    [Icon("Assets/Editor/Icons/ShieldIcon.png")]
    public class EnemyShieldComponent : MonoBehaviour
    {
        [SerializeField] private HitboxTrigger armor; 
        
        public void SetLookDirection(float direction)
        {
            var rotation = transform.rotation.eulerAngles;
            rotation.z = Shared.Math.RoundTo(direction * Mathf.Rad2Deg, 90);
            transform.rotation = Quaternion.Euler(rotation);
        }

        public void RaiseShield()
        {
            if (!armor)
            {
                return;
            }
            
            armor.Enable();
        }

        public void LowerShield()
        {
            if (!armor)
            {
                return;
            }
            
            armor.Disable();
        }
    }
}