using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters.Enemies.Scripts
{
    public class VisionCone : MonoBehaviour
    {
        [SerializeField] [Min(0.0f)] float coneRadius = 1.0f;
        [SerializeField] [Range(0.0f, 180.0f)] private float coneHalfAngle = 30.0f;

        [field: SerializeField] [Range(-180.0f, 180.0f)]
        private float currentAngle = 0.0f;
        
        #if UNITY_EDITOR
        private Mesh _coneMesh;

        private float _previousConeHalfAngle;
        #endif

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_previousConeHalfAngle.Equals(coneHalfAngle))
            {
                return;
            }

            if (!_coneMesh)
            {
                _coneMesh = new Mesh();
            }

            _previousConeHalfAngle = coneHalfAngle;

            var coneHalfAngleRads = coneHalfAngle * Mathf.Deg2Rad;

            _coneMesh.triangles = new int[] { };
            _coneMesh.normals = new Vector3[] { };
            _coneMesh.vertices = new Vector3[] { };

            List<Vector3> vertices = new() { Vector3.zero, new Vector3(Mathf.Cos(-coneHalfAngleRads), Mathf.Sin(-coneHalfAngleRads), 0.0f)};
            List<Vector3> normals = new() { Vector3.forward, Vector3.forward };
            List<int> triangles = new();

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

            if (_coneMesh)
            {
                Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.25f);
                Gizmos.DrawMesh(_coneMesh, transform.position, Quaternion.Euler(0.0f, 0.0f, currentAngle), Vector3.one * coneRadius);
            }
        }
        #endif
    }
}