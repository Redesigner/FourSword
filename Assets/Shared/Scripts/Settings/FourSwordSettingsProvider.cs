using UnityEditor;
using UnityEngine.UIElements;

namespace Settings
{
    public class FourSwordSettingsProvider : SettingsProvider
    {
        private SerializedObject _settings;

        private FourSwordSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            :base(path, scope)
        {}

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = FourSwordSettings.GetSerializedSettings();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new FourSwordSettingsProvider("Project/Four Sword Settings", SettingsScope.Project);
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.PropertyField(_settings.FindProperty("statusEffects"));
            _settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}