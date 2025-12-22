using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Shared
{
    public static class Math
    {
        public static float RoundTo(float value, int interval)
        {
            return (float)System.Math.Round(value / (float) interval) * interval;
        }

        public static Vector2 ClampDirectionVector(Vector2 direction)
        {
            return Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ?
                new Vector2(Mathf.Sign(direction.x), 0.0f) :
                new Vector2(0.0f, Mathf.Sign(direction.y));
        }

        public static Vector2 ClampVectorLength(Vector2 vector, float length)
        {
            var squareMagnitude = vector.sqrMagnitude;
            return squareMagnitude > length * length ? vector / Mathf.Sqrt(squareMagnitude) * length : vector;
        }

        public static Vector2 RandomPointInRadius(float radius)
        {
            var theta = Random.value * 2.0f * Mathf.PI;
            var outRadius = radius * Mathf.Sqrt(Random.value);

            return new Vector2(Mathf.Cos(theta) * outRadius, Mathf.Sin(theta) * outRadius);
        }
    }
}