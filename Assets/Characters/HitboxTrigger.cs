using System;
using Characters.Player.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Characters
{
    public class HitboxTrigger : MonoBehaviour
    {
        private Collider2D _collider;
        private SpriteRenderer _visualization;

        public UnityEvent<Collider2D, Collider2D> hitboxOverlapped;

        private void Start()
        {
            _collider = GetComponent<Collider2D>();
            _visualization = GetComponent<SpriteRenderer>();
        }

        public void SetHitboxStance(SwordStance stance)
        {
            switch (stance)
            {
                case SwordStance.Attacking:
                    _visualization.color = new Color(1.0f, 0.0f, 0.0f, 0.1f);
                    return;
                case SwordStance.Idle:
                    _visualization.color = new Color(0.0f, 1.0f, 0.0f, 0.1f);
                    return;
                case SwordStance.Blocking:
                    _visualization.color = new Color(0.0f, 0.0f, 1.0f, 0.1f);
                    return;
                default:
                    return;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            hitboxOverlapped.Invoke(_collider, other);
        }

        public void Enable()
        {
            _collider.enabled = true;
            _visualization.enabled = true;
        }

        public void Disable()
        {
            // These components might have been destroyed
            if (_collider)
            {
                _collider.enabled = false;
            }

            if (_visualization)
            {
                _visualization.enabled = false;
            }
        }
    }
}