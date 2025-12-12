using System;
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
            if (positions.Count == 0)
            {
                return;
            }
            
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
            
            DrawScore(positions, 1.0f);
        }

        public static void ScorePositionsByVector(Vector3 direction, List<PositionResult> positions)
        {
            var max = float.NegativeInfinity;
            var min = float.PositiveInfinity;
            
            foreach (var positionResult in positions)
            {
                positionResult.score = Vector3.Dot(direction, positionResult.position);
                max = Mathf.Max(max, positionResult.score);
                min = Mathf.Min(min, positionResult.score);
            }

            var factor = 1.0f / (max - min);
            foreach (var positionResult in positions)
            {
                positionResult.score = (positionResult.score - min) * factor;
            }
            
            DrawScore(positions, 1.0f);
        }

        public static void DrawScore(List<PositionResult> positions, float duration)
        {
            if (!GameState.instance.settings.showPositionScore)
            {
                return;
            }
            
            foreach (var position in positions)
            {
                DebugHelpers.Drawing.DrawCross(position.position, 0.25f, new Color(position.score, 0.0f, 0.0f), duration);
            }
        }
    }
}