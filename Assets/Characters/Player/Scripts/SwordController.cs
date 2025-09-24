using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player.Scripts
{
    public enum SwordDirection
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
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
        [SerializeField] private List<GameObject> hitboxes;
        
        static SwordDirection GetSwordDirectionFromVector(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0.0f ? SwordDirection.Right : SwordDirection.Left;
            }
            return input.y > 0.0f ? SwordDirection.Up : SwordDirection.Down;
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
            if (!context.performed || GameState.instance.paused)
            {
                return;
            }
            
            SetSwordDirection(direction);
        }

        private void SetSwordDirection(SwordDirection direction)
        {
            var oldDirection = swordDirection;
            swordDirection = direction;
            swordSprite.transform.parent.rotation = Quaternion.Euler(0.0f, 0.0f, (int)swordDirection * -90);
            //transform.rotation = Quaternion.Euler(0.0f, 0.0f, (int)swordDirection * -90);
            OnSwordDirectionChanged(oldDirection, swordDirection);
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
            HideAllHitboxes();
            GetHitboxForDirection(direction).SetActive(true);
        }

        private void Slash(SwordDirection start, SwordDirection end)
        {
            HideAllHitboxes();
            GetHitboxForDirection(start).SetActive(true);
            GetHitboxForDirection(end).SetActive(true);
            GetHitboxForDirections(start, end).SetActive(true);
        }

        private void Slam(SwordDirection direction)
        {
            HideAllHitboxes();
            GetHitboxForDirection(direction).SetActive(true);
        }

        GameObject GetHitboxForDirection(SwordDirection direction)
        {
            var realIndex = (int)direction * 2;
            return hitboxes[realIndex];
        }

        GameObject GetHitboxForDirections(SwordDirection directionA, SwordDirection directionB)
        {
            return hitboxes[GetHitboxIndexForDirections(directionA, directionB)];
        }
        
        static int GetHitboxIndexForDirections(SwordDirection start, SwordDirection end)
        {
            var result = (int)start * 2 + GetSwordDirectionDelta(start, end);
            if (result < 0)
            {
                result += 8;
            }

            return result;
        }

        private void HideAllHitboxes()
        {
            foreach(var hitbox in hitboxes)
            {
                hitbox.SetActive(false);
            }
        }
    }
}