using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(KinematicCharacterController))]
public class EnemyPathfindingComponent : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private KinematicCharacterController _kinematicObject;
    
    [SerializeField]
    private GameObject target;
    
    private void OnEnable()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _kinematicObject = GetComponent<KinematicCharacterController>();
    }

    public void Start()
    {
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis = false;
        _navMeshAgent.destination = Vector3.zero;
        _navMeshAgent.updatePosition = false;
        // target = GameObject.Find("P_Player");
        _navMeshAgent.isStopped = true;
    }

    public void FixedUpdate()
    {
        if (_navMeshAgent.isStopped)
        {
            return;
        }
        
        _navMeshAgent.nextPosition = _kinematicObject.transform.position;
        _kinematicObject.MoveInput(_navMeshAgent.desiredVelocity);
        Debug.DrawRay(_kinematicObject.gameObject.transform.position, _navMeshAgent.desiredVelocity, Color.red, Time.fixedDeltaTime);
    }
}