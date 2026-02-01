using System;
using UnityEngine;

namespace DebugHelpers
{
    public static class Drawing
    {
        // Lazy initialized mesh
        private static Mesh _quadMesh;
        private static Mesh quadMesh
        {
            get
            {
                if (!_quadMesh)
                {
                    _quadMesh = new Mesh
                    {
                        vertices = new Vector3[]
                        {
                            new(-0.5f, 0.5f, 0.0f),
                            new(0.5f, 0.5f, 0.0f),
                            new(0.5f, -0.5f, 0.0f),
                            new(-0.5f, -0.5f, 0.0f)
                        },
                        normals = new[]
                        {
                            Vector3.forward,
                            Vector3.forward,
                            Vector3.forward,
                            Vector3.forward
                        },
                        triangles = new[] { 0, 1, 2, 2, 3, 0 }
                    };
                }
                return _quadMesh;
            }
        }

        // Lazy initialized mesh
        private static Mesh _circleMesh;
        private static Mesh circleMesh
        {
            get
            {
                if (!_circleMesh)
                {
                    _circleMesh = MakeCircleMesh(32);
                }

                return _circleMesh;
            }

            set => _circleMesh = value;
        }
        
        private const float ArrowheadAngleOffsetRads = 2.5f;
        public static void DrawArrow(Vector3 position, Vector2 direction, float length, Color color, float duration)
        {
            var arrowEnd = position;
            arrowEnd.x += direction.x * length;
            arrowEnd.y += direction.y * length;
            
            const float arrowheadLength = 0.2f;

            var arrowDirection = Mathf.Atan2(direction.y, direction.x);
            var arrowRight = arrowEnd;
            var arrowLeft = arrowEnd;
            arrowRight.x += Mathf.Cos(arrowDirection + ArrowheadAngleOffsetRads) * arrowheadLength;
            arrowRight.y += Mathf.Sin(arrowDirection + ArrowheadAngleOffsetRads) * arrowheadLength;
            arrowLeft.x  += Mathf.Cos(arrowDirection - ArrowheadAngleOffsetRads) * arrowheadLength;
            arrowLeft.y  += Mathf.Sin(arrowDirection - ArrowheadAngleOffsetRads) * arrowheadLength;
            
            Debug.DrawLine(position, arrowEnd, color, duration);
            Debug.DrawLine(arrowEnd, arrowRight, color, duration);
            Debug.DrawLine(arrowEnd, arrowLeft, color, duration);
        }
        
        public static void DrawArrow(Vector3 start, Vector3 end, Color color, float duration)
        {
            const float arrowheadLength = 0.2f;

            var arrowDirection = Mathf.Atan2(end.y - start.y, end.x - start.x);
            var arrowRight = end;
            var arrowLeft = end;
            arrowRight.x += Mathf.Cos(arrowDirection + ArrowheadAngleOffsetRads) * arrowheadLength;
            arrowRight.y += Mathf.Sin(arrowDirection + ArrowheadAngleOffsetRads) * arrowheadLength;
            arrowLeft.x  += Mathf.Cos(arrowDirection - ArrowheadAngleOffsetRads) * arrowheadLength;
            arrowLeft.y  += Mathf.Sin(arrowDirection - ArrowheadAngleOffsetRads) * arrowheadLength;
            
            Debug.DrawLine(start, end, color, duration);
            Debug.DrawLine(end, arrowRight, color, duration);
            Debug.DrawLine(end, arrowLeft, color, duration);
        }
        
