using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player.Scripts
{
    public enum SwordDirection
    {
        Up,
        Right,
        Down,
        Left
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

        public void OnSwordInput(InputAction.CallbackContext context)
        {
            
        }

        public void SetSwordDirection(SwordDirection direction)
        {
            if (swordDirection == direction)
            {
                return;
            }
            
            
        }

        private void OnSwordDirectionChanged(SwordDirection oldDirection, SwordDirection newDirection)
        {
            
        }
    }
}