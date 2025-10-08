using System;
using System.Linq;
using Characters.Player.Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Characters
{
    public enum HitboxType
    {
        None,
        Hitbox,
        Hurtbox,
        Armor
    }
    [RequireComponent(typeof(BoxCollider2D))]
    public class HitboxTrigger : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D hitboxCollider;
        public UnityEvent<Collider2D, Collider2D> hitboxOverlapped;

        private void OnEnable()
        {
            hitboxCollider = GetComponent<BoxCollider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            hitboxOverlapped.Invoke(hitboxCollider, other);
        }

        public void Enable()
        {
            hitboxCollider.enabled = true;
        }

        public void Disable()
        {
            // These components might have been destroyed
            if (hitboxCollider)
            {
                hitboxCollider.enabled = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (!hitboxCollider)
            {
                hitboxCollider = GetComponent<BoxCollider2D>();
            }

            if (!hitboxCollider.isActiveAndEnabled)
            {
                return;
            }
            
            DebugHelpers.Drawing.DrawBoxCollider2D(hitboxCollider, GetHitboxColorForLayer(hitboxCollider.gameObject.layer));
        }

        public HitboxType GetHitboxType()
        {
            return hitboxCollider.gameObject.layer switch
            {
                8 => HitboxType.Armor,
                6 => HitboxType.Hitbox,
                7 => HitboxType.Hurtbox,
                _ => HitboxType.None
            };
        }

        public static int GetLayer(HitboxType type)
        {
            return type switch
            {
                HitboxType.Armor => 8,
                HitboxType.Hitbox => 6,
                HitboxType.Hurtbox => 7,
                HitboxType.None => 0,
                _ => 0
            };
        }

        static Color GetHitboxColorForLayer(int layer)
        {
            return layer switch
            {
                8 => new Color(0.0f, 0.0f, 1.0f, 0.5f),
                6 => new Color(1.0f, 0.0f, 0.0f, 0.5f),
                7 => new Color(0.0f, 1.0f, 0.0f, 0.5f),
                _ => new Color(0.5f, 0.5f, 0.5f, 0.5f)
            };
        }
    }
}