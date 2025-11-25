using System;

namespace Shared
{
    public static class Math
    {
        public static float RoundTo(float value, int interval)
        {
            return (float)System.Math.Round(value / (float) interval) * interval;
        }
    }
}