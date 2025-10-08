using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class StunHandle
{
    public StunHandle(HealthComponent healthComponent)
    {
        _healthComponent = new WeakReference<HealthComponent>(healthComponent);
        healthComponent.IncrementStunCount();
    }

    ~StunHandle()
    {
        if (_healthComponent.TryGetTarget(out var owner))
        {
            owner.DecrementStunCount();
        }
    }
    
    private readonly WeakReference<HealthComponent> _healthComponent;
}
public class HealthComponent : MonoBehaviour
{
    [SerializeField] public float maxHealth;
    [SerializeField] private float health;
    [SerializeField] public UnityEvent onStunned;
    [SerializeField] public UnityEvent onStunEnd;

    private int _stunCount = 0;
    [field: SerializeField] public bool alive { get; private set; } = true;

    public UnityEvent<GameObject> onTakeDamage;
    public UnityEvent onDeath;

    private TimerHandle _stunTimer;

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
        TimerManager.instance.CreateTimer(this, 0.5f, () =>
        {
            Destroy(gameObject);
        });
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
        Debug.LogFormat("{0} stunned by {1}", name, source.GetType());
        IncrementStunCount();
        TimerManager.instance.CreateTimer(this, duration, () =>
        {
            Debug.LogFormat("{0} stun ended by timer from {1}", name, source.GetType());
            DecrementStunCount();
        });
    }

    public StunHandle Stun()
    {
        return new StunHandle(this);
    }

    private void OnDrawGizmos()
    {
        if (IsStunned())
        {
            Handles.Label(transform.position + new Vector3(-0.5f, 1.25f, 0.0f), $"Stunned: {_stunCount}");
        }
        Handles.Label(transform.position + new Vector3(-0.5f, 1.5f, 0.0f), $"{health} / {maxHealth}");
    }

    public bool IsStunned()
    {
        return _stunCount > 0;
    }

    public void IncrementStunCount()
    {
        if (_stunCount++ == 0)
        {
            onStunned.Invoke();
        }
    }

    public void DecrementStunCount()
    {
        if (--_stunCount == 0)
        {
            onStunEnd.Invoke();
        }
    }
}