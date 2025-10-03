using Characters.Scripts;
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Target Perceived")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(
    name: "Target Perceived",
    message: "Agent has spotted [target]",
    category: "Events",
    id: "f5eac99414c38a466384fe34f5d1ded9")]
public sealed partial class TargetPerceived : EventChannel<PerceptionSourceComponent> { }

