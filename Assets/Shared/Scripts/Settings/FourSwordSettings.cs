using Game.StatusEffects;
using UnityEditor;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "FourSwordSettings", menuName = "FourSwordSettings", order = 0)]
    public class FourSwordSettings : ScriptableObject
    {
        private const string SettingsDefaultPath = "Assets/Resources/FourSwordSettings.asset";

        [SerializeField] public StatusEffectList statusEffects;

        public static SerializedObject GetSerializedSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<FourSwordSettings>(SettingsDefaultPath);
            if (settings != null)
            {
                return new SerializedObject(settings);
            }
            
            settings = CreateInstance<FourSwordSettings>();
            AssetDatabase.CreateAsset(settings, SettingsDefaultPath);
            AssetDatabase.SaveAssets();

            return new SerializedObject(settings);
        }
    }
}