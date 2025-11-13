using System;
using System.Collections.Generic;
using System.IO;
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

        public void ExecuteChange(FactChange change)
        {
            if (!_facts.TryGetValue(change.factName, out var fact))
            {
                Debug.LogWarningFormat("Fact change failed. '{0}' was not in the registry.", change.factName);
                return;
            }

            switch (change.type)
            {
                case FactChangeType.Assignment:
                {
                    // This only matters if we're running an assignment change here
                    // this would fail on other types, because there is no variant inside
                    if (fact.type != change.assignment.type)
                    {
                        Debug.LogWarningFormat("Fact type mismatch. Fact assignments expect the types to match. The assigment was of type '{0}', but '{1}', was '{2}'.", change.assignment.type, change.factName, fact.type);
                        return;
                    }

                    _facts[change.factName] = change.assignment;
                    break;
                }
                
                case FactChangeType.Increment:
                {
                    if (fact.type == FactType.Flag)
                    {
                        Debug.LogWarningFormat("Attempted to increment flag '{0}'. Flags can only be assigned to.", change.factName);
                        return;
                    }

                    _facts[change.factName] = new FactVariant(fact.Get<int>() + 1);
                    break;
                }
                
                case FactChangeType.Decrement:
                {
                    if (fact.type == FactType.Flag)
                    {
                        Debug.LogWarningFormat("Attempted to decrement flag '{0}'. Flags can only be assigned to.", change.factName);
                        return;
                    }

                    _facts[change.factName] = new FactVariant(fact.Get<int>() - 1);
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Generate a save file based on this registry, storing only values that
        // are different from the default values
        public List<string> GenerateSaveDifferences(FactRegistry registry)
        {
            var result = new List<string>();
            foreach (var registryFact in registry.facts)
            {
                if (!_facts.TryGetValue(registryFact.name, out var currentGameStateFact))
                {
                    Debug.LogWarningFormat("Attempted to save fact '{0}', but it was not in the registry", registryFact.name);
                    continue;
                }

                // While this will get caught by our next conditional
                // because FactVariant equality checks type first,
                // I want to make sure that we know if this exceptional
                // state is occurring. It *shouldn't*, but...
                if (currentGameStateFact.type != registryFact.data.type)
                {
                    Debug.LogWarningFormat("Attempted to save fact '{0}', but the types were mismatched.", registryFact.name);
                    continue;
                }

                if (currentGameStateFact == registryFact.data)
                {
                    continue;
                }

                switch (currentGameStateFact.type)
                {
                    case FactType.Flag:
                        result.Add($"\"{registryFact.name}\" {currentGameStateFact.Get<bool>().ToString().ToLower()}");
                        break;
                    
                    case FactType.Numeric:
                        result.Add($"\"{registryFact.name}\" {currentGameStateFact.Get<int>()}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }

        public void Save(string filepath = FactGameSave.DefaultSaveLocation)
        {
            var factRegistry = Resources.Load<FactRegistry>(FactRegistry.DefaultFactRegistryPath);
            if (!factRegistry)
            {
                Debug.LogWarning("Failed to save game. Could not find the fact registry file.");
                return;
            }

            var saveFile = new StreamWriter($"Assets/Resources/{filepath}.txt", false);
            foreach (var line in GenerateSaveDifferences(factRegistry))
            {
                saveFile.WriteLine(line);
            }
            saveFile.Close();
        }

        /**
         * <param name="flagName">Flag name to set</param>
         * <param name="value">Value to be assigned</param>
         * <returns>True if the flag was set successfully, false otherwise.</returns>
         */
        public bool WriteFlag(string flagName, bool value)
        {
            if (!_facts.TryGetValue(flagName, out var variant))
            {
                Debug.LogWarningFormat("FactState: Unable to write flag. '{0}' is not a valid fact name.", flagName);
                return false;
            }

            if (variant.type != FactType.Flag)
            {
                Debug.LogWarningFormat("FactState: Unable to write flag. '{0}' is not a flag.", flagName);
                return false;
            }

            _facts[flagName] = new FactVariant(value);
            return true;
        }

        public bool TryGetFlag(string flagName, out bool value)
        {
            if (!_facts.TryGetValue(flagName, out var variant))
            {
                Debug.LogWarningFormat("FactState: Unable to get flag. '{0}' is not a valid fact name.", flagName);
                value = false;
                return false;
            }

            if (variant.type != FactType.Flag)
            {
                Debug.LogWarningFormat("FactState: Unable to get flag. '{0}' is not a flag.", flagName);
                value = false;
                return false;
            }

            value = variant.Get<bool>();
            return true;
        }
    }
}