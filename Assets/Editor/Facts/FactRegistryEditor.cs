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

    private VisualTreeAsset _entryVisualAsset;
    private FactRegistry _factRegistry;
    private VisualElement _inspectorRoot;
    
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
            _factRegistry.CreateFact("Empty", new Fact(true));
        };
        
        var minusButton = _inspectorRoot.Query<Button>("Remove");
        minusButton.First().clicked += () =>
        {
            _factRegistry.Reset();
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
        
        foreach (var factEntryPair in _factRegistry.facts)
        {
            _inspectorRoot.Add(CreateEntryVisual(factEntryPair.Key, factEntryPair.Value));
        }
    }

    private VisualElement CreateEntryVisual(string factName, Fact fact)
    {
        var entryVisual = _entryVisualAsset.CloneTree();
        entryVisual.AddToClassList("entry");
        var nameField = entryVisual.Query<TextField>("Name").First();
        var dropDown = entryVisual.Query<DropdownField>("Type").First();
        var toggle = entryVisual.Query<Toggle>("Bool").First();
        var numeric = entryVisual.Query<IntegerField>("Int").First();

        nameField.SetValueWithoutNotify(factName);

        nameField.isDelayed = true;
        nameField.RegisterValueChangedCallback(evt =>
        {
            _factRegistry.facts.Remove(factName);
            factName = evt.newValue;
            _factRegistry.CreateFact(factName, fact);
        });

        dropDown.choices = new List<string>{"Flag", "Numeric"};
        dropDown.index = (int)fact.type;
        dropDown.RegisterValueChangedCallback((evt) =>
        {
            _factRegistry.facts.Remove(factName);
            switch (evt.newValue)
            {
                case "Flag":
                    _factRegistry.CreateFact(factName, new Fact(true));
                    break;
                case "Numeric":
                    _factRegistry.CreateFact(factName, new Fact(0));
                    break;
            }
        });
        
        
        switch (fact.type)
        {
            case FactType.Flag:
                numeric.parent.Remove(numeric);
                toggle.value = fact.Get<bool>();
                toggle.RegisterValueChangedCallback(evt =>
                {
                    fact.Set(evt.newValue);
                });
                break;
            case FactType.Numeric:
                toggle.parent.Remove(toggle);
                numeric.value = fact.Get<int>();
                numeric.RegisterValueChangedCallback(evt =>
                {
                    fact.Set(evt.newValue);
                });
                break;
            default:
                break;
        }
        return entryVisual;
    }
}
