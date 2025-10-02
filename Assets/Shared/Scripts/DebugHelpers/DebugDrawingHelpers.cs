using UnityEngine;

namespace DebugHelpers
{
    public static class Drawing
    {
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

        // Can only be called from inside OnDrawGizmos!
        public static void DrawBoxCollider2D(BoxCollider2D collider, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawMesh(quadMesh, collider.transform.position + (Vector3)collider.offset, collider.transform.rotation, collider.size * collider.transform.lossyScale);
            var outlineColor = color;
            outlineColor.a += 0.5f;
            
            Gizmos.DrawWireMesh(quadMesh, collider.transform.position + (Vector3)collider.offset, collider.transform.rotation, collider.size * collider.transform.lossyScale);
        }

        public static void DrawCross(Vector3 position, float radius, Color color, float duration)
        {
            Debug.DrawLine(position + new Vector3(radius, radius, 0.0f), position + new Vector3(-radius, -radius, 0.0f), color, duration);
            Debug.DrawLine(position + new Vector3(-radius, radius, 0.0f), position + new Vector3(radius, -radius, 0.0f), color, duration);
        }
    }
}