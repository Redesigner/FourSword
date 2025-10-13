using System;
using UnityEngine;

namespace Characters.Scripts
{
    public class PerceptionSourceComponent : MonoBehaviour
    {
        private void OnEnable()
        {
            GameState.instance.perceptionSubsystem.RegisterPerceptionSource(this);
        }

        private void OnDisable()
        {
            GameState.instance.perceptionSubsystem.UnregisterPerceptionSource(this);
        }
    }
}