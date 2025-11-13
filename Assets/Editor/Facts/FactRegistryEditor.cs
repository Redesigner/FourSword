using System;
using System.Collections.Generic;
using Game.Facts;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using TextField = UnityEngine.UIElements.TextField;
using Toggle = UnityEngine.UIElements.Toggle;

[CustomEditor(typeof(FactRegistry))]
public class FactRegistryEditor : UnityEditor.Editor
{
    private const string ResourceFilename = "editor/FactRegistryEditor";
    private const string EntryFilename = "editor/FactRegistryEntry";

    private const string SelectedCssClass = "selected";

    private VisualTreeAsset _entryVisualAsset;
    private FactRegistry _factRegistry;
    private VisualElement _inspectorRoot;
    private VisualElement _selectedEntry;
    private string _selectedKey;
    
    public override VisualElement CreateInspectorGUI()
    {
        _inspectorRoot = new VisualElement();
        var visualTree = Resources.Load(ResourceFilename) as VisualTreeAsset;
        if (!visualTree)
        {
            return null;
        }
        
        visualTree.CloneTree(_inspectorRoot);
        _inspectorRoot.styleSheets.Add(Resources.Load($"{ResourceFilename}-style") as StyleSheet);
        _entryVisualAsset = Resources.Load(EntryFilename) as VisualTreeAsset;
        _factRegistry = (FactRegistry)serializedObject.targetObject;
        
        var plusButton = _inspectorRoot.Query<Button>("Add");
        plusButton.First().clicked += () =>
        {
            _factRegistry.CreateFact("Empty", true);
        };
        
        var minusButton = _inspectorRoot.Query<Button>("Remove");
        minusButton.First().clicked += () =>
        {
            _factRegistry.RemoveFact(_selectedKey);
        };
        
        DrawFacts();
        _factRegistry.onItemsChanged += DrawFacts;
        return _inspectorRoot;
    }

    private void DrawFacts()
    {
        Debug.LogFormat("Inspector drawing {0} facts", _factRegistry.facts.Count);

        foreach (var query in _inspectorRoot.Query<VisualElement>(className: "entry").ToList())
        {
            _inspectorRoot.Remove(query);
        }
        
        foreach (var fact in _factRegistry.facts)
        {
            _inspectorRoot.Add(CreateEntryVisual(fact));
        }
    }

    private VisualElement CreateEntryVisual(Fact fact)
    {
        var entryVisual = _entryVisualAsset.CloneTree();
        entryVisual.AddToClassList("entry");
        var nameField = entryVisual.Query<TextField>("Name").First();
        var dropDown = entryVisual.Query<DropdownField>("Type").First();
        var toggle = entryVisual.Query<Toggle>("Bool").First();
        var numeric = entryVisual.Query<IntegerField>("Int").First();
        var box = nameField.parent;
        
        box.RegisterCallback<ClickEvent>(_ =>
        {
            SetSelectedEntry(box, fact.name);
        });

        nameField.SetValueWithoutNotify(fact.name);

        nameField.isDelayed = true;
        nameField.RegisterValueChangedCallback(evt =>
        {
            // The name shouldn't contain quotation marks
            if (evt.newValue.Contains("\""))
            {
                return;
            }
            _factRegistry.RenameFact(fact.name, evt.newValue);
            
            // If we renamed the currently selected entry, update our selected name to match
            if (_selectedEntry == box)
            {
                _selectedKey = fact.name;
            }
            EditorUtility.SetDirty(target);
        });

        dropDown.choices = new List<string>{"Flag", "Numeric"};
        dropDown.index = (int)fact.data.type;
        dropDown.RegisterValueChangedCallback((evt) =>
        {
            _factRegistry.RemoveFact(fact.name);
            switch (evt.newValue)
            {
                case "Flag":
                    _factRegistry.CreateFact(fact.name, true);
                    break;
                case "Numeric":
                    _factRegistry.CreateFact(fact.name, 0);
                    break;
            }
            EditorUtility.SetDirty(target);
        });
        
        
        switch (fact.data.type)
        {
            default:
            case FactType.Flag:
                numeric.parent.Remove(numeric);
                toggle.value = fact.data.Get<bool>();
                toggle.RegisterValueChangedCallback(evt =>
                {
                    fact.data.Set(evt.newValue);
                    EditorUtility.SetDirty(target);
                });
                break;
            case FactType.Numeric:
                toggle.parent.Remove(toggle);
                numeric.value = fact.data.Get<int>();
                numeric.RegisterValueChangedCallback(evt =>
                {
                    fact.data.Set(evt.newValue);
                    EditorUtility.SetDirty(target);
                });
                break;
        }
        return entryVisual;
    }

    private void SetSelectedEntry(VisualElement element, string key)
    {
        _selectedEntry?.RemoveFromClassList(SelectedCssClass);
        _selectedKey = key;
        _selectedEntry = element;
        element.AddToClassList(SelectedCssClass);
    }
}
