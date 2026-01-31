using System;
using UnityEngine;

public class StaminaDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform staminaBarRect;

    private float _initialScale;
    private void Awake()
    {
        _initialScale = staminaBarRect.localScale.x;
    }

    public void SetStaminaValues(float stamina, float maxStamina)
    {
        var scale = staminaBarRect.transform.localScale;
        scale.x = _initialScale * stamina / maxStamina;
        staminaBarRect.transform.localScale = scale;
    }
}
