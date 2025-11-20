using System;
using Unity.Behavior;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(KinematicCharacterController))]
[RequireComponent(typeof(BehaviorGraphAgent))]
public class EnemyPathfindingComponent : MonoBehaviour
{

    enum PathFollowingMode
    {
        None,   // Not following any path
        Target, // Following the target, usually via destinationAlias
        Spline, // Moving along a spline
    }
    
    private NavMeshAgent _navMeshAgent;
    private KinematicCharacterController _kinematicObject;
    private GameObject _destinationAlias;
    private BehaviorGraphAgent _behaviorGraphAgent;
    
    // Spline following variables, might move this to a separate component
    [SerializeField]
    private Spline2DComponent _targetSpline;
    private int _splineTargetPointIndex;
    private Vector2 _splineTargetPosition;
    private bool _onPath;
    private PathFollowingMode _pathFollowingMode;
    
    /** <summary>
     * How close to the point on the spline the agent should be before moving to the next point
     * </summary>
     */
    [SerializeField] [Range(0.0f, 1.0f)] private float splineFollowingPrecision = 0.1f;

    /** <summary>
     * Should the agent path along the spline using the navmesh, or just go directly to the next point.
     * </summary>
     */
    [SerializeField] private bool pathfindOnSpline = false;
    
    private void OnEnable()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _kinematicObject = GetComponent<KinematicCharacterController>();
        _destinationAlias = new GameObject(gameObject.name + "_DA");

        _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        _behaviorGraphAgent.SetVariableValue("DestinationAlias", _destinationAlias.transform);
    }

    private void OnDisable()
    {
        Destroy(_destinationAlias);
    }

    public void Start()
    {
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis = false;
        _navMeshAgent.destination = Vector3.zero;
        _navMeshAgent.updatePosition = false;
        // target = GameObject.Find("P_Player");
        _navMeshAgent.isStopped = true;

        if (_targetSpline)
        {
            MoveToSpline(_targetSpline);
        }
    }

    public void FixedUpdate()
    {
        switch (_pathFollowingMode)
        {
            default:
            case PathFollowingMode.None:
            {
                return;
            }
            case PathFollowingMode.Target:
            {
                FollowNavMesh();
                return;
            }
            
            case PathFollowingMode.Spline:
            {
                FollowSpline();
                return;
            }
        }
    }

    public void MoveToSpline(Spline2DComponent splineComponent)
    {
        _pathFollowingMode = PathFollowingMode.Spline;
        BeginMoveToSpline(splineComponent);
    }

    private void OnDrawGizmos()
    {
        if (!_kinematicObject || !_navMeshAgent)
        {
            return;
        }
        
        var position = _kinematicObject.gameObject.transform.position;
        DebugHelpers.Drawing.DrawArrow(position, position + _navMeshAgent.desiredVelocity, Color.red, Time.deltaTime);
    }

    // ReSharper disable Unity.PerformanceAnalysis -- Rider is flagging this as expensive for some reason?
    private void FollowNavMesh()
    {
        if (_navMeshAgent.isStopped)
        {
            return;
        }
        
        _navMeshAgent.nextPosition = _kinematicObject.transform.position;
        _kinematicObject.MoveInput(_navMeshAgent.desiredVelocity);
    }
    
    // ReSharper disable Unity.PerformanceAnalysis -- Rider is flagging this as expensive for some reason?
    private void FollowSpline()
    {
        if (_onPath)
        {
            var delta = _splineTargetPosition - (Vector2)transform.position;
            var distanceSquared = delta.sqrMagnitude;
            if (distanceSquared < splineFollowingPrecision * splineFollowingPrecision)
            {
                _splineTargetPointIndex = _targetSpline.GetNextSubdividedIndex(_splineTargetPointIndex);
                _splineTargetPosition = _targetSpline.GetSubdividedPointWorldSpace(_splineTargetPointIndex);
            }
            
            _kinematicObject.MoveInput(delta.normalized);
        }
        else
        {
            var distanceSquared = (_splineTargetPosition - (Vector2)transform.position).sqrMagnitude;
            if (distanceSquared < splineFollowingPrecision * splineFollowingPrecision)
            {
                _onPath = true;
                return;
            }
            
            FollowNavMesh();
        }
    }

    private void BeginMoveToSpline(Spline2DComponent spline2DComponent)
    {
        _targetSpline = spline2DComponent;
        _splineTargetPosition = spline2DComponent.GetClosestPoint(transform.position, out _splineTargetPointIndex);
        _navMeshAgent.SetDestination(_splineTargetPosition);
        _navMeshAgent.isStopped = false;
        _onPath = false;
    }
}