using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
public class HealthComponent : MonoBehaviour
{
    [SerializeField] public float maxHealth;

    [SerializeField] private float health;

    public UnityEvent<GameObject> onTakeDamage;
    public UnityEvent onDeath;

    [field: SerializeField] public bool alive { get; private set; } = true;

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

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + new Vector3(-0.5f, 1.5f, 0.0f), $"{health} / {maxHealth}");
    }
}