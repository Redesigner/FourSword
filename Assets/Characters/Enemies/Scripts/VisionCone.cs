using System;
using System.Collections.Generic;
using Characters.Scripts;
using Unity.Behavior;
using UnityEngine;

namespace Characters.Enemies.Scripts
{
    [Serializable]
    public struct PerceptionEvent
    {
        public GameObject seenObject;
        public KinematicCharacterController controller;
    }

    [Icon("Assets/Editor/Icons/VisionConeIcon.png")]
    public class VisionCone : MonoBehaviour
    {
        [SerializeField] [Min(0.0f)] private float coneRadius = 1.0f;
        [SerializeField] [Range(0.0f, 180.0f)] private float coneHalfAngle = 30.0f;

        [SerializeField] [Range(-180.0f, 180.0f)]
        private float currentAngle;
        
        [SerializeField] private ContactFilter2D visibilityContactFilter;

        [SerializeField] private BehaviorGraphAgent agent;

        [SerializeField] [Range(0.0f, 100.0f)] private float lostSightTime;
        
        private BlackboardVariable<SpottedEnemy> _seenEnemyEventChannel;
        private BlackboardVariable<SpottedEnemy> _lostSightEventChannel;
        
        private readonly HashSet<PerceptionSourceComponent> _seenCharacters = new();

        private TimerHandle _loseSightTimer;

        
#if UNITY_EDITOR
        private Mesh _coneMesh;
        private float _previousConeHalfAngle;
#endif

        private void Start()
        {
            agent.GetVariable("SeenEnemy", out _seenEnemyEventChannel);
            agent.GetVariable("LostSight", out _lostSightEventChannel);
        }

        private void FixedUpdate()
        {
            foreach (var perceptionSource in GameState.instance.perceptionSubsystem.perceptionSources)
            {
                if (CanDetect(perceptionSource.transform.position))
                {
                    if (!_seenCharacters.Add(perceptionSource))
                    {
                        continue;
                    }
                    
                    // Debug.Log("Saw enemy!");
                    _seenEnemyEventChannel.Value.SendEventMessage(perceptionSource.gameObject, perceptionSource.GetComponent<KinematicCharacterController>());
                    _loseSightTimer.Pause();
                }
                else if (_seenCharacters.Remove(perceptionSource))
                {
                    // Debug.Log("Lost sight of enemy... counting down...");
                    TimerManager.instance.CreateOrResetTimer(ref _loseSightTimer, this, lostSightTime, () =>
                    {
                        if (!perceptionSource)
                        {
                            return;
                        }
                        _lostSightEventChannel.Value.SendEventMessage(perceptionSource.gameObject, perceptionSource.GetComponent<KinematicCharacterController>());
                    });
                }
            }
        }

        public void SetLookDirection(float direction)
        {
            currentAngle = direction * Mathf.Rad2Deg;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Compare our cached cone half angle values
            if (_previousConeHalfAngle.Equals(coneHalfAngle)) return;
            
            _previousConeHalfAngle = coneHalfAngle;
            RegenerateConeMesh();
        }

        private void RegenerateConeMesh()
        {
            // Lazily initialize mesh
            if (!_coneMesh)
            {
                _coneMesh = new Mesh();
            }


            var coneHalfAngleRads = coneHalfAngle * Mathf.Deg2Rad;

            // Clear arrays in a set order, so we don't get errors for having out of index vertices or too many normals
            _coneMesh.triangles = new int[] { };
            _coneMesh.normals = new Vector3[] { };
            _coneMesh.vertices = new Vector3[] { };

            // Insert first two vertices, origin and 'top'
            List<Vector3> vertices = new() { Vector3.zero, new Vector3(Mathf.Cos(-coneHalfAngleRads), Mathf.Sin(-coneHalfAngleRads), 0.0f)};
            List<Vector3> normals = new() { Vector3.forward, Vector3.forward };
            List<int> triangles = new();

            // 32 Vertices for a full circle
            const float angleInterval = Mathf.PI / 16.0f;
            var i = 1;
            for (var angle = -coneHalfAngleRads; angle < coneHalfAngleRads; angle += angleInterval)
            {
                vertices.Add(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f));
                normals.Add(Vector3.forward);
                triangles.AddRange(new[]{i + 1, i, 0});
                ++i;
            }
            
            vertices.Add(new Vector3(Mathf.Cos(coneHalfAngleRads), Mathf.Sin(coneHalfAngleRads), 0.0f));
            normals.Add(Vector3.forward);
            triangles.AddRange(new[]{i + 1, i, 0});

            _coneMesh.vertices = vertices.ToArray();
            _coneMesh.triangles = triangles.ToArray();
            _coneMesh.normals = normals.ToArray();
        }
        
        private void OnDrawGizmosSelected()
        {
            var upperAngle = (currentAngle + coneHalfAngle) * Mathf.Deg2Rad;
            var lowerAngle = (currentAngle - coneHalfAngle) * Mathf.Deg2Rad;
            Debug.DrawRay(transform.position, new Vector3(Mathf.Cos(upperAngle), Mathf.Sin(upperAngle), transform.position.z) * coneRadius, Color.red);
            Debug.DrawRay(transform.position, new Vector3(Mathf.Cos(lowerAngle), Mathf.Sin(lowerAngle), transform.position.z) * coneRadius, Color.red);

            if (!_coneMesh)
            {
                return;
            }
            
            Gizmos.color = _seenCharacters.Count > 0 ? new Color(0.0f, 1.0f, 0.0f, 0.25f) : new Color(1.0f, 0.0f, 0.0f, 0.25f);
            Gizmos.DrawMesh(_coneMesh, transform.position, Quaternion.Euler(0.0f, 0.0f, currentAngle), Vector3.one * coneRadius);
            DebugHelpers.Drawing.DrawCircle(transform.position, 1.0f, new Color(1.0f, 0.0f, 0.0f, 0.1f));
        }
#endif

        private bool IsPointInside(Vector2 point)
        {
            var delta = point - (Vector2)transform.position;
            var distSquared = delta.sqrMagnitude;

            if (distSquared > coneRadius * coneRadius)
            {
                return false;
            }

            // This should almost never happen, but it'll catch any divide by zeros
            if (distSquared == 0.0f)
            {
                return true;
            }

            var currentAngleRads = currentAngle * Mathf.Deg2Rad;
            var coneForwardVector = new Vector2(Mathf.Cos(currentAngleRads), Mathf.Sin(currentAngleRads));
            var deltaVector = delta / Mathf.Sqrt(distSquared);

            var cosBetween = Vector2.Dot(deltaVector, coneForwardVector);
            var coneHalfAngleCos = Mathf.Cos(coneHalfAngle * Mathf.Deg2Rad);
            return cosBetween > coneHalfAngleCos;
        }

        private bool CanSeePoint(Vector2 point)
        {
            var previousQueryHitTriggerValue = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = false;
            var result = Physics2D.Linecast(transform.position, point, LayerMask.GetMask("Default"));
            Physics2D.queriesHitTriggers = previousQueryHitTriggerValue;
            
            Debug.DrawLine(transform.position, result ? result.centroid : point, Color.red, Time.fixedDeltaTime);
            
            return !result;
        }

        private bool CanDetect(Vector2 point)
        {
            return IsPointInside(point) && CanSeePoint(point);
        }
    }
}