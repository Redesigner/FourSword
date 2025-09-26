using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters.Enemies.Scripts
{
    public class PositionResult
    {
        public Vector3 position;
        public float score = 0.0f;

        public PositionResult(Vector3 position, float score)
        {
            this.position = position;
            this.score = score;
        }

        public static void ScorePositionsByDistanceFromTarget(Vector3 target, List<PositionResult> positions)
        {
            var positionCount = positions.Count;
            var distancesSquared = new List<float>(positionCount);
            distancesSquared.AddRange(positions.Select(position => (position.position - target).sqrMagnitude));
            var min = distancesSquared.Min();
            var max = distancesSquared.Max();
            var delta = max - min;
            // Avoid divide by 0
            var rangeFactor = delta == 0.0f ? 1.0f : 1.0f / delta;
            for (var i = 0; i < positionCount; ++i)
            {
                positions[i].score = 1.0f - (distancesSquared[i] - min) * rangeFactor;
            }
        }
    }
}