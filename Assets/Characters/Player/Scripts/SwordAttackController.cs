using System;
using System.Collections.Generic;
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
        [SerializeField] private SwordDirection swordDirection;
        [SerializeField] private SpriteRenderer swordSprite;

        [SerializeField] private HitboxTrigger primaryHitbox;
        [SerializeField] private HitboxTrigger secondaryHitbox;
        [SerializeField] private HitboxTrigger diagonalHitbox;

        [SerializeField] [Min(0.0f)] private float counterWindow = 0.5f;

        // Map transitions where a tuple containing our current stance, and the command received
        // is the Key, and the new stance is the Value
        private Dictionary<Tuple<SwordStance, SwordCommand>, SwordStance> _transitions;

        private float _hitboxOffset;
        private TimerHandle _diagonalHitboxTimer;
        private TimerHandle _secondaryHitboxTimer;
        private TimerHandle _transitionTimer;

        private SwordStance _idle;
        private SwordStance _attacking;
        private SwordStance _blocking;
        private SwordStance _countering;

        private SwordStance _currentStance;

        private void Start()
        {
            _idle = new SwordStance
            {
                name = "Idle",
                hitboxType = HitboxType.Hitbox
            };
            _attacking = new SwordStance
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
            _countering = new SwordStance
            {
                name = "Countering",
                hitboxType = HitboxType.Armor,
                transitionTime = 0.5f
            };
            
            _currentStance = _idle;
            
            _transitions = new Dictionary<Tuple<SwordStance, SwordCommand>, SwordStance>
            {
                { new Tuple<SwordStance, SwordCommand>(_idle, SwordCommand.Press), _attacking }, // Idle -> attacking
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.Release), _idle }, // Attacking -> Idle
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.Expire), _blocking }, // Attacking -> Block
                { new Tuple<SwordStance, SwordCommand>(_attacking, SwordCommand.Press), _attacking }, // Self transition
                { new Tuple<SwordStance, SwordCommand>(_blocking, SwordCommand.Release), _idle }, // Blocking -> Idle
                { new Tuple<SwordStance, SwordCommand>(_blocking, SwordCommand.Hit), _countering }, // Blocking -> Countering
                { new Tuple<SwordStance, SwordCommand>(_countering, SwordCommand.Expire), _idle } // Countering -> Idle
            };

            _hitboxOffset = primaryHitbox.transform.localPosition.y;
            primaryHitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
            secondaryHitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
            diagonalHitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
            
            SetSwordDirection(SwordDirection.Up);
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
            primaryHitbox.gameObject.layer = HitboxTrigger.GetLayer(_currentStance.hitboxType);
            if (_currentStance.transitionTime > 0.0f)
            {
                TimerManager.instance.CreateOrResetTimer(ref _transitionTimer, this, _currentStance.transitionTime, () => { Command(SwordCommand.Expire); });
            }
        }

        static SwordDirection GetSwordDirectionFromVector(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0.0f ? SwordDirection.Right : SwordDirection.Left;
            }
            return input.y > 0.0f ? SwordDirection.Up : SwordDirection.Down;
        }
        
        private static float GetRotation(SwordDirection direction)
        {
            return (int)direction * 90.0f;
        }

        private static float GetRotation(SwordDirection start, SwordDirection end)
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
        public void OnSwordLeft(InputAction.CallbackContext context) { SwordDirectInput(context, SwordDirection.Left); }
        public void OnSwordRight(InputAction.CallbackContext context) { SwordDirectInput(context, SwordDirection.Right); }
        public void OnSwordUp(InputAction.CallbackContext context) { SwordDirectInput(context, SwordDirection.Up); }
        public void OnSwordDown(InputAction.CallbackContext context) { SwordDirectInput(context, SwordDirection.Down); }

        private void SwordDirectInput(InputAction.CallbackContext context, SwordDirection direction)
        {
            if (GameState.instance.paused)
            {
                return;
            }
            
            if (context.performed)
            {
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
            Command(SwordCommand.Press);

            if (!_currentStance.canChangeDirection)
            {
                return;
            }
            
            var oldDirection = swordDirection;
            swordDirection = direction;
            swordSprite.transform.parent.rotation = Quaternion.Euler(0.0f, 0.0f, GetRotation(direction));
            OnSwordDirectionChanged(oldDirection, swordDirection);
        }

        private void OnSwordDirectionChanged(SwordDirection oldDirection, SwordDirection newDirection)
        {
            targetsHit.Clear();
            
            var directionalChange = Mathf.Abs(GetSwordDirectionDelta(oldDirection, newDirection));
            switch (directionalChange)
            {
                case 0:
                    Stab(newDirection);
                    return;
                case 1:
                    Slash(oldDirection, newDirection);
                    return;
                case 2:
                    Slam(newDirection);
                    return;
            }
        }

        private void Stab(SwordDirection direction)
        {
            primaryHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(direction));
            primaryHitbox.Disable();
            primaryHitbox.Enable();
            secondaryHitbox.Disable();
            diagonalHitbox.Disable();
        }

        private void Slash(SwordDirection start, SwordDirection end)
        {
            primaryHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(end));
            primaryHitbox.transform.rotation = Quaternion.Euler(0.0f, 0.0f, GetRotation(end) - 90.0f);
            secondaryHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(start));
            diagonalHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(start, end));
            diagonalHitbox.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, GetRotation(start, end) - 90.0f);
            primaryHitbox.Enable();
            secondaryHitbox.Enable();
            diagonalHitbox.Enable();
            
            TimerManager.instance.CreateOrResetTimer(ref _diagonalHitboxTimer, this, 0.12f, () => { diagonalHitbox.Disable(); });
            TimerManager.instance.CreateOrResetTimer(ref _secondaryHitboxTimer, this, 0.06f, () => { secondaryHitbox.Disable(); });
        }

        private void Slam(SwordDirection direction)
        {
            primaryHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(direction));
            primaryHitbox.Enable();
            secondaryHitbox.Disable();
            diagonalHitbox.Disable();
        }

        private Vector3 GetLocalPositionFromRotation(float rotationDegrees)
        {
            var rads = Mathf.Deg2Rad * rotationDegrees;
            return new Vector3(Mathf.Cos(rads) * _hitboxOffset, Mathf.Sin(rads) * _hitboxOffset, 0.0f);
        }

        public override void BlockedEnemyAttack(Collider2D selfArmorHitbox, Collider2D attackerHitbox)
        {
            Command(SwordCommand.Hit);
            var enemyHealth = attackerHitbox.transform.root.GetComponent<HealthComponent>();
            if (enemyHealth)
            {
                enemyHealth.Stun(1.0f, this);
            }
        }
    }
}