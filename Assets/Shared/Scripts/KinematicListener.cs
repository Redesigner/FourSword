using UnityEngine;

namespace Kinematics
{
    public class KinematicListener : MonoBehaviour
    {
        private Rigidbody2D _body;
    
        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        public virtual void OnHit(KinematicCharacterController source, RaycastHit2D hit)
        {
            _body.position += hit.normal * -0.01f;
        }
    }
}