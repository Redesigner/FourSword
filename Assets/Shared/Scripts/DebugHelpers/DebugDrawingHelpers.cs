using UnityEngine;

namespace DebugHelpers
{
    public static class Drawing
    {
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
    }
}