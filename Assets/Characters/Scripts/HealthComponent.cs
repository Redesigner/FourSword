using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Game.StatusEffects;
using ImGuiNET;
using Shared;
using UImGui;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Events;

public enum Team
{
    Dogs,
    Robots
}

public class HealthComponent : DamageListener
{
    [SerializeField] public float maxHealth;
    [field: SerializeField] public float health { get; private set; }

    [SerializeField] [Min(0.0f)] private float slashResistance = 1.0f;
    [SerializeField] [Min(0.0f)] private float pierceResistance = 1.0f;
    [SerializeField] [Min(0.0f)] private float smashResistance = 1.0f;

    [SerializeField] [Min(0.0f)] private float invulnerabilityTime = 0.5f;
    [SerializeField] [Min(0.0f)] private float deathFadeOutTime = 0.5f;
    
    [SerializeField] public UnityEvent onStunned;
    [SerializeField] public UnityEvent onStunEnd;

    [SerializeField] public UnityEvent onTakeSlashDamage;
    [SerializeField] public UnityEvent onTakePierceDamage;
    [SerializeField] public UnityEvent onTakeSmashDamage;

    [SerializeField] public Team team;

    public readonly StatusEffectContainer statusEffects = new();
    [field: SerializeField] public bool alive { get; private set; } = true;

    public UnityEvent<GameObject> onTakeDamage;
    public UnityEvent onDeath;
    
    private TimerHandle _stunTimer;

    private StatusEffect _stun;
    private StatusEffect _invulnerability;

    private List<HitboxTrigger> _hurtboxes;

    private void Start()
    {
        _stun = GameState.instance.effectList.stunEffect;
        _invulnerability = GameState.instance.effectList.invulnerabilityEffect;

        _hurtboxes = GetComponentsInChildren<HitboxTrigger>().Where(trigger => trigger.GetHitboxType() == HitboxType.Hurtbox).ToList();
        
        statusEffects.GetEffectAppliedEvent(_stun).AddListener( () => { onStunned.Invoke();} );
        statusEffects.GetEffectRemovedEvent(_stun).AddListener( () => { onStunEnd.Invoke();} );
        
        
        // Invulnerability toggles the hurtboxes, so we don't have to worry about missing events
        // or re-triggering overlaps when the invulnerability period ends
        statusEffects.GetEffectAppliedEvent(_invulnerability).AddListener(() =>
        {
            foreach (var hurtbox in _hurtboxes)
            {
                hurtbox.Disable();
            }
        });
        
        statusEffects.GetEffectRemovedEvent(_invulnerability).AddListener(() =>
        {
            foreach (var hurtbox in _hurtboxes)
            {
                hurtbox.Enable();
            }
        });
    }

    private void Awake()
    {
#if UNITY_EDITOR
        UImGuiUtility.Layout += OnLayout;
        UImGuiUtility.OnInitialize += OnInitialize;
        UImGuiUtility.OnDeinitialize += OnDeinitialize;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        UImGuiUtility.Layout -= OnLayout;
        UImGuiUtility.OnInitialize -= OnInitialize;
        UImGuiUtility.OnDeinitialize -= OnDeinitialize;
#endif
    }

    public override void TakeDamage(float damage, GameObject source, DamageType damageType = DamageType.Raw)
    {
        // In some rare cases, projectiles can live after their owners, so make sure the owner isn't null
        if (!source)
        {
            return;
        }
        
        var attackerHealthComponent = source.transform.root.GetComponent<HealthComponent>();
        if (attackerHealthComponent && attackerHealthComponent.team == team)
        {
            return;
        }
        
        if (damage < 0.0f)
        {
            return;
        }

        // Double check that we don't apply damage, it shouldn't happen because the hurtboxes
        // are toggled off, but there are some situations where it could happen
        if (statusEffects.HasEffect(_invulnerability))
        {
            return;
        }

        if (!alive)
        {
            return;
        }

        var modifiedDamage = CalculateDamageAfterResistance(damage, damageType);
        if (modifiedDamage == 0.0f)
        {
            return;
        }

        health -= modifiedDamage;
        if (health > 0.0f)
        {
            // Apply a stun and knockback with the same duration
            // if we take damage
            onTakeDamage.Invoke(source);
            // statusEffects.ApplyStatusEffectInstance(new StatusEffectInstance(_stun, this, 0.25f));

            switch (damageType)
            {
                case DamageType.Slashing:
                    onTakeSlashDamage.Invoke();
                    break;
                
                case DamageType.Piercing:
                    onTakePierceDamage.Invoke();
                    break;
                
                case DamageType.Smash:
                    onTakeSmashDamage.Invoke();
                    break;
                
                case DamageType.Raw:
                default:
                    break;
            }
            
            if (invulnerabilityTime > 0.0f)
            {
                statusEffects.ApplyStatusEffectInstance(new StatusEffectInstance(GameState.instance.effectList.invulnerabilityEffect, this, invulnerabilityTime));
            }
            return;
        }

        Death(source);
    }

