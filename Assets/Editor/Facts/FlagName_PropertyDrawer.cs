using System;
using System.Linq;
using Game.Facts;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Editor.Facts
{
    [CustomPropertyDrawer(typeof(FlagName))]
    public class FlagNamePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var flagName = (FlagName)property.boxedValue;
            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, position.width, 18.0f);
            EditorGUI.LabelField(labelRect, label);

            var factRegistry = Resources.Load<FactRegistry>(FactRegistry.DefaultFactRegistryPath);
            
            
            // Name dropdown
            var nameRect = new Rect(position.x, position.y + 18.0f, position.width, position.height);
            // Only select flags here
            var options = factRegistry.facts.Where(fact => fact.data.type == FactType.Flag).Select(fact => fact.name).ToArray();
            var currentIndex = Array.FindIndex(options, option => option.Equals(flagName.name));

            var validIndex = currentIndex >= 0;
            if (!validIndex)
            {
                currentIndex = 0;
            }

            var selectedIndex = EditorGUI.Popup(nameRect, currentIndex, options);
            if (selectedIndex != currentIndex || !validIndex)
            {
                flagName.name = options[selectedIndex];
                property.boxedValue = flagName;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + 18.0f;
        }
    }
}