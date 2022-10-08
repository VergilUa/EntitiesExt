using System;
using Unity.Entities;

// ReSharper disable once CheckNamespace -> Trim it down
namespace HybridTransformations {
   /// <summary>
   /// Marks entity to ignore sync'ing between ECS & MonoBehaviour sides for one frame
   /// Useful if entity's position is to be set / initialized on ECS side for example
   /// </summary>
   [Serializable]
   public struct DontSyncOneFrame : IComponentData { }
}