    public void Update()
    {
        statusEffects.Update(Time.deltaTime);
    }
    
    public void Heal(float healing)
    {
        if (!alive)
        {
            return;
        }

        if (healing < 0.0f)
        {
            return;
        }

        health += healing;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    private void Death(GameObject source)
    {
        var attackerHealthComponent = source.transform.root.GetComponent<HealthComponent>();
        onTakeDamage.Invoke(source);
        health = 0.0f;
        alive = false;
        onDeath.Invoke();
        
        GetComponent<KinematicCharacterController>().enabled = false;
        var attackController = GetComponentInChildren<AttackController>();
        if (attackController)
        {
            attackController.enabled = false;
        }

        if (deathFadeOutTime > 0.0f)
        {
            TimerManager.instance.CreateTimer(this, deathFadeOutTime, () =>
            {
                if (CompareTag("Player") && GameManager.Instance)
                {
                    GameManager.Instance.GameOver();
                }

                Destroy(gameObject);
            });
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Stun(float duration, MonoBehaviour source)
    {
        statusEffects.ApplyStatusEffectInstance(new StatusEffectInstance(_stun, source, duration));
    }

    private float CalculateDamageAfterResistance(float damage, DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Raw => damage,
            DamageType.Slashing => damage * slashResistance,
            DamageType.Piercing => damage * pierceResistance,
            DamageType.Smash => damage * smashResistance,
            _ => damage
        };
    }

    private void OnDrawGizmos()
    {
        var style = GUI.skin.label;
        style.alignment = TextAnchor.MiddleCenter;
        Handles.Label(transform.position + new Vector3(0.0f, 2.0f, 0.0f), $"{health} / {maxHealth}", style);
    }
    private void OnLayout(UImGui.UImGui obj)
    {
        if (!Selection.Contains(gameObject))
        {
            return;
        }

        if (ImGui.Begin($"{gameObject.name} Status###HealthComponent"))
        {
            ImGui.Text($"Health: {health} / {maxHealth}");
            foreach (var item in statusEffects)
            {
                var title = item.Key.accumulator == EffectAccumulator.None
                    ? $"{item.Key.effectName}: {item.Value.Count} stacks###HealthComponent{item.Key.effectName}"
                    : $"{item.Key.effectName}: {item.Value.Count} stack(s) - {statusEffects.Accumulate(item.Key, item.Value)}###HealthComponent{item.Key.effectName}";
                if (!ImGui.TreeNode(title))
                {
                    continue;
                }

                foreach (var instance in item.Value)
                {
                    var content = $"Source: {instance.applier.name}";
                    if (instance.duration != 0.0f)
                    {
                        content += $"\tTime: {instance.currentTime}/{instance.duration}s";
                    }

                    if (instance.strength != 0.0f)
                    {
                        content += $"\tStrength: {instance.strength}";
                    }

                    ImGui.Text(content);
                }
                ImGui.TreePop();
            }
        }

        if (ImGui.TreeNode("Damage resistances"))
        {
            ImGui.Text($"Slashing x{slashResistance:0.0}");
            ImGui.Text($"Piercing x{pierceResistance:0.0}");
            ImGui.Text($"Smash x{smashResistance:0.0}");
        }
        
        ImGui.End();

    }

    private void OnInitialize(UImGui.UImGui obj)
    {
        
    }

    private void OnDeinitialize(UImGui.UImGui obj)
    {
        
    }
    
    
}