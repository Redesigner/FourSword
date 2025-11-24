using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Spotted Enemy")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Spotted Enemy", message: "Agent has spotted [enemy] with [controller]", category: "Events", id: "3d723e9ace263fde6451421cdda6ad91")]
public sealed partial class SpottedEnemy : EventChannel<GameObject, KinematicCharacterController> { }

