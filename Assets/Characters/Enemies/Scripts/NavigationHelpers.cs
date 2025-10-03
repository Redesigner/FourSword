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

        /** <summary>
         * Get the closest point to the agent in a set radius, counting only points on the navmesh
         * </summary>
         * <param name="agentPosition">Position of the agent - finds closet valid point to this</param>
         * <param name="center">Position to generate points around</param>
         * <param name="radius">Length of radius, in units</param>
         * <param name="numLocations">Number of test points to generate</param>
         * <returns>Closest point that is on the navmesh</returns>
         */ 
        public static Vector3 GetClosestPointAroundRadius(Vector3 agentPosition, Vector3 center, float radius, int numLocations)
        {
            var points = GetLocationsInRadius(center, radius, numLocations);
            points.RemoveAll(point => !IsLocationInNavMesh(point.position));
            PositionResult.ScorePositionsByDistanceFromTarget(agentPosition, points);
            points.Sort((a, b) => -a.score.CompareTo(b.score));
            /*foreach (var point in points)
            {
                Debug.DrawRay(point.position - new Vector3(0.25f, 0.0f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), new Color(point.score, 0.0f, 0.0f, 1.0f), 0.5f);
                Debug.DrawRay(point.position - new Vector3(0.0f, 0.25f, 0.0f), new Vector3(0.0f, 0.5f, 0.0f), new Color(point.score, 0.0f, 0.0f, 1.0f), 0.5f);
            }*/
            //Debug.DrawRay(points.First().position - new Vector3(0.25f, 0.0f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), Color.red);
            //Debug.DrawRay(points.First().position - new Vector3(0.0f, 0.25f, 0.0f), new Vector3(0.0f, 0.5f, 0.0f), Color.red);

            PositionResult.DrawScore(points, 0.5f);

            return points.First().position;
        }
        
        /** <summary>
         * Get a valid point on the navmesh, that is furthest along some vector
         * </summary>
         * <param name="center">Position to generate points around</param>
         * <param name="direction">Direction to score points by. Expected to be already normalized.</param>
         * <param name="radius">Length of radius, in units</param>
         * <param name="numLocations">Number of test points to generate</param>
         * <returns>Point most along vector that is also on the navmesh</returns>
         */
        public static Vector3 GetPointInRadiusByDirection(Vector3 center, Vector3 direction, float radius, int numLocations)
        {
            var points = GetLocationsInRadius(center, radius, numLocations);
            points.RemoveAll(point => !IsLocationInNavMesh(point.position));
            PositionResult.ScorePositionsByVector(direction, points);
            points.Sort((a, b) => -a.score.CompareTo(b.score));

            PositionResult.DrawScore(points, 0.5f);
            return points.First().position;
        }
    }
}