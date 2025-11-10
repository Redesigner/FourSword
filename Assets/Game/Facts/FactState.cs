using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Facts
{
    public class FactState
    {
        private readonly Dictionary<string, FactVariant> _facts = new();

        public void Initialize(FactRegistry registry, FactGameSave gameSave)
        {
            // convert the list into a dictionary for faster lookup
            foreach (var fact in registry.facts)
            {
                _facts.Add(fact.name, fact.data);
            }

            var loadedFactsCount = 0;
            // Apply our game saved facts, type by type
            foreach (var savedFact in gameSave.flags)
            {
                if (!_facts.TryGetValue(savedFact.name, out var fact))
                {
                    Debug.LogWarningFormat("Could not find fact '{0}'", savedFact.name);
                    continue;
                }

                if (fact.type != FactType.Flag)
                {
                    Debug.LogWarningFormat("Fact type mismatch: '{0}' is not a flag", savedFact.name);
                    continue;
                }

                ++loadedFactsCount;
                _facts[savedFact.name] = new FactVariant(savedFact.value);
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

                ++loadedFactsCount;
                _facts[savedFact.name] = new FactVariant(savedFact.value);
            }
            
            Debug.LogFormat("Successfully loaded '{0}' facts from the registry, and '{1}' facts from the save file.", registry.facts.Count, loadedFactsCount);
        }

        public bool RunQuery(FactQuery query)
        {
            if (!_facts.TryGetValue(query.factName, out var fact))
            {
                Debug.LogWarningFormat("Fact query failed. '{0}' was not in the registry.", query.factName);
                return false;
            }

            if (fact.type != query.comparison.type)
            {
                Debug.LogWarningFormat("Fact type mismatch. Query expected a '{0}', but '{1}', was '{2}'.", query.comparison.type, query.factName, fact.type);
                return false;
            }


            switch (query.comparison.type)
            {
                default:
                case FactType.Flag:
                {
                    var left = fact.Get<bool>();
                    var right = query.comparison.Get<bool>();
                    return query.type switch
                    {
                        FactQueryType.Equal => left == right,
                        FactQueryType.NotEqual => left != right,
                        _ => false
                    };
                }
                
                case FactType.Numeric:
                {
                    var left = fact.Get<int>();
                    var right = query.comparison.Get<int>();
                    return query.type switch
                    {
                        FactQueryType.Equal => left == right,
                        FactQueryType.NotEqual => left != right,
                        FactQueryType.LessThan => left < right,
                        FactQueryType.GreaterThan => left > right,
                        _ => false
                    };
                }
            }
        }
    }
}