using System.Collections.Generic;
using System.Linq;
using Characters;
using Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class EnemyAttackController : AttackController
{
    [SerializeField] private HitboxTrigger hitboxComponent;
    [SerializeField] private bool damageOnce = true;
    [SerializeField] public UnityEvent onShieldBroken;

    public UnityEvent attackCompleted;
    private Animator _animator;
    private readonly List<HitboxTrigger> _hitboxes = new();
    
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    
    protected void OnEnable()
    {
        _animator = GetComponent<Animator>();

        foreach (var hitbox in GetComponentsInChildren<HitboxTrigger>().ToList().Where(hitbox => hitbox.GetHitboxType() == HitboxType.Hitbox))
        {
            _hitboxes.Add(hitbox);
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
        targetsHit.Clear();

        foreach (var hitbox in _hitboxes)
        {
            hitbox.Disable();
        }
    }

    public override bool BlockedEnemyAttack(DamageType blockedDamageType, Collider2D selfArmorHitbox, Collider2D attackerHitbox)
    {
        if (blockedDamageType != DamageType.Smash)
        {
            return true;
        }
        
        onShieldBroken.Invoke();
        return false;

    }
}