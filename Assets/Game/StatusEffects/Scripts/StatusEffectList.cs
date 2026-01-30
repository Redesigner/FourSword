using UnityEngine;

namespace Game.StatusEffects
{
    [CreateAssetMenu(fileName = "StatusEffectList", menuName = "StatusEffects/List", order = 0)]
    public class StatusEffectList : ScriptableObject
    {
        [field: SerializeField] public StatusEffect stunEffect { private set; get; }
        [field: SerializeField] public StatusEffect speedEffect { private set; get; }
        [field: SerializeField] public StatusEffect invulnerabilityEffect { private set; get; }
        [field: SerializeField] public StatusEffect stabReachEffect { private set; get; }
        [field: SerializeField] public StatusEffect staminaRegenRateEffect { private set; get; }
    }
}