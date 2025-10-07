using System.Collections.Generic;
using Characters.Scripts;

namespace Game
{
    public class PerceptionSubsystem
    {
        public List<PerceptionSourceComponent> perceptionSources { get; private set; } = new();

        public void RegisterPerceptionSource(PerceptionSourceComponent source)
        {
            if (perceptionSources.Contains(source))
            {
                return;
            }
            
            perceptionSources.Add(source);
        }

        public void UnregisterPerceptionSource(PerceptionSourceComponent source)
        {
            perceptionSources.Remove(source);
        }
    }
}