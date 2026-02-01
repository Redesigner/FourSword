using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

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
            var healthComponent = GetComponent<HealthComponent>();
            if (healthComponent)
            {
                healthComponent.onTakeDamage.AddListener(TakeDamage);
            }
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

        public void TakeDamage(GameObject source)
        {
            if (!source)
            {
                return;
            }
            
            if (source.transform.root.CompareTag("Player"))
            {
                Explode();
            }
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

                if (!CanSeeObject(hitCollider.gameObject))
                {
                    continue;
                }
                
                hitTarget.TakeDamage(explosionDamage, null);
            }

            _isCountingDown = false;
            if (_animator)
            {
                _animator.SetTrigger(ExplodeHash);
            }
            _countdownTimer.Reset();
        }
        
        private bool CanSeeObject(GameObject obj)
        {
            var point = (Vector2)obj.transform.position;
            var previousQueryHitTriggerValue = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = false;
            var result = Physics2D.Linecast(transform.position, point, LayerMask.GetMask("Default"));
            Physics2D.queriesHitTriggers = previousQueryHitTriggerValue;
            
            Debug.DrawLine(transform.position, result ? result.centroid : point, Color.red, Time.fixedDeltaTime);
            
            return !result;
        }

        private void OnDrawGizmos()
        {
            var timeRemaining = _countdownTimer.GetRemainingTime();
            Handles.Label(transform.position - new Vector3(0.0f, 0.5f, 0.0f),
                timeRemaining > 0.0f ? $"Timer: {_countdownTimer.GetRemainingTime():0.0}" : "Bomb inactive");
            DebugHelpers.Drawing.DrawCircle(transform.position, explosionRadius,
                _isCountingDown ? new Color(1.0f, 0.1f, 0.5f, 0.4f) : new Color(0.2f, 0.1f, 0.8f, 0.4f));
        }
    }
}