using System;
using System.Collections.Generic;
using System.Linq;
using Game.StatusEffects;
using ImGuiNET;
using UImGui;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] public float maxHealth;
    [SerializeField] private float health;
    [SerializeField] public UnityEvent onStunned;
    [SerializeField] public UnityEvent onStunEnd;

    private readonly StatusEffectContainer _statusEffects = new();
    [field: SerializeField] public bool alive { get; private set; } = true;

    public UnityEvent<GameObject> onTakeDamage;
    public UnityEvent onDeath;
    
    private TimerHandle _stunTimer;

    private StatusEffect _stun;

    private void Start()
    {
        _stun = ScriptableObject.CreateInstance<StatusEffect>();
        _stun.effectName = "Stun";
        
        _statusEffects.onStatusEffectApplied.AddListener(statusEffect => { if (statusEffect == _stun) { onStunned.Invoke();}});
        _statusEffects.onStatusEffectRemoved.AddListener(statusEffect => { if (statusEffect == _stun) { onStunEnd.Invoke();}});
    }

    private void Awake()
    {
        UImGuiUtility.Layout += OnLayout;
        UImGuiUtility.OnInitialize += OnInitialize;
        UImGuiUtility.OnDeinitialize += OnDeinitialize;
    }

    private void OnDisable()
    {
        UImGuiUtility.Layout -= OnLayout;
        UImGuiUtility.OnInitialize -= OnInitialize;
        UImGuiUtility.OnDeinitialize -= OnDeinitialize;
    }

    public void TakeDamage(float damage, GameObject source)
    {
        if (damage < 0.0f)
        {
            return;
        }

        if (!alive)
        {
            return;
        }

        health -= damage;
        if (health > 0.0f)
        {
            onTakeDamage.Invoke(source);
            GetComponent<KinematicCharacterController>().Knockback((gameObject.transform.position - source.transform.position).normalized * 5.0f, 0.25f);
            return;
        }
        
        // onTakeDamage.Invoke(source);
        health = 0.0f;
        alive = false;
        onDeath.Invoke();

        GetComponent<KinematicCharacterController>().enabled = false;
        // onStunned.Invoke();
        TimerManager.instance.CreateTimer(this, 0.5f, () =>
        {
            Destroy(gameObject);
        });
    }

    public void Update()
    {
        _statusEffects.Update(Time.deltaTime);
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

    public void Stun(float duration, MonoBehaviour source)
    {
        _statusEffects.ApplyStatusEffectInstance(new StatusEffectInstance(_stun, source, duration));
    }

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + new Vector3(-0.5f, 2.0f, 0.0f), $"{health} / {maxHealth}");
    }
    private void OnLayout(UImGui.UImGui obj)
    {
        if (!Selection.Contains(gameObject))
        {
            return;
        }
        
        if (ImGui.Begin($"{gameObject.name} Status"))
        {
            ImGui.Text($"Health: {health} / {maxHealth}");
            foreach (var item in _statusEffects)
            {
                if (ImGui.TreeNode($"{item.Key.effectName}: {item.Value.Count} stacks"))
                {
                    foreach (var instance in item.Value)
                    {
                        ImGui.Text($"Source: {instance.applier.name} \tTime: {instance.currentTime}/{instance.duration}");
                    }
                }
            }
            ImGui.End();
        }
        
    }

    private void OnInitialize(UImGui.UImGui obj)
    {
        
    }

    private void OnDeinitialize(UImGui.UImGui obj)
    {
        
    }
}