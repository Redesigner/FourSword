using System;
using UnityEngine;

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
    }
}