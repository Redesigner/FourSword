using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public HealthComponent playerHealth;
    
    [SerializeField] public Sprite hearts;
    [SerializeField] public Sprite[] heartContainers;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = hearts;
    }

    void Update()
    {
        switch (playerHealth.health)
        {
            case >= 6.0f:
                spriteRenderer.sprite = heartContainers[6];
                return;
            
            case <= 6.0f and > 5.0f:
                spriteRenderer.sprite = heartContainers[5];
                return;
            
            case <= 5.0f and > 4.0f:
                spriteRenderer.sprite = heartContainers[4];
                return;
            
            case <= 4.0f and > 3.0f:
                spriteRenderer.sprite = heartContainers[3];
                return;
            
            case <= 2.0f and > 1.0f:
                spriteRenderer.sprite = heartContainers[2];
                return;
            
            case <= 1.0f and > 0.0f:
                spriteRenderer.sprite = heartContainers[1];
                return;
            
            case <= 0.0f:
                spriteRenderer.sprite = heartContainers[0];
                return;
        }
    }
}

