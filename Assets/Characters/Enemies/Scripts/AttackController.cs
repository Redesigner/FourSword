using Characters;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class AttackController : MonoBehaviour
{
    [SerializeField] private HitboxTrigger hitboxComponent;

    public UnityEvent attackCompleted;
    
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");

    private Animator _animator;

    protected void OnEnable()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }
        
        Attack();
    }

    public void Attack()
    {
        // hitboxComponent.Reset();
        _animator.SetTrigger(AttackTrigger);
    }

    public void CompleteAttack()
    {
        // Should be called from animation (if used)
        attackCompleted.Invoke();
    }
}