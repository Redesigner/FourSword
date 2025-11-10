using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Game.Facts
{
    [Serializable]
    public struct IntFact
    {
        public int value;
        public string name;
    }

    [Serializable]
    public struct BoolFact
    {
        public bool value;
        public string name;
    }
    
    [Serializable]
    public class FactGameSave
    {
        [SerializeField] public List<IntFact> numerics = new();
        [SerializeField] public List<BoolFact> flags = new();

        private const string DefaultSaveLocation = "FourSwordSave";

        public FactGameSave(string filepath = DefaultSaveLocation)
        {
            var saveFile = Resources.Load<TextAsset>(filepath);
            if (!saveFile)
            {
                Debug.LogWarningFormat("Failed to open save file '{0}'", filepath);
                return;
            }
                
            foreach (Match pair in Regex.Matches(saveFile.text, "\"(.*?)\"\\s(.*)"))
            {
                // The first group is actually our match here, so we want 3
                if (pair.Groups.Count != 3)
                {
                    continue;
                }

                switch (pair.Groups[2].Value)
                {
                    case "true":
                        flags.Add(new BoolFact
                        {
                            name = pair.Groups[1].Value,
                            value = true
                        });
                        continue;
                    
                    case "false":
                        flags.Add(new BoolFact
                        {
                            name = pair.Groups[1].Value,
                            value = false
                        });
                        continue;
                }

                if (int.TryParse(pair.Groups[2].Value, out var intValue))
                {
                    numerics.Add(new IntFact
                    {
                        name = pair.Groups[1].Value,
                        value = intValue
                    });
                }
            }
            
            Debug.LogFormat("Successfully loaded {0} facts from save file '{1}'", numerics.Count + flags.Count, filepath);
        }
    }
}