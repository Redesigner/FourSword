using System.Collections.Generic;
using UnityEngine;

namespace Characters.Enemies.Scripts
{
    public class VisionCone : MonoBehaviour
    {
        [SerializeField] [Min(0.0f)] private float coneRadius = 1.0f;
        [SerializeField] [Range(0.0f, 180.0f)] private float coneHalfAngle = 30.0f;

        [SerializeField] [Range(-180.0f, 180.0f)]
        private float currentAngle;
        
        [SerializeField] private ContactFilter2D visibilityContactFilter;

        
        #if UNITY_EDITOR
        private Mesh _coneMesh;

        private float _previousConeHalfAngle;
        #endif

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
        
        private void OnDrawGizmos()
        {
            var upperAngle = (currentAngle + coneHalfAngle) * Mathf.Deg2Rad;
            var lowerAngle = (currentAngle - coneHalfAngle) * Mathf.Deg2Rad;
            Debug.DrawRay(transform.position, new Vector3(Mathf.Cos(upperAngle), Mathf.Sin(upperAngle), transform.position.z) * coneRadius, Color.red);
            Debug.DrawRay(transform.position, new Vector3(Mathf.Cos(lowerAngle), Mathf.Sin(lowerAngle), transform.position.z) * coneRadius, Color.red);

            if (!_coneMesh)
            {
                return;
            }

            var mouseInCone = false;
            if (Camera.main)
            {
                Vector2 mousePositionWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (IsPointInside(mousePositionWorld))
                {
                    mouseInCone = CanSeePoint(mousePositionWorld, out var furthestSeenPoint);
                    Debug.DrawLine(transform.position, furthestSeenPoint, Color.red);
                }
            }
            
            Gizmos.color = mouseInCone ? new Color(0.0f, 1.0f, 0.0f, 0.25f) : new Color(1.0f, 0.0f, 0.0f, 0.25f);
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

        private bool CanSeePoint(Vector2 point, out Vector2 visiblePoint)
        {
            var result = Physics2D.Linecast(transform.position, point, LayerMask.GetMask("Default"));
            if (result)
            {
                visiblePoint = result.point;
                return false;
            }

            visiblePoint = point;
            return true;
        }
    }
}