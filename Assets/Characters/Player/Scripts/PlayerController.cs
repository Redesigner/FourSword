using System;
using Props.Rooms.Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Characters.Player.Scripts
{
    [RequireComponent(typeof(KinematicCharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float endTransitionThreshold = 0.2f;
        [SerializeField][Min(0.0f)] private float forcedMoveSpeed = 0.7f;

        public UnityEvent forcedDestinationReached;
        
        private KinematicCharacterController _characterController;
        private Vector2 _forcedDestination;
        private bool _movingToDestination;
        
        private void Awake()
        {
            _characterController = GetComponent<KinematicCharacterController>();
        }

        private void Start()
        {
            GameState.instance.RegisterPlayer(this);
        }

        public void SetDestination(Vector2 destination)
        {
            _forcedDestination = destination;
            _movingToDestination = true;
            _characterController.DisableMovement();
        }

        private void FixedUpdate()
        {
            if (!_movingToDestination)
            {
                return;
            }
            
            var position2D = new Vector2(transform.position.x, transform.position.y);
            var delta = _forcedDestination - position2D;
            var deltaSquareMagnitude = delta.sqrMagnitude;
            if (!(deltaSquareMagnitude < endTransitionThreshold * endTransitionThreshold))
            {
                // Don't use normalize here because we've already calculated the square magnitude
                // It's not really important, but saves redoing math we've already done
                var magnitude = (float)Math.Sqrt(deltaSquareMagnitude);
                var direction = new Vector2(delta.x / magnitude, delta.y / magnitude);
                _characterController.transform.position += (Vector3)(direction * (Time.fixedDeltaTime * forcedMoveSpeed));
                return;
            }
            
            _movingToDestination = false;
            _characterController.EnableMovement();
            forcedDestinationReached.Invoke();
        }
    }
}