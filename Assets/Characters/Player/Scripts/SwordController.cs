using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

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

    enum SwordStance
    {
        Idle,
        Attacking,
        Blocking
    }
    
    public class SwordController : MonoBehaviour
    {
        [SerializeField] private SwordDirection swordDirection;
        [SerializeField] private SwordStance swordStance;
        [SerializeField] private SpriteRenderer swordSprite;

        [SerializeField] private GameObject primaryHitbox;
        [SerializeField] private GameObject secondaryHitbox;
        [SerializeField] private GameObject diagonalHitbox;

        private float _hitboxOffset;
        private TimerHandle _diagonalHitboxTimer;
        private TimerHandle _secondaryHitboxTimer;
        private TimerHandle _blockTimer;

        private void Start()
        {
            _hitboxOffset = primaryHitbox.transform.localPosition.y;
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
            switch (stance)
            {
                case SwordStance.Attacking:
                    primaryHitbox.GetComponent<SpriteRenderer>().color = Color.red;
                    return;
                case SwordStance.Blocking:
                    primaryHitbox.GetComponent<SpriteRenderer>().color = Color.blue;
                    return;
                case SwordStance.Idle:
                    primaryHitbox.GetComponent<SpriteRenderer>().color = Color.green;
                    return;
                default:
                    return;
            }
        }

        private void OnSwordDirectionChanged(SwordDirection oldDirection, SwordDirection newDirection)
        {
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
            primaryHitbox.SetActive(true);
            secondaryHitbox.SetActive(false);
            diagonalHitbox.SetActive(false);
            
            TimerManager.instance.CreateOrResetTimer(ref _blockTimer, this, 0.5f, () => { SetSwordStance(SwordStance.Blocking); });
        }

        private void Slash(SwordDirection start, SwordDirection end)
        {
            SetSwordStance(SwordStance.Attacking);
            primaryHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(end));
            secondaryHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(start));
            diagonalHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(start, end));
            diagonalHitbox.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, GetRotation(start, end) - 90.0f);
            primaryHitbox.SetActive(true);
            secondaryHitbox.SetActive(true);
            diagonalHitbox.SetActive(true);
            
            TimerManager.instance.CreateOrResetTimer(ref _diagonalHitboxTimer, this, 0.12f, () => { diagonalHitbox.SetActive(false); });
            TimerManager.instance.CreateOrResetTimer(ref _secondaryHitboxTimer, this, 0.06f, () => { secondaryHitbox.SetActive(false); });
            TimerManager.instance.CreateOrResetTimer(ref _blockTimer, this, 0.5f, () => { SetSwordStance(SwordStance.Blocking); });
        }

        private void Slam(SwordDirection direction)
        {
            SetSwordStance(SwordStance.Attacking);
            primaryHitbox.transform.localPosition = GetLocalPositionFromRotation(GetRotation(direction));
            primaryHitbox.SetActive(true);
            secondaryHitbox.SetActive(false);
            diagonalHitbox.SetActive(false);
            
            TimerManager.instance.CreateOrResetTimer(ref _blockTimer, this, 0.5f, () => { SetSwordStance(SwordStance.Blocking); });
        }

        private Vector3 GetLocalPositionFromRotation(float rotationDegrees)
        {
            var rads = Mathf.Deg2Rad * rotationDegrees;
            return new Vector3(Mathf.Cos(rads) * _hitboxOffset, Mathf.Sin(rads) * _hitboxOffset, 0.0f);
        }
    }
}