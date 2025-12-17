using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileComponent : MonoBehaviour
{
    [SerializeField] private bool destroyOnPlayerTakeDamage;
    [SerializeField] public float damage;
    [SerializeField] private GameObject destructionParticlePrefab;
    [SerializeField] private float particleLifetime = 1.0f;

    public Vector2 velocity;
    
    private GameObject _owner;
    private Rigidbody2D _rigidbody;
    private TimerHandle _lifetimeTimer;
    [SerializeField] private CircleCollider2D circleCollider;

    private void OnEnable()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Setup(Vector3 targetPosition, GameObject parent, float speed, float projectileLifetime = 4.0f)
    {
        _owner = parent;
        velocity = (targetPosition - transform.position).normalized * speed;
        TimerManager.instance.CreateOrResetTimer(ref _lifetimeTimer, this, projectileLifetime, DestroyProjectile);
    }

    private void FixedUpdate()
    {
        _rigidbody.transform.position += (Vector3)velocity * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var root = other.transform.root.gameObject;
        if (!root.CompareTag("Player"))
        {
            return;
        }

        var healthComponent = root.GetComponent<HealthComponent>();
        if (!healthComponent)
        {
            return;
        }
        
        healthComponent.TakeDamage(damage, _owner);

        if (destroyOnPlayerTakeDamage)
        {
            DestroyProjectile();
        }
    }

    private void OnCollisionEnter2D()
    {
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        if (destructionParticlePrefab)
        {
            var particleObject = Instantiate(destructionParticlePrefab);
            particleObject.transform.position = transform.position;
            TimerManager.instance.CreateTimer(this, particleLifetime, () =>
            {
                Destroy(particleObject);
            });
        }

        if (gameObject)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        DebugHelpers.Drawing.DrawCircle(transform.position, circleCollider.radius, new Color(1.0f, 0.0f, 0.0f, 0.5f));
    }
}
