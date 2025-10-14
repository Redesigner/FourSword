using UnityEngine;

namespace Game.StatusEffects
{
    public enum EffectAccumulator
    {
        None,
        Additive,
        Multiplicative,
        Maximum,
        Minimum
    }
    
    [CreateAssetMenu(fileName = "New Status Effect Definition", menuName = "StatusEffects/StatusEffect", order = 0)]
    public class StatusEffect : ScriptableObject
    {
        [SerializeField] public string effectName;
        [SerializeField] public EffectAccumulator accumulator;
    }
}