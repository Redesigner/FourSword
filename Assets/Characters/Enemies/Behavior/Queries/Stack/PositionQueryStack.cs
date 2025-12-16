using System.Collections.Generic;
using System.Linq;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "QS_Stack", menuName = "PositionQuery/Stack/Stack", order = 0)]
    public class PositionQueryStack : ScriptableObject
    {
        [SerializeField] private List<PositionQueryStackEntry> queries = new();

        public List<PositionResult> Evaluate(KinematicCharacterController self, KinematicCharacterController target)
        {
            var result = new List<PositionResult> { new(target.transform.position, 1.0f) };
            foreach (var entry in queries)
            {
                entry.Evaluate(ref result, self, target);
                if (result.Count == 0)
                {
                    return result;
                }
            }

            return result;
        }
    }
}