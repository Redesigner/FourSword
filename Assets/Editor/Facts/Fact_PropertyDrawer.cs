using System.Linq;
using Game.Facts;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Editor.Facts
{
    [CustomPropertyDrawer(typeof(FactVariant))]
    public class FactPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fact = (FactVariant)property.boxedValue;

            EditorGUI.BeginProperty(position, label, property);

            var typeRect = new Rect(position.x, position.y, position.width / 2.0f, position.height);
            var valueRect = new Rect(position.x + position.width / 2.0f, position.y, position.width / 2.0f, position.height);
            
            var oldIndex = (int) fact.type;
            var result = EditorGUI.Popup(typeRect, oldIndex, new[] { "Flag", "Numeric" });

            if (result != oldIndex)
            {
                property.boxedValue = result switch
                {
                    0 => new FactVariant(true),
                    1 => new FactVariant(0),
                    _ => property.boxedValue
                };
            }

            switch (oldIndex)
            {
                case 0:
                {
                    //EditorGUI.ToggleLeft(valueRect, new GUIContent(), fact.Get<bool>());
                    var newValue = EditorGUI.ToggleLeft(valueRect, new GUIContent(), fact.Get<bool>());
                    if (newValue != fact.Get<bool>())
                    {
                        var newFact = new FactVariant(newValue);
                        property.boxedValue = newFact;
                    }
                    break;
                }
                case 1:
                {
                    //EditorGUI.IntField(valueRect, new GUIContent(), fact.Get<int>());
                    EditorGUI.IntField(valueRect, new GUIContent(), 0);
                    break;
                }
            }
            EditorGUI.EndProperty();
        }
    }
}