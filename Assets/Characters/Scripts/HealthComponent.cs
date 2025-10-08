using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class StunHandle
{
    public StunHandle(HealthComponent source, StunStack stack)
    {
        _source = new WeakReference<HealthComponent>(source);
        _stack = stack;
    }

    ~StunHandle()
    {
        Clear();
    }

    public void Clear()
    {
        if (_source != null && _source.TryGetTarget(out var list))
        {
            list.RemoveStun(_stack);
        }

        _source = null;
        _stack = null;
    }
    
    private WeakReference<HealthComponent> _source;
    private StunStack _stack;
}

public class StunStack
{
    public StunStack(MonoBehaviour source)
    {
        this._source = source;
    }

    private readonly MonoBehaviour _source;

    public override string ToString()
    {
        return _source.name + ":" + _source.GetType();
    }
}
public class HealthComponent : MonoBehaviour
{
    [SerializeField] public float maxHealth;
    [SerializeField] private float health;
    [SerializeField] public UnityEvent onStunned;
    [SerializeField] public UnityEvent onStunEnd;

    private readonly List<StunStack> _stunStacks = new();
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
        onStunned.Invoke();
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
        var tempStunHandle = Stun(source);
        // Debug.LogFormat("{0} stunned by {1}", name, source.GetType());
        TimerManager.instance.CreateTimer(this, duration, () =>
        {
            tempStunHandle.Clear();
            // Debug.LogFormat("{0} stun ended by timer from {1}", name, source.GetType());
        });
    }

    public StunHandle Stun(MonoBehaviour source)
    {
        if (_stunStacks.Count == 0)
        {
            onStunned.Invoke();
        }
        var newStack = new StunStack(source);
        _stunStacks.Add(newStack);
        return new StunHandle(this, newStack);
    }

    public void RemoveStun(StunStack stun)
    {
        if (!_stunStacks.Remove(stun) || _stunStacks.Count != 0)
        {
            return;
        }
        
        onStunEnd.Invoke();
    }

    private void OnDrawGizmos()
    {
        if (IsStunned())
        {
            var stuns = string.Join("\n", _stunStacks.Select(stack => stack.ToString()));
            Handles.Label(transform.position + new Vector3(-0.5f, 1.75f, 0.0f), $"Stunned:\n{stuns}");
        }
        Handles.Label(transform.position + new Vector3(-0.5f, 2.0f, 0.0f), $"{health} / {maxHealth}");
    }

    private bool IsStunned()
    {
        return _stunStacks.Count > 0;
    }
}