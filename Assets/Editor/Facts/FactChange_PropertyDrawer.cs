using System;
using System.Collections.Generic;
using System.Linq;
using Game.Facts;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Editor.Facts
{
    [CustomPropertyDrawer(typeof(FactChange))]
    public class FactChangePropertyDrawer : PropertyDrawer
    {
        private static readonly List<string> FlagOptionNames = new() { "Equals" };
        private static readonly List<string> NumericOptionNames = new() { "Equals", "Plus", "Minus" };
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var factDirty = false;
            
            var factChange = (FactChange)property.boxedValue;
            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, position.width, 18.0f);
            EditorGUI.LabelField(labelRect, label);
            
            var factRegistry = Resources.Load<FactRegistry>(FactRegistry.DefaultFactRegistryPath);
            // Early out if we can't find our fact in the registry
            // because the other logic won't work without it
            if (!factRegistry.TryGetFact(factChange.factName, out var referenceFact))
            {
                // Default to the first fact
                if (factRegistry.facts.Count > 0)
                {
                    factChange.factName = factRegistry.facts.First().name;
                    property.boxedValue = factChange;
                }
                EditorGUI.EndProperty();
                return;
            }

            if (factChange.assignment == null || factChange.assignment.type != referenceFact.data.type)
            {
                factChange.assignment = referenceFact.data.type switch
                {
                    FactType.Flag => new FactVariant(true),
                    FactType.Numeric => new FactVariant(0),
                    _ => new FactVariant(false)
                };
            }

            // Name dropdown
            var nameRect = new Rect(position.x, position.y + 18.0f, position.width * 0.4f, position.height - 18.0f);
            var options = factRegistry.facts.Select(fact => fact.name).ToArray();
            var currentIndex = Array.FindIndex(options, option => option.Equals(factChange.factName));
            if (currentIndex == -1)
            {
                currentIndex = 0;
            }
            var selectedIndex = EditorGUI.Popup(nameRect, currentIndex, options);
            if (selectedIndex != currentIndex)
            {
                factChange.factName = options[selectedIndex];
                factDirty = true;
            }
            
            // Comparator dropdown
            var assignmentTypeRect = new Rect(position.x + position.width * 0.45f, position.y + 18.0f, position.width * 0.25f,
                position.height - 18.0f);
            var currentAssignmentType = GetAssignmentIndex(referenceFact.data.type, factChange);
            var selectedAssignmentType = EditorGUI.Popup(assignmentTypeRect, currentAssignmentType,
                GetComparatorOptionsList(referenceFact.data.type).ToArray());
            if (selectedAssignmentType != currentAssignmentType)
            {
                factChange.type = GetAssignmentTypeFromInt(referenceFact.data.type, selectedAssignmentType);
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
                    var oldValue = factChange.assignment.Get<bool>();
                    var newValue = GUI.Toggle(valueRect, oldValue, "");
                    if (newValue != oldValue)
                    {
                        factChange.assignment = new FactVariant(newValue);
                        factDirty = true;
                    }
                    break;
                }
                case FactType.Numeric:
                {
                    if (factChange.type == FactChangeType.Assignment)
                    {
                        var oldValue = factChange.assignment.Get<int>();
                        var newValue = EditorGUI.IntField(valueRect, factChange.assignment.Get<int>());
                        if (newValue != oldValue)
                        {
                            factChange.assignment = new FactVariant(newValue);
                            factDirty = true;
                        }
                    }

                    break;
                }
            }

            if (factDirty)
            {
                property.boxedValue = factChange;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 54.0f;
        }

        private static int GetAssignmentIndex(FactType factType, FactChange change)
        {
            return factType switch
            {
                FactType.Flag => 0,
                FactType.Numeric => change.type switch
                {
                    FactChangeType.Assignment => 0,
                    FactChangeType.Increment => 1,
                    FactChangeType.Decrement => 2,
                    _ => 0
                },
                _ => 0
            };
        }

        private static FactChangeType GetAssignmentTypeFromInt(FactType factType, int value)
        {
            return factType switch
            {
                FactType.Flag => FactChangeType.Assignment,
                FactType.Numeric => value switch
                {
                    0 => FactChangeType.Assignment,
                    1 => FactChangeType.Increment,
                    2 => FactChangeType.Decrement,
                    _ => FactChangeType.Assignment
                },
                _ => FactChangeType.Assignment
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