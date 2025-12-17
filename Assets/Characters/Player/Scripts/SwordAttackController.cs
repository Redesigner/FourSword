using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player.Scripts
{
    // Same order as unit circle
    public enum SwordDirection
    {
        Right = 0,
        Up = 1,
        Left = 2,
        Down = 3
    }
    
    public class SwordAttackController : AttackController
    {
        [field: SerializeField] public SwordDirection swordDirection { private set; get; }
        [SerializeField] private SpriteRenderer swordSprite;

        [field: SerializeField] public HitboxTrigger primaryHitbox { private set; get; }
        [field: SerializeField] public HitboxTrigger secondaryHitbox { private set; get; }
        [field: SerializeField] public HitboxTrigger diagonalHitbox { private set; get; }

        // Map transitions where a tuple containing our current stance, and the command received
        // is the Key, and the new stance is the Value
        private Dictionary<Tuple<SwordStance, SwordCommand>, SwordStance> _transitions;

        private float _hitboxOffset;
        public TimerHandle diagonalHitboxTimer;
        public TimerHandle secondaryHitboxTimer;
        private TimerHandle _transitionTimer;

        private SwordStance _idle;
        private SwordStance _attacking;
        private SwordStance _blocking;
        private SwordStance _countering;

        private SwordStance _currentStance;
        
        public List<HealthComponent> blockedEnemies { private set; get; } = new();

        
        // ANIMATION
        [SerializeField] private Animator animator;
        private static readonly int SwordDirectionHash = Animator.StringToHash("SwordDirection");
        

        private void Start()
        {
            _idle = new SwordStance
            {
                name = "Idle",
                hitboxType = HitboxType.Hitbox
            };
            _attacking = new AttackStance
            {
                name = "Attacking",
                hitboxType = HitboxType.Hitbox,
                canChangeDirection = true,
                transitionTime = 0.5f
            };
            _blocking = new SwordStance
            {
                name = "Blocking",
                hitboxType = HitboxType.Armor
            };
            _countering = new CounterStance
            {
                name = "Countering",
                hitboxType = HitboxType.Armor,
                canChangeDirection = true,
                transitionTime = 1.5f
            };
            
            _currentStance = _idle;
            
            _transitions = new Dictionary<Tuple<SwordStance, SwordCommand>, SwordStance>
            {
                { new Tuple<SwordStance, SwordCommand>(_idle, SwordCommand.Press), _attacking },        // Idle -> attacking
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.Release), _idle },      // Attacking -> Idle
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.Expire), _blocking },   // Attacking -> Block
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.Press), _attacking },   // Self transition
                { new Tuple<SwordStance, SwordCommand>(_blocking, SwordCommand.Release), _idle },       // Blocking -> Idle
                { new Tuple<SwordStance, SwordCommand>(_blocking, SwordCommand.Hit), _countering },     // Blocking -> Countering
                { new Tuple<SwordStance, SwordCommand>(_countering, SwordCommand.Expire), _idle }       // Countering -> Idle
            };

            _hitboxOffset = primaryHitbox.transform.localPosition.y;
            
            primaryHitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
            secondaryHitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
            diagonalHitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
            
            swordSprite.transform.parent.rotation = Quaternion.Euler(0.0f, 0.0f, GetRotation(Scripts.SwordDirection.Up));
            swordDirection = Scripts.SwordDirection.Up;
            secondaryHitbox.Disable();
            diagonalHitbox.Disable();
        }

        private void Command(SwordCommand command)
        {
            if (!_transitions.TryGetValue(new Tuple<SwordStance, SwordCommand>(_currentStance, command), out var newStance))
            {
                return;
            }
            
            // Debug.LogFormat("Transitioning '{0}' => '{1}' Command '{2}'", _currentStance.name, newStance.name, command.ToString());
            _currentStance = newStance;
            _currentStance.Enter(this);
            primaryHitbox.gameObject.layer = HitboxTrigger.GetLayer(_currentStance.hitboxType);
            if (_currentStance.transitionTime > 0.0f)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                TimerManager.instance.CreateOrResetTimer(ref _transitionTimer, this, _currentStance.transitionTime, () => { Command(SwordCommand.Expire); });
            }
            else
            {
                _transitionTimer.Pause();
            }
        }

        static SwordDirection GetSwordDirectionFromVector(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0.0f ? Scripts.SwordDirection.Right : Scripts.SwordDirection.Left;
            }
            return input.y > 0.0f ? Scripts.SwordDirection.Up : Scripts.SwordDirection.Down;
        }
        
        public static float GetRotation(SwordDirection direction)
        {
            return (int)direction * 90.0f;
        }

        public static float GetRotation(SwordDirection start, SwordDirection end)
        {
            var endRotation = GetRotation(end);
            var delta = GetSwordDirectionDelta(start, end);
            return endRotation - delta * 45.0f;
        }

        private static int GetSwordDirectionDelta(SwordDirection a, SwordDirection b)
        {
            var rawDelta = b - a;
            return rawDelta switch
            {
                > 2 => rawDelta - 4,
                < -2 => rawDelta + 4,
                _ => rawDelta
            };
        }

        static SwordDirection OffsetSwordDirection(SwordDirection direction, int offset)
        {
            var result = (int)direction + offset;
            return result switch
            {
                < 0 => (SwordDirection)result + 4,
                >= 4 => (SwordDirection)result - 4,
                _ => (SwordDirection)result
            };
        }
        

        // These are just hooks for our PlayerInput component to forward.
        // We handle checking the input and other things in SwordDirectInput
        public void OnSwordLeft(InputAction.CallbackContext context) { SwordDirectInput(context, Scripts.SwordDirection.Left); }
        public void OnSwordRight(InputAction.CallbackContext context) { SwordDirectInput(context, Scripts.SwordDirection.Right); }
        public void OnSwordUp(InputAction.CallbackContext context) { SwordDirectInput(context, Scripts.SwordDirection.Up); }
        public void OnSwordDown(InputAction.CallbackContext context) { SwordDirectInput(context, Scripts.SwordDirection.Down); }

        private void SwordDirectInput(InputAction.CallbackContext context, SwordDirection direction)
        {
            if (GameState.instance.paused)
            {
                return;
            }
            
            if (context.performed)
            {
                Command(SwordCommand.Press);
                SetSwordDirection(direction);
                return;
            }

            // Only cancel if the button that was released is the same as our current direction
            if (context.canceled && direction == swordDirection)
            {
                Command(SwordCommand.Release);
            }
        }

        private void SetSwordDirection(SwordDirection direction)
        {
            if (!_currentStance.canChangeDirection)
            {
                return;
            }
            
            var oldDirection = swordDirection;
            swordDirection = direction;
            swordSprite.transform.parent.rotation = Quaternion.Euler(0.0f, 0.0f, GetRotation(direction));
            OnSwordDirectionChanged(oldDirection, swordDirection);
            
            animator.SetInteger(SwordDirectionHash, (int)direction);
        }

        private void OnSwordDirectionChanged(SwordDirection oldDirection, SwordDirection newDirection)
        {
            targetsHit.Clear();
            
            var directionalChange = Mathf.Abs(GetSwordDirectionDelta(oldDirection, newDirection));
            switch (directionalChange)
            {
                case 0:
                    _currentStance.Stab(this, newDirection);
                    break;
                case 1:
                    _currentStance.Slash(this, oldDirection, newDirection);
                    break;
                case 2:
                    _currentStance.Slam(this, newDirection);
                    break;
            }
            blockedEnemies.Clear();
        }
        
        public Vector3 GetLocalPositionFromRotation(float rotationDegrees)
        {
            var rads = Mathf.Deg2Rad * rotationDegrees;
            return new Vector3(Mathf.Cos(rads) * _hitboxOffset, Mathf.Sin(rads) * _hitboxOffset, 0.0f);
        }

        public override void BlockedEnemyAttack(Collider2D selfArmorHitbox, Collider2D attackerHitbox)
        {
            if (attackerHitbox.gameObject.CompareTag("Projectile"))
            {
                var projectile = attackerHitbox.GetComponent<ProjectileComponent>();
                if (projectile && projectile.CanBeBlocked())
                {
                    return;
                }
            }
            
            Command(SwordCommand.Hit);
            var enemyHealth = attackerHitbox.transform.root.GetComponent<HealthComponent>();
            if (!enemyHealth)
            {
                return;
            }

            if (blockedEnemies.Contains(enemyHealth))
            {
                return;
            }
            
            blockedEnemies.Add(enemyHealth);
            // enemyHealth.Stun(1.0f, this);
        }

        private void OnDrawGizmos()
        {
            if (_currentStance == null)
            {
                return;
            }
            
            Handles.Label(transform.position + new Vector3(-0.5f, -0.5f, 0.0f), _currentStance.name);
        }
    }
}