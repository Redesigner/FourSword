using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Enemies.Scripts
{
    public class ExplosionComponent : MonoBehaviour
    {
        [SerializeField] private float explosionRadius = 1.0f;
        [SerializeField] private float explosionDamage = 2.0f;
        [SerializeField] private float fuseTime = 1.0f;
        [SerializeField] private ContactFilter2D explosionContactFilter;

        public UnityEvent onExplode;

        private bool _isCountingDown = false;
        private Animator _animator;
        private TimerHandle _countdownTimer;
        
        private static readonly int CountdownHash = Animator.StringToHash("Countdown");
        private static readonly int ExplodeHash = Animator.StringToHash("Explode");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void StartCountDown()
        {
            if (_isCountingDown)
            {
                return;
            }

            _isCountingDown = true;
            if (_animator)
            {
                _animator.SetTrigger(CountdownHash);
            }
            TimerManager.instance.CreateOrResetTimer(ref _countdownTimer, this, fuseTime, Explode);
            
        }

        private void Explode()
        {
            onExplode.Invoke();

            var results = new List<Collider2D>();
            Physics2D.OverlapCircle(transform.position, explosionRadius, explosionContactFilter, results);
            
            foreach(var hitCollider in results)
            {
                var hitTarget = hitCollider.transform.root.GetComponent<HealthComponent>();
                if (!hitTarget)
                {
                    continue;
                }
                
                hitTarget.TakeDamage(explosionDamage, gameObject);
            }

            _isCountingDown = false;
            if (_animator)
            {
                _animator.SetTrigger(ExplodeHash);
            }
        }

        private void OnDrawGizmos()
        {
            DebugHelpers.Drawing.DrawCircle(transform.position, explosionRadius,
                _isCountingDown ? new Color(1.0f, 0.1f, 0.5f, 0.4f) : new Color(0.2f, 0.1f, 0.8f, 0.4f));
        }
    }
}