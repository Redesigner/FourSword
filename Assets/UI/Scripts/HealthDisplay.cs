using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public HealthComponent playerHealth;
    public Sprite hearts;
    public Sprite[] heartContainers;
    
    private Sprite newSprite;

    void Update()
    {
        switch (playerHealth.health)
        {
            case >= 6.0f:
                hearts = heartContainers[6];
                break;
            
            case <= 6.0f and >= 5.0f:
                hearts = heartContainers[5];
                break;
            
            case <= 5.0f and >= 4.0f:
                hearts = heartContainers[4];
                break;
            
            case <= 4.0f and >= 3.0f:
                hearts = heartContainers[3];
                break;
            
            case <= 2.0f and >= 1.0f:
                hearts = heartContainers[2];
                break;
            
            case <= 1.0f and >= 0.0f:
                hearts = heartContainers[1];
                break;
            
            case <= 0.0f:
                hearts = heartContainers[0];
                break;
        }
    }
}

