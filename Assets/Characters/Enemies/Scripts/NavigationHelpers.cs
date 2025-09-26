using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Enemies.Scripts
{
    public abstract class NavigationHelpers
    {
        public static bool IsLocationInNavMesh(Vector3 location)
        {
            return NavMesh.SamplePosition(location, out var hit, 0.1f, 1 << NavMesh.GetAreaFromName("Walkable"));
        }

        public static List<PositionResult> GetLocationsInRadius(Vector3 center, float radius, int numLocations)
        {
            var result = new List<PositionResult>(numLocations);
            var interval = Mathf.PI * 2.0f / numLocations;
            for (var i = 0; i < numLocations; ++i)
            {
                var newPosition = center;
                newPosition.x += Mathf.Cos(interval * i) * radius;
                newPosition.y += Mathf.Sin(interval * i) * radius;
                result.Add(new PositionResult(newPosition, 1.0f));
            }

            return result;
        }

        public static Vector3 GetClosestPointAroundRadius(Vector3 agentPosition, Vector3 center, float radius, int numLocations)
        {
            var points = GetLocationsInRadius(center, radius, numLocations);
            points.RemoveAll(point => !IsLocationInNavMesh(point.position));
            PositionResult.ScorePositionsByDistanceFromTarget(agentPosition, points);
            points.Sort((a, b) => -a.score.CompareTo(b.score));
            foreach (var point in points)
            {
                Debug.DrawRay(point.position - new Vector3(0.25f, 0.0f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), new Color(point.score, 0.0f, 0.0f, 1.0f), 0.5f);
                Debug.DrawRay(point.position - new Vector3(0.0f, 0.25f, 0.0f), new Vector3(0.0f, 0.5f, 0.0f), new Color(point.score, 0.0f, 0.0f, 1.0f), 0.5f);
            }
            //Debug.DrawRay(points.First().position - new Vector3(0.25f, 0.0f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), Color.red);
            //Debug.DrawRay(points.First().position - new Vector3(0.0f, 0.25f, 0.0f), new Vector3(0.0f, 0.5f, 0.0f), Color.red);

            return points.First().position;
        }
    }
}