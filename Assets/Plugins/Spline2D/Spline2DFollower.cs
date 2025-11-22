using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Plugins.Spline2D
{
    public class Spline2DFollower : MonoBehaviour
    {
        [SerializeField] private Spline2DComponent targetSpline;

        private float _currentPosition;
        
        private void FixedUpdate()
        {
            _currentPosition += Time.fixedDeltaTime;
            if (_currentPosition > targetSpline.length)
            {
                _currentPosition -= targetSpline.length;
            }
            
            gameObject.transform.position = targetSpline.InterpolateWorldSpace(targetSpline.DistanceToLinearT(_currentPosition));
        }
    }
}