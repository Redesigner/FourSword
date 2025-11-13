using UnityEditor;
using UnityEngine;

namespace Game.Facts
{
    [InitializeOnLoad]
    public class FactDefaultEditorList
    {
        public static FactRegistry factRegistry;
        
        static FactDefaultEditorList()
        {
            factRegistry = Resources.Load<FactRegistry>(FactRegistry.DefaultFactRegistryPath);
        }
    }
}