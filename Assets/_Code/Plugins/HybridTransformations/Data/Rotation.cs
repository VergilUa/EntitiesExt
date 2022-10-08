using System;
using Unity.Entities;
using Unity.Mathematics;

namespace HybridTransformations {
   /// <summary>
   /// World space rotation value
   /// </summary>
   [Serializable]
   public struct Rotation : IComponentData {
      public quaternion Value;

      public Rotation(quaternion rotation) { Value = rotation; }

      public float3 AsForward => math.normalizesafe(math.forward(Value));
   }
}