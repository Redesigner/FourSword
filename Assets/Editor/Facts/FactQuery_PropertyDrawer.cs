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
        private static readonly List<string> FlagOptionNames = new() { "Equal", "Not Equal" };
        private static readonly List<string> NumericOptionNames = new() { "Equal", "Greater", "Less" };
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var factDirty = false;
            
            var factQuery = (FactQuery)property.boxedValue;
            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, position.width, 18.0f);
            EditorGUI.LabelField(labelRect, label);
            
            var factRegistry = Resources.Load<FactRegistry>(FactRegistry.DefaultFactRegistryPath);
            // Early out if we can't find our fact in the registry
            // because the other logic won't work without it
            if (!factRegistry.TryGetFact(factQuery.factName, out var referenceFact))
            {
                // Default to the first fact
                if (factRegistry.facts.Count > 0)
                {
                    factQuery.factName = factRegistry.facts.First().name;
                    property.boxedValue = factQuery;
                }
                EditorGUI.EndProperty();
                return;
            }

            if (factQuery.comparison == null || factQuery.comparison.type != referenceFact.data.type)
            {
                factQuery.comparison = referenceFact.data.type switch
                {
                    FactType.Flag => new FactVariant(true),
                    FactType.Numeric => new FactVariant(0),
                    _ => new FactVariant(false)
                };
            }

            // Name dropdown
            var nameRect = new Rect(position.x, position.y + 18.0f, position.width * 0.4f, position.height - 18.0f);
            var options = factRegistry.facts.Select(fact => fact.name).ToArray();
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
            var comparatorRect = new Rect(position.x + position.width * 0.45f, position.y + 18.0f, position.width * 0.25f,
                position.height - 18.0f);
            var currentComparatorIndex = GetComparatorIndex(referenceFact.data.type, factQuery);
            var selectedComparatorIndex = EditorGUI.Popup(comparatorRect, currentComparatorIndex,
                GetComparatorOptionsList(referenceFact.data.type).ToArray());
            if (selectedComparatorIndex != currentComparatorIndex)
            {
                factQuery.type = GetQueryTypeFromInt(referenceFact.data.type, selectedComparatorIndex);
                factDirty = true;
            }
            
            // Value Field
            var valueRect = new Rect(position.x + position.width * 0.75f, position.y + 18.0f, position.width * 0.25f,
                18.0f);

            switch (referenceFact.data.type)
            {
                default:
                case FactType.Flag:
                {
                    var oldValue = factQuery.comparison.Get<bool>();
                    var newValue = GUI.Toggle(valueRect, oldValue, "");
                    if (newValue != oldValue)
                    {
                        factQuery.comparison = new FactVariant(newValue);
                        factDirty = true;
                    }
                    break;
                }
                case FactType.Numeric:
                {
                    var oldValue = factQuery.comparison.Get<int>();
                    var newValue = EditorGUI.IntField(valueRect, factQuery.comparison.Get<int>());
                    if (newValue != oldValue)
                    {
                        factQuery.comparison = new FactVariant(newValue);
                        factDirty = true;
                    }
                    break;
                }
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
                FactType.Flag => FlagOptionNames,
                FactType.Numeric => NumericOptionNames,
                _ => new List<string>()
            };
        }
    }
}