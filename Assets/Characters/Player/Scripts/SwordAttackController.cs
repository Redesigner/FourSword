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

    public enum SwordStance
    {
        Idle,
        Attacking,
        Blocking
    }
    
    public class SwordAttackController : AttackController
    {
        [SerializeField] private SwordDirection swordDirection;
        [SerializeField] private SwordStance swordStance;
        [SerializeField] private SpriteRenderer swordSprite;

        [SerializeField] private HitboxTrigger primaryHitbox;
        [SerializeField] private HitboxTrigger secondaryHitbox;
        [SerializeField] private HitboxTrigger diagonalHitbox;

        private float _hitboxOffset;
        private TimerHandle _diagonalHitboxTimer;
        private TimerHandle _secondaryHitboxTimer;
        private TimerHandle _blockTimer;

        private void Start()
        {
            _hitboxOffset = primaryHitbox.transform.localPosition.y;
            primaryHitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
            secondaryHitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
            diagonalHitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
            
            SetSwordStance(SwordStance.Idle);
            SetSwordDirection(SwordDirection.Up);
            secondaryHitbox.Disable();
            diagonalHitbox.Disable();
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

        static int GetSwordDirectionDelta(SwordDirection a, SwordDirection b)
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
        

        public void OnSwordInput(InputAction.CallbackContext context)
        {
            // ?
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
                SetSwordStance(SwordStance.Idle);
            }
        }

        private void SetSwordDirection(SwordDirection direction)
        {
            var oldDirection = swordDirection;
            swordDirection = direction;
            swordSprite.transform.parent.rotation = Quaternion.Euler(0.0f, 0.0f, GetRotation(direction));
            OnSwordDirectionChanged(oldDirection, swordDirection);
        }

        private void SetSwordStance(SwordStance stance)
        {
            // Explicitly prevent idle -> blocking
            if (stance == SwordStance.Blocking && swordStance == SwordStance.Idle)
            {
                return;
            }
            
            swordStance = stance;
            switch(stance)
            {
                default:
                case SwordStance.Attacking:
                case SwordStance.Idle:
                    primaryHitbox.gameObject.layer = 6;
                    break;
                case SwordStance.Blocking:
                    primaryHitbox.gameObject.layer = 8;
                    break;
            }
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
            SetSwordStance(SwordStance.Attacking);
            primaryHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(direction));
            primaryHitbox.Disable();
            primaryHitbox.Enable();
            secondaryHitbox.Disable();
            diagonalHitbox.Disable();
            
            TimerManager.instance.CreateOrResetTimer(ref _blockTimer, this, 0.5f, () => { SetSwordStance(SwordStance.Blocking); });
        }

        private void Slash(SwordDirection start, SwordDirection end)
        {
            SetSwordStance(SwordStance.Attacking);
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
            TimerManager.instance.CreateOrResetTimer(ref _blockTimer, this, 0.5f, () => { SetSwordStance(SwordStance.Blocking); });
        }

        private void Slam(SwordDirection direction)
        {
            SetSwordStance(SwordStance.Attacking);
            primaryHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(direction));
            primaryHitbox.Enable();
            secondaryHitbox.Disable();
            diagonalHitbox.Disable();
            
            TimerManager.instance.CreateOrResetTimer(ref _blockTimer, this, 0.5f, () => { SetSwordStance(SwordStance.Blocking); });
        }

        private Vector3 GetLocalPositionFromRotation(float rotationDegrees)
        {
            var rads = Mathf.Deg2Rad * rotationDegrees;
            return new Vector3(Mathf.Cos(rads) * _hitboxOffset, Mathf.Sin(rads) * _hitboxOffset, 0.0f);
        }
    }
}