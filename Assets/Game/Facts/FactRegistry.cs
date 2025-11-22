using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Facts
{
    [CreateAssetMenu(fileName = "FactRegistry", menuName = "FactRegistry", order = 0)]
    public class FactRegistry : ScriptableObject
    {
        public Action onItemsChanged;
        
        [SerializeField]
        public List<Fact> facts = new();

        public const string DefaultFactRegistryPath = "FourSwordFacts";

        public void CreateFact(string factName, bool data)
        {
            if (Contains(factName))
            {
                return;
            }
            
            facts.Add(new Fact(factName, data));
            onItemsChanged.Invoke();
        }
        
        public void CreateFact(string factName, int data)
        {
            if (Contains(factName))
            {
                return;
            }
            
            facts.Add(new Fact(factName, data));
            onItemsChanged.Invoke();
        }

        public void RemoveFact(string factName)
        {
            var index = facts.FindIndex(fact => fact.name == factName);
            if (index < 0)
            {
                return;
            }
            
            facts.RemoveAt(index);
            onItemsChanged.Invoke();
        }

        public bool TryGetFact(string factName, out Fact fact)
        {
            var index = facts.FindIndex(fact => fact.name == factName);
            if (index < 0)
            {
                fact = new Fact();
                return false;
            }

            fact = facts[index];
            return true;
        }

        public bool RenameFact(string oldName, string newName)
        {
            var index = facts.FindIndex(fact => fact.name == oldName);
            if (index < 0)
            {
                return false;
            }

            var newFact = facts[index];
            newFact.name = newName;

            facts[index] = newFact;
            return true;
        }

        public bool Contains(string factName)
        {
            return facts.Any(fact => fact.name == factName);
        }
    }
}