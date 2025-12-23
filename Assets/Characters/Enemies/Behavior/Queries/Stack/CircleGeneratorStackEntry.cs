using System.Collections.Generic;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "QSE_CircleGenerator", menuName = "PositionQuery/Stack/CircleGenerator")]
    public class CircleGeneratorStackEntry : PositionQueryStackEntry
    {
        [SerializeField] [Min(0.0f)] private float radius;
        [SerializeField] [Min(0)] private int numPoints;
        
        public override void Evaluate(ref List<PositionResult> results, KinematicCharacterController self, KinematicCharacterController other)
        {
            var newResults = new List<PositionResult>();
            foreach (var result in results)
            {
                newResults.AddRange(NavigationHelpers.GetLocationsInRadius(result.position, radius, numPoints));
            }

            results = newResults;
        }
    }
}