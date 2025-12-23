using System.Collections.Generic;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "QSE_CanSee", menuName = "PositionQuery/Stack/CanSee")]
    public class CanSeeStackEntry : PositionQueryStackEntry
    {
        public override void Evaluate(ref List<PositionResult> results, KinematicCharacterController self, KinematicCharacterController other)
        {
            results.RemoveAll(result => !CanSee(other.transform.position, result.position));
        }

        private static bool CanSee(Vector3 origin, Vector3 point)
        {
            var previousQueryHitTriggerValue = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = false;
            var result = Physics2D.Linecast(origin, point, LayerMask.GetMask("Default"));
            Physics2D.queriesHitTriggers = previousQueryHitTriggerValue;
            
            return !result;
        }
    }
}