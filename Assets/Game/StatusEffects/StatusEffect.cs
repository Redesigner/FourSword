using UnityEngine;

namespace Game.StatusEffects
{
    [CreateAssetMenu(fileName = "New Status Effect Definition", menuName = "Status Effect", order = 0)]
    public class StatusEffect : ScriptableObject
    {
        [SerializeField] public string effectName;
    }
}