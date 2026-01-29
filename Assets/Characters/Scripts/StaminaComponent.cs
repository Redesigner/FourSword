using Characters.Player.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class StaminaComponent : MonoBehaviour
{
    public Image StaminaBar;

    public float Stamina, MaxStamina;

    public float StabCost, SlashCost, SlamCost;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void staminaUse(SwordStance _currentStance)
    {
        if (_currentStance == _currentStance.Stab(this, ))
        {
            Stamina -= StabCost;
        }
    }
    
    
}
