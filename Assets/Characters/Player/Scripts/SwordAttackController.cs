using System;
using System.Collections.Generic;
using Shared;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Math = Shared.Math;

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

        [Header("Stamina")]
        [SerializeField] private float staminaRegenRate = 1.0f;
        [field: SerializeField] [Min(0.0f)] public float stamina { private set; get; }
        [field: SerializeField] [Min(0.0f)] public float maxStamina { private set; get; }
        
        [SerializeField] [Min(0.0f)] private float blockCost = 1.0f;
        [SerializeField] [Min(0.0f)] private float stabCost = 1.0f;
        [SerializeField] [Min(0.0f)] private float slashCost = 2.0f;
        [SerializeField] [Min(0.0f)] private float slamCost = 3.0f;
        
        // Map transitions where a tuple containing our current stance, and the command received
        // is the Key, and the new stance is the Value
        private Dictionary<Tuple<SwordStance, SwordCommand>, SwordStance> _transitions;

        private float _hitboxOffset;
        public TimerHandle diagonalHitboxTimer;
        public TimerHandle secondaryHitboxTimer;
        private TimerHandle _transitionTimer;

        private SwordDirection _pendingDirection;

        private SwordStance _idle;
        private SwordStance _attacking;
        private SwordStance _weakAttack;
        private SwordStance _blocking;
        private SwordStance _countering;

        private SwordStance _currentStance;
        
        public List<HealthComponent> blockedEnemies { private set; get; } = new();

        
        // ANIMATION
        [SerializeField] private Animator animator;
        private static readonly int SwordDirectionHash = Animator.StringToHash("SwordDirection");
        private static readonly int PreviousDirectionHash = Animator.StringToHash("PreviousDirection");
        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
        private static readonly int CancelTriggerHash = Animator.StringToHash("Cancel");
        private static readonly int Blocking = Animator.StringToHash("Blocking");
        private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");
        

        private void Start()
        {
            _idle = new IdleStance
            {
                name = "Idle",
                hitboxType = HitboxType.Hitbox
            };
            _attacking = new AttackStance
            {
                name = "Attacking",
                hitboxType = HitboxType.Hitbox,
                canChangeDirection = true,
                transitionTime = 0.25f,
                costFunction = GetAttackCost
            };
            _weakAttack = new AttackStance
            {
                name = "WeakAttack",
                hitboxType = HitboxType.Hitbox,
                canChangeDirection = true,
                transitionTime = 0.5f,
                attackAnimationSpeed = 0.5f,
            };
            _blocking = new BlockStance()
            {
                name = "Blocking",
                hitboxType = HitboxType.Armor,
                costFunction = _ => blockCost
            };
            _countering = new CounterStance
            {
                name = "Countering",
                hitboxType = HitboxType.Armor,
                canChangeDirection = true,
                transitionTime = 1.5f
            };
            
            _currentStance = _idle;
            // _idle.Enter(this);
            
            _transitions = new Dictionary<Tuple<SwordStance, SwordCommand>, SwordStance>
            {
                { new Tuple<SwordStance, SwordCommand>(_idle, SwordCommand.Press), _attacking },        // Idle -> attacking
                { new Tuple<SwordStance, SwordCommand>(_idle, SwordCommand.CostFailed), _weakAttack },  // Idle -> weak attack
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.Release), _idle },      // Attacking -> Idle
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.CostFailed), _idle },   // Attacking -> Idle (When stamina cost is too high)
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.Expire), _blocking },   // Attacking -> Block
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.Press), _attacking },   // Self transition
                { new Tuple<SwordStance, SwordCommand>(_blocking, SwordCommand.Release), _idle },       // Blocking -> Idle
                { new Tuple<SwordStance, SwordCommand>(_blocking, SwordCommand.Hit), _countering },     // Blocking -> Countering
                { new Tuple<SwordStance, SwordCommand>(_countering, SwordCommand.Expire), _idle },      // Countering -> Idle
                { new Tuple<SwordStance, SwordCommand>(_weakAttack, SwordCommand.Expire), _idle }       // Weak Attack -> Idle
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

        // ReSharper disable Unity.PerformanceAnalysis
        private void Command(SwordCommand command)
        {
            if (!_transitions.TryGetValue(new Tuple<SwordStance, SwordCommand>(_currentStance, command), out var newStance))
            {
                return;
            }

            var staminaCost = newStance.costFunction.Invoke(_pendingDirection);
            if (staminaCost > stamina)
            {
                // ReSharper disable once TailRecursiveCall
                Command(SwordCommand.CostFailed);
                return;
            }

            stamina -= staminaCost;
            // Debug.LogFormat("Transitioning '{0}' => '{1}' Command '{2}'", _currentStance.name, newStance.name, command.ToString());
            _currentStance.Exit(this);
            _currentStance = newStance;
            _currentStance.Enter(this);
            animator.SetFloat(AttackSpeed, _currentStance.attackAnimationSpeed);
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

        private void Update()
        {
            stamina = System.Math.Clamp(stamina + staminaRegenRate * Time.deltaTime, 0.0f, maxStamina);
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
                _pendingDirection = direction;
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

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Transition"))
            {
                animator.SetTrigger(CancelTriggerHash);
            }

            animator.SetTrigger(AttackTriggerHash);
            animator.SetFloat(PreviousDirectionHash, (int)oldDirection);
            animator.SetFloat(SwordDirectionHash, (int)direction);
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

        public override bool BlockedEnemyAttack(DamageType damageType, Collider2D selfArmorHitbox, Collider2D attackerHitbox)
        {
            if (attackerHitbox.gameObject.CompareTag("Projectile"))
            {
                var projectile = attackerHitbox.GetComponent<ProjectileComponent>();
                if (projectile && projectile.CanBeBlocked())
                {
                    return true;
                }
            }
            
            Command(SwordCommand.Hit);
            var enemyHealth = attackerHitbox.transform.root.GetComponent<HealthComponent>();
            if (!enemyHealth)
            {
                return true;
            }

            if (blockedEnemies.Contains(enemyHealth))
            {
                return true;
            }
            
            blockedEnemies.Add(enemyHealth);
            // enemyHealth.Stun(1.0f, this);
            return true;
        }

        protected override void DealDamage(DamageListener enemy)
        {
            base.DealDamage(enemy);
            enemy.Stun(0.2f, this);

            var kinematicCharacterController = enemy.GetComponent<KinematicCharacterController>();
            if (kinematicCharacterController)
            {
                kinematicCharacterController.Knockback((gameObject.transform.position - enemy.transform.position).normalized * -5.0f, 0.25f);
            }
        }

        private void OnDrawGizmos()
        {
            if (_currentStance == null)
            {
                return;
            }
            
            var style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = false;
            Handles.Label(transform.position + new Vector3(0.0f, -0.5f, 0.0f),
                $"{_currentStance.name}\nStamina: {stamina:0.0}/{maxStamina}", style);
        }

        public void SetBlockingAnimator(bool isBlocking)
        {
            animator.SetBool(Blocking, isBlocking);
        }

        private float GetAttackCost(SwordDirection newDirection)
        {
            var directionalChange = Mathf.Abs(GetSwordDirectionDelta(swordDirection, newDirection));
            return directionalChange switch
            {
                0 => stabCost,
                1 => slashCost,
                2 => slamCost,
                _ => 0.0f
            };
        }
    }
}