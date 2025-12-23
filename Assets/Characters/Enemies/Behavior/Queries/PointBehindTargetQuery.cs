using System.Collections.Generic;
using System.Linq;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "Q_PointBehindTarget", menuName = "PositionQuery/PointBehindTarget", order = 0)]
    public class PointBehindTargetQuery : PositionQuery
    {
        [SerializeField] [Min(0.0f)] private float radius;
        [SerializeField] [Min(1)] private int numPoints = 8;
        public override Vector3 RunQuery(GameObject self, KinematicCharacterController target)
        {
            return NavigationHelpers.GetPointInRadiusByDirection(target.transform.position, -target.lookDirection, radius, numPoints);
        }

        public override Vector3 RunQueryWithAllResults(GameObject self, KinematicCharacterController target, out List<PositionResult> results)
        {
            results = NavigationHelpers.GetLocationsInRadius(target.transform.position, radius, numPoints);
            results.RemoveAll(point => !NavigationHelpers.IsLocationInNavMesh(point.position));
            PositionResult.ScorePositionsByVector(-target.lookDirection, results);
            results.Sort((a, b) => -a.score.CompareTo(b.score));

            // PositionResult.DrawScore(points, 0.5f);
            return results.First().position;
        }
    }
}