        public static void DrawArrow(Vector3 start, Vector3 end, Color color)
        {
            const float arrowheadLength = 0.2f;

            var arrowDirection = Mathf.Atan2(end.y - start.y, end.x - start.x);
            var arrowRight = end;
            var arrowLeft = end;
            arrowRight.x += Mathf.Cos(arrowDirection + ArrowheadAngleOffsetRads) * arrowheadLength;
            arrowRight.y += Mathf.Sin(arrowDirection + ArrowheadAngleOffsetRads) * arrowheadLength;
            arrowLeft.x  += Mathf.Cos(arrowDirection - ArrowheadAngleOffsetRads) * arrowheadLength;
            arrowLeft.y  += Mathf.Sin(arrowDirection - ArrowheadAngleOffsetRads) * arrowheadLength;

            Gizmos.color = color;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(end, arrowRight);
            Gizmos.DrawLine(end, arrowLeft);
        }

        // Can only be called from inside OnDrawGizmos!
        public static void DrawBoxCollider2D(BoxCollider2D collider, Color color)
        {
            Gizmos.color = color;
            var position = collider.transform.position + collider.transform.rotation * collider.offset;
            Gizmos.DrawMesh(quadMesh, position, collider.transform.rotation, collider.size * collider.transform.lossyScale);
            var outlineColor = color;
            outlineColor.a += 0.5f;
            
            //Gizmos.DrawWireMesh(quadMesh, collider.transform.position + (Vector3)collider.offset, collider.transform.rotation, collider.size * collider.transform.lossyScale);
            DrawLineStrip(quadMesh.vertices, position, collider.transform.rotation, collider.size * collider.transform.lossyScale);
        }

        public static void DrawBox(Vector3 position, Vector2 extents, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawMesh(quadMesh, position, Quaternion.identity, extents);
            
            var outlineColor = color;
            outlineColor.a += 0.5f;
            Gizmos.color = outlineColor;
            DrawLineStrip(quadMesh.vertices, position, Quaternion.identity, extents);

        }

        public static void DrawCross(Vector3 position, float radius, Color color, float duration)
        {
            Debug.DrawLine(position + new Vector3(radius, radius, 0.0f), position + new Vector3(-radius, -radius, 0.0f), color, duration);
            Debug.DrawLine(position + new Vector3(-radius, radius, 0.0f), position + new Vector3(radius, -radius, 0.0f), color, duration);
        }

        public static void DrawCircle(Vector3 position, float radius, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawMesh(circleMesh, position, Quaternion.identity, Vector3.one * radius);
            var solidColor = color;
            solidColor.a += 0.5f;
            Gizmos.color = solidColor;
            DrawLineStrip(circleMesh.vertices, position, Quaternion.identity, Vector3.one * radius);
        }

        public static void DrawLineStrip(Vector3[] vertices, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var verticesTransformed = new Vector3[vertices.Length];
            var transform = Matrix4x4.TRS(position, rotation, scale);
            for (var i = 0; i < vertices.Length; ++i)
            {
                verticesTransformed[i] = transform.MultiplyPoint(vertices[i]);
            }
            
            Gizmos.DrawLineStrip(verticesTransformed, true);
        }

        private static Mesh MakeCircleMesh(int points)
        {
            if (points <= 2)
            {
                return null;
            }

            var vertices = new Vector3[points];
            var normals = new Vector3[points];
            var triangles = new int[(points - 2) * 3];

            var angleInterval = Mathf.PI * 2.0f / (points);

            for (var i = 0; i < points; ++i)
            {
                vertices[i] = new Vector3(Mathf.Cos(angleInterval * i), Mathf.Sin(angleInterval * i), 0.0f);
                normals[i] = Vector3.forward;
            }

            for (var i = 0; i < points - 2; ++i)
            {
                var realIndex = i * 3;
                triangles[realIndex] = 0;
                triangles[realIndex + 1] = i + 2;
                triangles[realIndex + 2] = i + 1;
            }

            return new Mesh()
            {
                vertices = vertices,
                normals = normals,
                triangles = triangles
            };
        }
    }

    public static class Names
    {
        public static string GetNameSafe(GameObject obj)
        {
            return obj ? obj.name : "None";
        }

        public static string GetNameSafe(MonoBehaviour obj)
        {
            return obj ? obj.name : "None";
        }
    }
}