using System;
using UImGui;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player.Scripts
{
    public class UIController : MonoBehaviour
    {
        public void Pause(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }
            
            GameState.instance.TogglePause();
        }
        private void Awake()
        {
            UImGuiUtility.Layout += OnLayout;
            UImGuiUtility.OnInitialize += OnInitialize;
            UImGuiUtility.OnDeinitialize += OnDeinitialize;
        }
        
        private void OnLayout(UImGui.UImGui obj)
        {
        }
        
        private void OnInitialize(UImGui.UImGui obj)
        {
        }        

        private void OnDeinitialize(UImGui.UImGui obj)
        {
        }
    }
}