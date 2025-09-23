using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class KinematicCharacterController : Kinematics.KinematicObject
{
    private static readonly int HorizontalBlend = Animator.StringToHash("Horizontal");
    private static readonly int VerticalBlend = Animator.StringToHash("Vertical");
    private static readonly int SpeedBlend = Animator.StringToHash("Speed");

    public Vector2 lookDirection
    {
        get => _lookDirection;
        private set => _lookDirection = value;
    }
    private Vector2 _lookDirection;


    /// <summary>
    /// How fast the player walks when pressing the control stick all the way
    /// </summary>
    [SerializeField]
    private float walkSpeed = 4.0f;
    
    private bool _movementEnabled = true;

    private Animator _animator;

    private readonly List<(Kinematics.KinematicListener, RaycastHit2D)> _objectsHitThisFrame = new();

    private Vector2 _moveInput;

    private TimerHandle _knockbackTimer;

    protected override void OnEnable()
    {
        base.OnEnable();

        _animator = GetComponent<Animator>();
    }

    protected override Vector2 ComputeVelocity()
    {
        return _movementEnabled ? _moveInput * walkSpeed : velocity;
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

    /**
     * <summary>Input action listener for movement. If you want to set the movement directly, use MoveInput instead.</summary>
     */
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (!_movementEnabled || GameState.instance.paused)
        {
            return;
        }
        
        MoveInput(input);
    }

    /**
     * <summary>Set the character's movement direction. Will not do anything if movement is disabled or the game is paused.</summary>
     * <param name="input">Direction to move the player. Value is automatically normalized only if the length is greater than 1</param>
     */
    public void MoveInput(Vector2 input)
    {
        var inputMagnitudeSquared = input.sqrMagnitude;
        _moveInput = inputMagnitudeSquared > 1.0f ? input / Mathf.Sqrt(inputMagnitudeSquared) : input;
        
        if (_moveInput.x != 0.0f || _moveInput.y != 0.0f)
        {
            _lookDirection.x = _moveInput.x;
            _lookDirection.y = _moveInput.y;
            
            _animator.SetFloat(HorizontalBlend, _moveInput.x);
            _animator.SetFloat(VerticalBlend, _moveInput.y);
            _animator.SetFloat(SpeedBlend, 1.0f);
        }
        else
        {
            _animator.SetFloat(SpeedBlend, 0.0f);
        }
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

    /**
     * <summary>Enable the character's movement input. They can still be moved by other sources</summary>
     */
    public void EnableMovement()
    {
        _movementEnabled = true;
        
        // Pull our last registered input value into animator
        if (_moveInput.x != 0.0f || _moveInput.y != 0.0f)
        {
            _lookDirection.x = _moveInput.x;
            _lookDirection.y = _moveInput.y;
            
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
     * <summary>Diasble the character's movement input. They can still be moved by other sources</summary>
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
        DisableMovement();
        velocity = knockbackVector;
        _knockbackTimer = TimerManager.instance.CreateTimer(this, duration, EnableMovement);
    }
}