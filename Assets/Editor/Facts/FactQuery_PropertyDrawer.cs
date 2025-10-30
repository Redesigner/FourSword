using System.Linq;
using Game.Facts;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Editor.Facts
{
    [CustomPropertyDrawer(typeof(FactQuery))]
    public class FactQueryPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var value = (FactQuery)property.boxedValue;
            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, position.width, 18.0f);
            EditorGUI.LabelField(labelRect, label);

            var nameRect = new Rect(position.x, position.y + 18.0f, position.width * 0.4f, position.height - 18.0f);
            var factRegistry = Resources.Load<FactRegistry>("FourSwordFacts");

            var options = factRegistry.facts.Keys.ToArray();
            EditorGUI.Popup(nameRect, 0, options);
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 54.0f;
        }
    }
}