using System.Collections.Generic;
using System.Linq;
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

    private List<HealthComponent> _enemiesHit = new();
    private List<HitboxTrigger> _hitboxes = new();
    
    protected void OnEnable()
    {
        _animator = GetComponent<Animator>();

        _hitboxes = GetComponentsInChildren<HitboxTrigger>().ToList();
        foreach (var hitbox in _hitboxes)
        {
            hitbox.hitboxOverlapped.AddListener(OnHitboxOverlapped);
        }
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
        
        foreach (var hitbox in _hitboxes)
        {
            hitbox.Enable();
        }
    }

    public void CompleteAttack()
    {
        // Should be called from animation (if used)
        attackCompleted.Invoke();
        _enemiesHit.Clear();

        foreach (var hitbox in _hitboxes)
        {
            hitbox.Disable();
        }
    }

    private void OnHitboxOverlapped(Collider2D hitbox, Collider2D hitboxOther)
    {
        var enemyHealth = hitboxOther.GetComponent<HealthComponent>();
        if (!enemyHealth)
        {
            return;
        }

        if (_enemiesHit.Contains(enemyHealth))
        {
            return;
        }
        
        _enemiesHit.Add(enemyHealth);
        Debug.Log("Hit something!");
    }
}