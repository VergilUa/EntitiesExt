using Unity.Entities;
using Unity.Mathematics;

namespace HybridTransformations {
   /// <summary>
   /// "Up" vector direction of the entity
   /// </summary>
   public struct UpDirection : IComponentData {
      public float3 Value;

      public UpDirection(float3 value) { Value = value; }
   }
}