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
        
        static SwordDirection GetSwordDirectionFromVector(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0.0f ? SwordDirection.Right : SwordDirection.Left;
            }
            return input.y > 0.0f ? SwordDirection.Up : SwordDirection.Down;
        }

        static SwordDirection GetSwordDirectionDelta(SwordDirection a, SwordDirection b)
        {
            var rawDelta = b - a;
            rawDelta = rawDelta < 0 ? rawDelta : rawDelta + 4;
            return (SwordDirection)rawDelta;
        }
        

        public void OnSwordInput(InputAction.CallbackContext context)
        {
            
        }

        public void SetSwordDirection(SwordDirection direction)
        {
            if (swordDirection == direction)
            {
                Stab(direction);
                return;
            }
            
            //if (swordDirection )
        }

        private void OnSwordDirectionChanged(SwordDirection oldDirection, SwordDirection newDirection)
        {
        }

        private void Stab(SwordDirection direction)
        {
            
        }

        private void Slash(SwordDirection start, SwordDirection end)
        {
            
        }

        private void Slam(SwordDirection direction)
        {
            
        }
    }
}