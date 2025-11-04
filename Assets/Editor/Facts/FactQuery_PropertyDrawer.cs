using System;
using System.Collections.Generic;
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
        private static List<string> _flagOptionNames = new() { "True", "False" };
        private static List<string> _numericOptionNames = new() { "Equal", "Greater", "Less" };
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var factDirty = false;
            
            var factQuery = (FactQuery)property.boxedValue;
            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, position.width, 18.0f);
            EditorGUI.LabelField(labelRect, label);
            
            var factRegistry = Resources.Load<FactRegistry>("FourSwordFacts");
            // Early out if we can't find our fact in the registry
            // because the other logic won't work without it
            if (!factRegistry.facts.TryGetValue(factQuery.factName, out var referenceFact))
            {
                // Default to the first fact
                if (factRegistry.facts.Count > 0)
                {
                    factQuery.factName = factRegistry.facts.Keys.First();
                    property.boxedValue = factQuery;
                }
                EditorGUI.EndProperty();
                return;
            }
            
            // Name dropdown
            var nameRect = new Rect(position.x, position.y + 18.0f, position.width * 0.4f, position.height - 18.0f);
            var options = factRegistry.facts.Keys.ToArray();
            var currentIndex = Array.FindIndex(options, option => option.Equals(factQuery.factName));
            if (currentIndex == -1)
            {
                currentIndex = 0;
            }
            var selectedIndex = EditorGUI.Popup(nameRect, currentIndex, options);
            if (selectedIndex != currentIndex)
            {
                factQuery.factName = options[selectedIndex];
                factDirty = true;
            }
            
            // Comparator dropdown
            var comparatorRect = new Rect(position.x + position.width * 0.4f, position.y + 18.0f, position.width * 0.2f,
                position.height - 18.0f);
            var currentComparatorIndex = GetComparatorIndex(referenceFact.type, factQuery);
            var selectedComparatorIndex = EditorGUI.Popup(comparatorRect, currentComparatorIndex,
                GetComparatorOptionsList(referenceFact.type).ToArray());
            if (selectedComparatorIndex != currentComparatorIndex)
            {
                factQuery.type = GetQueryTypeFromInt(referenceFact.type, selectedComparatorIndex);
                factDirty = true;
            }

            if (factDirty)
            {
                property.boxedValue = factQuery;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 54.0f;
        }

        private static int GetComparatorIndex(FactType factType, FactQuery query)
        {
            return factType switch
            {
                FactType.Flag => query.type switch
                {
                    FactQueryType.Equal => 0,
                    FactQueryType.NotEqual => 1,
                    _ => 0
                },
                FactType.Numeric => query.type switch
                {
                    FactQueryType.Equal => 0,
                    FactQueryType.GreaterThan => 1,
                    FactQueryType.LessThan => 2,
                    _ => 0
                },
                _ => 0
            };
        }

        private static FactQueryType GetQueryTypeFromInt(FactType factType, int value)
        {
            return factType switch
            {
                FactType.Flag => value switch
                {
                    0 => FactQueryType.Equal,
                    1 => FactQueryType.NotEqual,
                    _ => FactQueryType.Equal
                },
                FactType.Numeric => value switch
                {
                    0 => FactQueryType.Equal,
                    1 => FactQueryType.GreaterThan,
                    2 => FactQueryType.LessThan,
                    _ => FactQueryType.Equal
                },
                _ => FactQueryType.Equal
            };
        }

        private static List<string> GetComparatorOptionsList(FactType type)
        {
            return type switch
            {
                FactType.Flag => _flagOptionNames,
                FactType.Numeric => _numericOptionNames,
                _ => new List<string>()
            };
        }
    }
}