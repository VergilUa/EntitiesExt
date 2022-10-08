using Unity.Entities;
using Unity.Mathematics;

namespace HybridTransformations {
   /// <summary>
   /// Determines local scale of the entity
   /// </summary>
   [System.Serializable]
   public struct LocalScale : IComponentData {
      public float3 Value;

      public LocalScale(float3 value) { Value = value; }
   }
}