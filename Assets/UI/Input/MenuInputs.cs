using UnityEngine;
using UnityEngine.InputSystem;

public class MenuInputs : MonoBehaviour
{
    public static MenuInputs instance;
    
    public bool MenuOpenCloseInput { get; private set; }
    
    private PlayerInput _playerInput;

    private InputAction _menuOpenCloseAction;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        _playerInput = GetComponent<PlayerInput>();
        _menuOpenCloseAction = _playerInput.actions["MenuOpenClose"];
    }
    
    // Update is called once per frame
    void Update()
    {
        MenuOpenCloseInput = _menuOpenCloseAction.WasPressedThisFrame();
    }
}
