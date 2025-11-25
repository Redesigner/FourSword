using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "Q_ClosestPoint", menuName = "PositionQuery/ClosestPoint", order = 0)]
    public class ClosestPointQuery : PositionQuery
    {
        [SerializeField] [Min(0.0f)] private float radius;
        [SerializeField] [Min(1)] private int numPoints = 8;
        public override Vector3 RunQuery(GameObject self, KinematicCharacterController target)
        {
            return NavigationHelpers.GetClosestPointAroundRadius(self.transform.position, target.transform.position,
                radius, numPoints);
        }
    }
}