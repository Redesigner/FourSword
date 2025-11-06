using System.Collections.Generic;
using UnityEngine;

namespace Game.Facts
{
    public class FactState
    {
        // The fact registry where values are "declare"
        private FactRegistry _underlyingFacts;

        private Dictionary<string, FactVariant> _facts;

        public void Initialize(FactRegistry registry, FactGameSave gameSave)
        {
            // convert the list into a dictionary for faster lookup
            foreach (var fact in registry.facts)
            {
                _facts.Add(fact.name, fact.data);
            }
            
            // Apply our game saved facts, type by type
            foreach (var savedFact in gameSave.flags)
            {
                if (!registry.TryGetFact(savedFact.name, out var fact))
                {
                    Debug.LogWarningFormat("Could not find fact '{0}'", savedFact.name);
                    continue;
                }

                if (fact.data.type != FactType.Flag)
                {
                    Debug.LogWarningFormat("Fact type mismatch: '{0}' is not a flag", savedFact.name);
                    continue;
                }

                fact.data = new FactVariant(savedFact.value);
            }
            foreach (var savedFact in gameSave.numerics)
            {
                if (!registry.TryGetFact(savedFact.name, out var fact))
                {
                    Debug.LogWarningFormat("Could not find fact '{0}'", savedFact.name);
                    continue;
                }

                if (fact.data.type != FactType.Numeric)
                {
                    Debug.LogWarningFormat("Fact type mismatch: '{0}' is not a numeric", savedFact.name);
                    continue;
                }

                fact.data = new FactVariant(savedFact.value);
            }
        }

        public bool RunQuery(FactQuery query)
        {
            return false;
        }
    }
}