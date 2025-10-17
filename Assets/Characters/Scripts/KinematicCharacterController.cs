using System.Collections.Generic;
using System.Linq;
using Game.StatusEffects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class KinematicCharacterController : Kinematics.KinematicObject
{
    private static readonly int HorizontalBlend = Animator.StringToHash("Horizontal");
    private static readonly int VerticalBlend = Animator.StringToHash("Vertical");
    private static readonly int SpeedBlend = Animator.StringToHash("Speed");

    /** <summary>
     *  Where the character is currently looking
     * </summary>
    **/ 
    public Vector2 lookDirection
    {
        get => _lookDirection;
        private set => _lookDirection = value.normalized;
    }
    private Vector2 _lookDirection;

    [SerializeField] public UnityEvent<float> lookDirectionChanged;

    /// <summary>
    /// How fast the player walks when pressing the control stick all the way
    /// </summary>
    [SerializeField]
    private float walkSpeed = 4.0f;

    /// <summary>
    /// Should the character automatically face (set lookDirection) to be the same as movement?
    /// </summary>
    [SerializeField] private bool faceMovement = true;
    
    private bool _movementEnabled = true;
    private Animator _animator;
    private readonly List<(Kinematics.KinematicListener, RaycastHit2D)> _objectsHitThisFrame = new();
    private Vector2 _moveInput;
    private TimerHandle _knockbackTimer;
    private HealthComponent _healthComponent;
    private float _speedModifier = 1.0f;
    private Vector2 _knockbackVelocity;
    private bool _isKnockedBack = false;

    private StatusEffect _speedEffect;

    protected override void OnEnable()
    {
        base.OnEnable();

        _speedEffect = GameState.instance.effectList.speedEffect;
        _healthComponent = GetComponent<HealthComponent>();
        _healthComponent.statusEffects.GetEffectStacksChangedEvent(_speedEffect).AddListener((stackCount, speedValue) =>
        {
            _speedModifier = stackCount == 0 ? 1.0f : speedValue;
        });
        _animator = GetComponent<Animator>();
    }

    protected override Vector2 ComputeVelocity()
    {
        var newVelocity = _movementEnabled ? _moveInput * GetWalkSpeed() : Vector2.zero;
        if (_isKnockedBack)
        {
            newVelocity += _knockbackVelocity;
        }
        return newVelocity;
    }

    protected override void FixedUpdate()
    {
        _objectsHitThisFrame.Clear();
        velocity = ComputeVelocity();

        foreach (var objectHit in _objectsHitThisFrame)
        {
            objectHit.Item1.OnHit(this, objectHit.Item2);
        }

        if (velocity is { x: 0.0f, y: 0.0f })
        {
            return;
        }

        CollideAndSlide(velocity * Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        DebugHelpers.Drawing.DrawArrow(transform.position, lookDirection, 1.0f, Color.green, Time.deltaTime);
    }

    /**
     * <summary>Input action listener for movement.
     * If you want to set the movement directly, use MoveInput instead.</summary>
     */
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        MoveInput(input);
    }

    /**
     * <summary>Set the character's movement direction.
     * Will not do anything if movement is disabled or the game is paused.</summary>
     * <param name="input">Direction to move the player.
     * Value is automatically normalized only if the length is greater than 1</param>
     */
    public void MoveInput(Vector2 input)
    {
        var inputMagnitudeSquared = input.sqrMagnitude;
        _moveInput = inputMagnitudeSquared > 1.0f ? input / Mathf.Sqrt(inputMagnitudeSquared) : input;
        
        if (!_movementEnabled || GameState.instance.paused)
        {
            return;
        }
        
        if (_moveInput.x != 0.0f || _moveInput.y != 0.0f)
        {
            if (faceMovement)
            {
                SetLookDirection(_moveInput);
            }

            _animator.SetFloat(HorizontalBlend, _moveInput.x);
            _animator.SetFloat(VerticalBlend, _moveInput.y);
            _animator.SetFloat(SpeedBlend, 1.0f);
        }
        else
        {
            _animator.SetFloat(SpeedBlend, 0.0f);
        }
    }

    public void SetLookDirection(Vector2 direction)
    {
        _lookDirection = direction.normalized;
        _animator.SetFloat(HorizontalBlend, _lookDirection.x);
        _animator.SetFloat(VerticalBlend, _lookDirection.y);
        lookDirectionChanged.Invoke(Mathf.Atan2(_lookDirection.y, _lookDirection.x));
    }
    
    protected override void OnMovementHit(RaycastHit2D hit)
    {
        // Debug.DrawRay(hit.point, hit.normal, Color.blue);

        if (!hit.collider.CompareTag("KinematicListener"))
        {
            return;
        }
        
        // This shouldn't be too expensive, because we pre-check the tag first
        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        var listener = hit.collider.GetComponent<Kinematics.KinematicListener>();
        if (!listener)
        {
            return;
        }
        
        if (_objectsHitThisFrame.Any(objectHit => objectHit.Item1 == listener))
        {
            return;
        }
        _objectsHitThisFrame.Add((listener, hit));
    }

    /** <summary>
     * Enable the character's movement input. They can still be moved by other sources, such as knockback
     * </summary>
     */
    public void EnableMovement()
    {
        _movementEnabled = true;
        
        // Pull our last registered input value into animator
        if (_moveInput.x != 0.0f || _moveInput.y != 0.0f)
        {
            if (faceMovement)
            {
                SetLookDirection(_moveInput);
            }

            _animator.SetFloat(HorizontalBlend, _moveInput.x);
            _animator.SetFloat(VerticalBlend, _moveInput.y);
            _animator.SetFloat(SpeedBlend, 1.0f);
        }
        else
        {
            _animator.SetFloat(SpeedBlend, 0.0f);
        }
    }

    /**
     * <summary>Disable the character's movement input. They can still be moved by other sources</summary>
     */
    public void DisableMovement()
    {
        _movementEnabled = false;
        velocity = Vector2.zero;
    }

    /**
     * <summary>Knock the character back, applying a constant velocity for a period of time</summary>
     * <param name="knockbackVector">Velocity to apply -- direction and magnitude</param>
     * <param name="duration">How long to apply the knockback. Movement will be disabled for this period of time</param>
     */
    public void Knockback(Vector2 knockbackVector, float duration)
    {
        var healthComponent = GetComponent<HealthComponent>();
        if (!healthComponent)
        {
            return;
        }

        if (_isKnockedBack)
        {
            return;
        }

        _isKnockedBack = true;
        _knockbackVelocity = knockbackVector;
        TimerManager.instance.CreateOrResetTimer(ref _knockbackTimer, this, duration, () =>
        {
            _isKnockedBack = false;
        });
    }

    private float GetWalkSpeed()
    {
        return walkSpeed * _speedModifier;
    }
}