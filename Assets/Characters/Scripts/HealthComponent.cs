using Game.StatusEffects;
using ImGuiNET;
using Shared;
using UImGui;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : DamageListener
{
    [SerializeField] public float maxHealth;
    [SerializeField] private float health;
    [SerializeField] public UnityEvent onStunned;
    [SerializeField] public UnityEvent onStunEnd;

    public readonly StatusEffectContainer statusEffects = new();
    [field: SerializeField] public bool alive { get; private set; } = true;

    public UnityEvent<GameObject> onTakeDamage;
    public UnityEvent onDeath;
    
    private TimerHandle _stunTimer;

    private StatusEffect _stun;

    private void Start()
    {
        _stun = GameState.instance.effectList.stunEffect;
        
        statusEffects.GetEffectAppliedEvent(_stun).AddListener( () => { onStunned.Invoke();} );
        statusEffects.GetEffectRemovedEvent(_stun).AddListener( () => { onStunEnd.Invoke();} );
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

    public override void TakeDamage(float damage, GameObject source, DamageType damageType = DamageType.Raw)
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

    public void Stun(float duration, MonoBehaviour source)
    {
        statusEffects.ApplyStatusEffectInstance(new StatusEffectInstance(_stun, source, duration));
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
            foreach (var item in statusEffects)
            {
                var title = item.Key.accumulator == EffectAccumulator.None
                    ? $"{item.Key.effectName}: {item.Value.Count} stacks"
                    : $"{item.Key.effectName}: {item.Value.Count} stacks {statusEffects.Accumulate(item.Key, item.Value)}";
                if (ImGui.TreeNode(title))
                {
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